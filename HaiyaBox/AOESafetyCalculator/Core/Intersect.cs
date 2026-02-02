using System.Runtime.CompilerServices;

namespace AOESafetyCalculator.Core;

/// <summary>
/// 几何相交检测工具类
/// 提供射线与各种形状的相交检测，以及圆形与其他形状的相交检测
/// </summary>
[SkipLocalsInit]
public static class Intersect
{
    #region 射线与圆形相交

    /// <summary>
    /// 计算射线与圆形的交点（使用相对于圆心的偏移）
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float RayCircle(in WDir rayOriginOffset, in WDir rayDir, float circleRadius)
    {
        var halfB = rayOriginOffset.Dot(rayDir);
        var halfDSq = halfB * halfB - rayOriginOffset.LengthSq() + circleRadius * circleRadius;
        if (halfDSq < 0f)
            return float.MaxValue;
        var t = -halfB + MathF.Sqrt(halfDSq);
        return t >= 0f ? t : float.MaxValue;
    }

    /// <summary>
    /// 计算射线与圆形的交点（使用世界坐标）
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float RayCircle(in WPos rayOrigin, in WDir rayDir, in WPos circleCenter, float circleRadius)
        => RayCircle(rayOrigin - circleCenter, rayDir, circleRadius);

    /// <summary>
    /// 检测射线段是否与圆形相交
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool RayCircle(in WDir rayOriginOffset, in WDir rayDir, float circleRadius, float maxDist)
    {
        var t = (-rayOriginOffset).Dot(rayDir);
        var tClamped = Math.Max(0f, Math.Min(maxDist, t));
        var closest = rayOriginOffset + rayDir * tClamped;
        return closest.LengthSq() <= circleRadius * circleRadius;
    }

    #endregion

    #region 射线与矩形相交

    /// <summary>
    /// 计算射线与轴对齐包围盒（AABB）的交点
    /// </summary>
    public static float RayAABB(in WDir rayOriginOffset, in WDir rayDir, float halfWidth, float halfHeight)
    {
        var invX = 1.0f / rayDir.X;
        var invZ = 1.0f / rayDir.Z;
        var tmin = -float.Epsilon;
        var tmax = float.MaxValue;

        var offsetX = rayOriginOffset.X;
        var offsetZ = rayOriginOffset.Z;
        var tx1 = (-halfWidth - offsetX) * invX;
        var tx2 = (+halfWidth - offsetX) * invX;
        var tz1 = (-halfHeight - offsetZ) * invZ;
        var tz2 = (+halfHeight - offsetZ) * invZ;

        static float min(float x, float y) => x < y ? x : y;
        static float max(float x, float y) => x > y ? x : y;

        tmin = min(max(tx1, tmin), max(tx2, tmin));
        tmax = max(min(tx1, tmax), min(tx2, tmax));
        tmin = min(max(tz1, tmin), max(tz2, tmin));
        tmax = max(min(tz1, tmax), min(tz2, tmax));

        return tmin > tmax ? float.MaxValue : tmin >= 0f ? tmin : tmax >= 0f ? tmax : float.MaxValue;
    }

    /// <summary>
    /// 计算射线与轴对齐包围盒的交点（使用世界坐标）
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float RayAABB(in WPos rayOrigin, in WDir rayDir, in WPos boxCenter, float halfWidth, float halfHeight)
        => RayAABB(rayOrigin - boxCenter, rayDir, halfWidth, halfHeight);

    /// <summary>
    /// 计算射线与旋转矩形的交点（使用相对于矩形中心的偏移）
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float RayRect(in WDir rayOriginOffset, in WDir rayDir, in WDir rectRotation, float halfWidth, float halfHeight)
    {
        var rectX = rectRotation.OrthoL();
        return RayAABB(new(rayOriginOffset.Dot(rectX), rayOriginOffset.Dot(rectRotation)),
                      new(rayDir.Dot(rectX), rayDir.Dot(rectRotation)), halfWidth, halfHeight);
    }

    /// <summary>
    /// 计算射线与旋转矩形的交点（使用世界坐标）
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float RayRect(in WPos rayOrigin, in WDir rayDir, in WPos rectCenter, in WDir rectRotation, float halfWidth, float halfHeight)
        => RayRect(rayOrigin - rectCenter, rayDir, rectRotation, halfWidth, halfHeight);

    #endregion

    #region 射线与直线/线段相交

    /// <summary>
    /// 计算射线与无限直线的交点
    /// </summary>
    public static float RayLine(in WDir rayOriginOffset, in WDir rayDir, in WDir line)
    {
        var n = line.OrthoL();
        var ddn = rayDir.Dot(n);
        var odn = rayOriginOffset.Dot(n);
        if (ddn == 0)
            return odn == 0 ? 0 : float.MaxValue;
        var t = -odn / ddn;
        return t >= 0 ? t : float.MaxValue;
    }

