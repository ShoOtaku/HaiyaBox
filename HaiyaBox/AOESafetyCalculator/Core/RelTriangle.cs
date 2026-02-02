using System.Runtime.CompilerServices;

namespace AOESafetyCalculator.Core;

/// <summary>
/// 相对三角形（使用相对坐标的三角形）
/// </summary>
/// <remarks>
/// 三角形的三个顶点使用相对坐标（WDir）表示，通常相对于某个中心点
/// </remarks>
[SkipLocalsInit]
public readonly struct RelTriangle(WDir a, WDir b, WDir c)
{
    /// <summary>顶点 A</summary>
    public readonly WDir A = a;

    /// <summary>顶点 B</summary>
    public readonly WDir B = b;

    /// <summary>顶点 C</summary>
    public readonly WDir C = c;
}
