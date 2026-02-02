using System.Runtime.CompilerServices;
using AOESafetyCalculator.Core;

namespace AOESafetyCalculator.SafetyZone;

/// <summary>
/// 场地边界抽象类
/// </summary>
/// <remarks>
/// 定义场地的可活动范围，安全点必须在场地内
/// </remarks>
[SkipLocalsInit]
public abstract class ArenaBounds
{
    /// <summary>
    /// 检查位置是否在场地内
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract bool Contains(WPos position);

    /// <summary>
    /// 计算位置到场地边界的距离
    /// </summary>
    /// <returns>正值表示在场地内，负值表示在场地外</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract float DistanceToBorder(WPos position);

    /// <summary>
    /// 获取场地中心点（用于候选点生成）
    /// </summary>
    public abstract WPos Center { get; }

    /// <summary>
    /// 获取场地的近似半径（用于候选点生成）
    /// </summary>
    public abstract float ApproximateRadius { get; }
}

/// <summary>
/// 圆形场地边界
/// </summary>
[SkipLocalsInit]
public sealed class CircleArenaBounds : ArenaBounds
{
    private readonly float centerX, centerZ, radius;

    public CircleArenaBounds(WPos center, float radius)
    {
        centerX = center.X;
        centerZ = center.Z;
        this.radius = radius;
    }

    public override WPos Center => new(centerX, centerZ);

    public override float ApproximateRadius => radius;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(WPos position)
    {
        var dx = position.X - centerX;
        var dz = position.Z - centerZ;
        return dx * dx + dz * dz <= radius * radius;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float DistanceToBorder(WPos position)
    {
        var dx = position.X - centerX;
        var dz = position.Z - centerZ;
        var distToCenter = MathF.Sqrt(dx * dx + dz * dz);
        return radius - distToCenter;
    }
}