    /// <summary>
    /// 计算射线与无限直线的交点（使用世界坐标）
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float RayLine(in WPos rayOrigin, in WDir rayDir, in WPos lineOrigin, in WDir line)
        => RayLine(rayOrigin - lineOrigin, rayDir, line);

    /// <summary>
    /// 计算射线与线段的交点（使用相对于某参考点的偏移）
    /// </summary>
    public static float RaySegment(in WDir rayOriginOffset, in WDir rayDir, in WDir oa, in WDir ob)
    {
        var lineDir = ob - oa;
        var t = RayLine(rayOriginOffset - oa, rayDir, lineDir);
        if (t == float.MaxValue)
            return float.MaxValue;

        var p = rayOriginOffset + t * rayDir;
        var u = lineDir.Dot(p - oa);
        return u >= 0f && u <= lineDir.LengthSq() ? t : float.MaxValue;
    }

    /// <summary>
    /// 计算射线与线段的交点（使用世界坐标）
    /// </summary>
    public static float RaySegment(in WPos rayOrigin, in WDir rayDir, in WPos vertexA, in WPos vertexB)
    {
        var lineDir = vertexB - vertexA;
        var t = RayLine(rayOrigin - vertexA, rayDir, lineDir);
        if (t == float.MaxValue)
            return float.MaxValue;

        var p = rayOrigin + t * rayDir;
        var u = lineDir.Dot(p - vertexA);
        return u >= 0f && u <= lineDir.LengthSq() ? t : float.MaxValue;
    }

    #endregion

    #region 圆形与其他形状相交

    /// <summary>
    /// 检测两个圆形是否相交或接触
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CircleCircle(in WDir circleOffset, float circleRadius, float radius)
    {
        var rsum = circleRadius + radius;
        return circleOffset.LengthSq() <= rsum * rsum;
    }

    /// <summary>
    /// 检测两个圆形是否相交或接触（使用世界坐标）
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CircleCircle(in WPos circleCenter, float circleRadius, in WPos center, float radius)
        => CircleCircle(circleCenter - center, circleRadius, radius);

    /// <summary>
    /// 检测圆形与扇形是否相交
    /// </summary>
    public static bool CircleCone(in WDir circleOffset, float circleRadius, float coneRadius, in WDir coneDir, Angle halfAngle)
    {
        var lsq = circleOffset.LengthSq();
        var rsq = circleRadius * circleRadius;

        if (lsq <= rsq)
            return true;

        var rsum = circleRadius + coneRadius;
        if (lsq > rsum * rsum)
            return false;

        if (halfAngle.Rad >= MathF.PI)
            return true;

        var correctSide = circleOffset.Dot(coneDir) > 0;
        var normal = coneDir.OrthoL();
        var sin = halfAngle.Sin();
        var distFromAxis = circleOffset.Dot(normal);

        var originInCone = (halfAngle.Rad - Angle.HalfPi) switch
        {
            < 0 => correctSide && distFromAxis * distFromAxis <= lsq * sin * sin,
            > 0 => correctSide || distFromAxis * distFromAxis >= lsq * sin * sin,
            _ => correctSide,
        };
        if (originInCone)
            return true;

        if (distFromAxis < 0)
            normal = -normal;

        var side = coneDir * halfAngle.Cos() + normal * sin;
        var distFromSide = Math.Abs(circleOffset.Cross(side));
        if (distFromSide > circleRadius)
            return false;
        var distAlongSide = circleOffset.Dot(side);
        if (distAlongSide < 0)
            return false;
        if (distAlongSide <= coneRadius)
            return true;

        var corner = side * coneRadius;
        return (circleOffset - corner).LengthSq() <= rsq;
    }

    /// <summary>
    /// 检测圆形与扇形是否相交（使用世界坐标）
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CircleCone(in WPos circleCenter, float circleRadius, in WPos coneCenter, float coneRadius, in WDir coneDir, Angle halfAngle)
        => CircleCone(circleCenter - coneCenter, circleRadius, coneRadius, coneDir, halfAngle);

    /// <summary>
    /// 检测圆形与轴对齐矩形是否相交
    /// </summary>
    public static bool CircleAARect(WDir circleOffset, float circleRadius, float halfExtentX, float halfExtentZ)
    {
        circleOffset = circleOffset.Abs();
        var cornerOffset = circleOffset - new WDir(halfExtentX, halfExtentZ);

        if (cornerOffset.X > circleRadius || cornerOffset.Z > circleRadius)
            return false;

        if (cornerOffset.X <= 0 || cornerOffset.Z <= 0)
            return true;

        return cornerOffset.LengthSq() <= circleRadius * circleRadius;
    }

    /// <summary>
    /// 检测圆形与轴对齐矩形是否相交（使用世界坐标）
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CircleAARect(in WPos circleCenter, float circleRadius, in WPos rectCenter, float halfExtentX, float halfExtentZ)
        => CircleAARect(circleCenter - rectCenter, circleRadius, halfExtentX, halfExtentZ);

