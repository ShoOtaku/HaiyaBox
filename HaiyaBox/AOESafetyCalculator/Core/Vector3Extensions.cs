using System.Numerics;

namespace AOESafetyCalculator.Core;

/// <summary>
/// Vector3 扩展方法，提供与 WPos/WDir 的便捷转换
/// </summary>
public static class Vector3Extensions
{
    /// <summary>
    /// 转换为世界位置（忽略 Y 坐标）
    /// </summary>
    /// <param name="v">三维向量</param>
    /// <returns>世界位置（X, Z）</returns>
    public static WPos ToWPos(this Vector3 v) => WPos.FromVec3(v);

    /// <summary>
    /// 转换为世界方向（忽略 Y 坐标）
    /// </summary>
    /// <param name="v">三维向量</param>
    /// <returns>世界方向（X, Z）</returns>
    public static WDir ToWDir(this Vector3 v) => WDir.FromVec3(v);
}
