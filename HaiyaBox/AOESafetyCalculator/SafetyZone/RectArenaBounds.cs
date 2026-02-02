using System.Runtime.CompilerServices;
using AOESafetyCalculator.Core;

namespace AOESafetyCalculator.SafetyZone;

/// <summary>
/// 矩形场地边界
/// </summary>
[SkipLocalsInit]
public sealed class RectArenaBounds : ArenaBounds
{
    private readonly float centerX, centerZ;
    private readonly float dirX, dirZ;  // 方向向量（单位向量）
    private readonly float halfWidth, halfLength;

    /// <summary>
    /// 创建矩形场地边界
    /// </summary>
    /// <param name="center">矩形中心</param>
    /// <param name="direction">矩形朝向（长边方向）</param>
    /// <param name="halfWidth">半宽（短边的一半）</param>
    /// <param name="halfLength">半长（长边的一半）</param>
    public RectArenaBounds(WPos center, WDir direction, float halfWidth, float halfLength)
    {
        centerX = center.X;
        centerZ = center.Z;

        // 归一化方向向量（零向量时使用默认方向）
        var len = MathF.Sqrt(direction.X * direction.X + direction.Z * direction.Z);
        if (len <= 0f)
        {
            dirX = 1f;
            dirZ = 0f;
        }
        else
        {
            dirX = direction.X / len;
            dirZ = direction.Z / len;
        }

        this.halfWidth = halfWidth;
        this.halfLength = halfLength;
    }

    public override WPos Center => new(centerX, centerZ);

    public override float ApproximateRadius => MathF.Sqrt(halfWidth * halfWidth + halfLength * halfLength);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(WPos position)
    {
        // 转换到矩形局部坐标系
        var dx = position.X - centerX;
        var dz = position.Z - centerZ;

        var localX = dx * dirX + dz * dirZ;      // 沿长边方向
        var localZ = -dx * dirZ + dz * dirX;     // 沿短边方向

        return MathF.Abs(localX) <= halfLength && MathF.Abs(localZ) <= halfWidth;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float DistanceToBorder(WPos position)
    {
        // 转换到矩形局部坐标系
        var dx = position.X - centerX;
        var dz = position.Z - centerZ;

        var localX = dx * dirX + dz * dirZ;
        var localZ = -dx * dirZ + dz * dirX;

        // 计算到边界的距离
        var distX = halfLength - MathF.Abs(localX);
        var distZ = halfWidth - MathF.Abs(localZ);

        // 返回到最近边界的距离
        if (distX > 0 && distZ > 0)
        {
            // 在矩形内部，返回到最近边的距离
            return MathF.Min(distX, distZ);
        }
        else if (distX > 0)
        {
            // 在矩形外部，Z方向超出
            return distZ;
        }
        else if (distZ > 0)
        {
            // 在矩形外部，X方向超出
            return distX;
        }
        else
        {
            // 在矩形外部，两个方向都超出（角落外）
            return -MathF.Sqrt(distX * distX + distZ * distZ);
        }
    }
}