    /// <summary>
    /// 检测圆形与旋转矩形是否相交（使用相对于矩形中心的偏移）
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CircleRect(in WDir circleOffset, float circleRadius, in WDir rectZDir, float halfExtentX, float halfExtentZ)
        => CircleAARect(circleOffset.Rotate(rectZDir.MirrorX()), circleRadius, halfExtentX, halfExtentZ);

    /// <summary>
    /// 检测圆形与旋转矩形是否相交（使用世界坐标）
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CircleRect(in WPos circleCenter, float circleRadius, in WPos rectCenter, in WDir rectZDir, float halfExtentX, float halfExtentZ)
        => CircleRect(circleCenter - rectCenter, circleRadius, rectZDir, halfExtentX, halfExtentZ);

    /// <summary>
    /// 检测圆形与扇形环（Donut Sector）是否相交
    /// </summary>
    public static bool CircleDonutSector(in WDir circleOffset, float circleRadius, float innerRadius, float outerRadius, WDir sectorDir, Angle halfAngle)
    {
        var distSq = circleOffset.LengthSq();
        var maxR = outerRadius + circleRadius;
        var minR = Math.Max(0, innerRadius - circleRadius);

        if (distSq > maxR * maxR || distSq < minR * minR)
            return false;

        if (halfAngle.Rad >= MathF.PI)
            return true;

        sectorDir = sectorDir.Normalized();

        var angleToCenter = Angle.Acos(Math.Clamp(circleOffset.Normalized().Dot(sectorDir), -1f, 1f));
        if (angleToCenter <= halfAngle)
            return true;

        var sideDirL = sectorDir.Rotate(halfAngle);
        var sideDirR = sectorDir.Rotate(-halfAngle);

        static float DistToRay(WDir dir, WDir pt) => Math.Abs(pt.Cross(dir));

        var dL = DistToRay(sideDirL, circleOffset);
        var dR = DistToRay(sideDirR, circleOffset);
        var projL = circleOffset.Dot(sideDirL);
        var projR = circleOffset.Dot(sideDirR);

        if (projL >= 0 && projL <= outerRadius && dL <= circleRadius ||
            projR >= 0 && projR <= outerRadius && dR <= circleRadius)
            return true;

        var cornerL = sideDirL * outerRadius;
        var cornerR = sideDirR * outerRadius;

        return (circleOffset - cornerL).LengthSq() <= circleRadius * circleRadius ||
               (circleOffset - cornerR).LengthSq() <= circleRadius * circleRadius;
    }

    /// <summary>
    /// 检测圆形与扇形环是否相交（使用世界坐标）
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CircleDonutSector(in WPos circleCenter, float circleRadius, in WPos sectorCenter, float innerRadius, float outerRadius, in WDir sectorDir, Angle halfAngle)
        => CircleDonutSector(circleCenter - sectorCenter, circleRadius, innerRadius, outerRadius, sectorDir, halfAngle);

    #endregion

    #region 射线与圆形交点角度计算

    /// <summary>
    /// 计算射线与圆形的交点角度（以度为单位）
    /// </summary>
    public static int RayCircleAnglesDeg(WPos centerC, float radius, WPos rayOriginO, WDir rayDirD, out float degEnter, out float degExit)
    {
        degEnter = degExit = default;

        var F = rayOriginO - centerC;

        var A = rayDirD.Dot(rayDirD);
        var B = 2f * F.Dot(rayDirD);
        var C = F.Dot(F) - radius * radius;

        var disc = B * B - 4f * A * C;
        if (disc < 0f)
            return 0;

        var s = MathF.Sqrt(Math.Max(0f, disc));
        var inv2A = 0.5f / A;
        var t0 = (-B - s) * inv2A;
        var t1 = (-B + s) * inv2A;

        var t0Ok = t0 >= 0f;
        var t1Ok = t1 >= 0f;

        if (!t0Ok && !t1Ok)
            return 0;

        static float HitDeg(WPos C, WPos O, WDir D, float t)
        {
            var p = O + t * D;
            var v = p - C;
            var a = Angle.Atan2(v.X, v.Z).Normalized();
            return a.Deg;
        }

        if (disc == 0f)
        {
            var t = Math.Max(t0, t1);
            if (t < 0f)
                return default;
            degEnter = HitDeg(centerC, rayOriginO, rayDirD, t);
            return 1;
        }

        if (t0Ok && t1Ok)
        {
            degEnter = HitDeg(centerC, rayOriginO, rayDirD, MathF.Min(t0, t1));
            degExit = HitDeg(centerC, rayOriginO, rayDirD, MathF.Max(t0, t1));
            return 2;
        }
        else
        {
            var t = Math.Max(t0, t1);
            degExit = HitDeg(centerC, rayOriginO, rayDirD, t);
            return 1;
        }
    }

    #endregion
}
