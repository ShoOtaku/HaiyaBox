using System.Runtime.CompilerServices;
using AOESafetyCalculator.Core;

namespace AOESafetyCalculator.DistanceField;

/// <summary>
/// 形状距离场抽象基类
/// </summary>
/// <remarks>
/// 距离场定义：
/// - 距离为正值：点在形状外部
/// - 距离为负值：点在形状内部
/// - 距离为零：点在形状边界上
///
/// 布尔运算：
/// - 并集（Union）：使用 min 操作
/// - 交集（Intersection）：使用 max 操作
///
/// 注意：某些特殊实现（如击退相关）可能不返回真实距离，
/// 而是返回 0（禁止区域）或 1（允许区域）。
/// 建议添加 1 码的安全边距以覆盖网格中的所有点。
/// </remarks>
[SkipLocalsInit]
public abstract class ShapeDistance
{
    /// <summary>浮点数比较的精度阈值</summary>
    public const float Epsilon = 1e-5f;

    /// <summary>
    /// 计算点到形状边界的距离
    /// </summary>
    /// <param name="p">要计算距离的点</param>
    /// <returns>
    /// 距离值：
    /// - 正值：点在形状外部
    /// - 负值：点在形状内部
    /// - 零：点在形状边界上
    /// </returns>
    public abstract float Distance(in WPos p);

    /// <summary>
    /// 检查一行是否与形状相交
    /// </summary>
    /// <param name="rowStart">行的起始位置</param>
    /// <param name="dx">行的方向向量</param>
    /// <param name="width">行的宽度</param>
    /// <param name="cushion">缓冲距离（默认为 0）</param>
    /// <returns>true 表示相交，false 表示不相交</returns>
    /// <remarks>
    /// 默认实现返回 true（保守策略）
    /// 子类可以重写此方法以提供更精确的相交检测
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;

    /// <summary>
    /// 检查点是否在形状内
    /// </summary>
    /// <param name="p">要检查的点</param>
    /// <returns>true 表示点在形状内或边界上，false 表示在形状外</returns>
    /// <remarks>
    /// 默认实现：距离 <= 0 表示在形状内
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual bool Contains(in WPos p) => Distance(p) <= 0f;

    /// <summary>
    /// 检查浮点数是否接近零
    /// </summary>
    /// <param name="v">要检查的值</param>
    /// <returns>true 表示值在 [-Epsilon, Epsilon] 范围内</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool NearlyZero(float v) => Math.Abs(v) <= Epsilon;
}
