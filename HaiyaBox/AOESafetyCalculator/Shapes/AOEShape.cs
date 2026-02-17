using System.Runtime.CompilerServices;
using AOESafetyCalculator.Core;
using AOESafetyCalculator.DistanceField;

namespace AOESafetyCalculator.Shapes;

/// <summary>
/// AOE（Area of Effect，范围效果）形状定义的抽象基类
/// </summary>
/// <remarks>
/// 定义各种 AOE 攻击的形状类型，用于：
/// 1. 检测位置是否在危险区域内（Check 方法）
/// 2. 计算到危险区域的距离（Distance 方法）
///
/// 支持的形状类型：
/// - AOEShapeCircle: 圆形 AOE
/// - AOEShapeCone: 扇形 AOE
/// - AOEShapeDonut: 环形 AOE
/// - AOEShapeDonutSector: 扇形环 AOE
/// - AOEShapeRect: 矩形 AOE
/// - AOEShapeCross: 十字形 AOE
/// - AOEShapeTriCone: 三角形扇形 AOE
/// - AOEShapeCapsule: 胶囊形 AOE
/// - AOEShapeArcCapsule: 弧形胶囊 AOE
/// </remarks>
/// <param name="invertForbiddenZone">是否反转禁止区域（true 表示形状内部是安全区，外部是危险区）</param>
[SkipLocalsInit]
public abstract class AOEShape(bool invertForbiddenZone)
{
    /// <summary>
    /// 是否反转禁止区域
    /// </summary>
    /// <remarks>
    /// false（默认）: 形状内部是危险区域，需要躲避
    /// true: 形状内部是安全区域，需要站在里面（如安全圈机制）
    /// </remarks>
    public bool InvertForbiddenZone = invertForbiddenZone;

    /// <summary>
    /// 检测指定位置是否在 AOE 形状内
    /// </summary>
    /// <param name="position">要检测的位置（世界坐标）</param>
    /// <param name="origin">AOE 的原点位置</param>
    /// <returns>true 表示位置在形状内，false 表示在形状外</returns>
    public abstract bool Check(WPos position, WPos origin);

    /// <summary>
    /// 获取 AOE 形状的距离场计算器（形状内部为负，外部为正）
    /// </summary>
    /// <param name="origin">AOE 的原点位置</param>
    /// <returns>距离场计算器</returns>
    public abstract ShapeDistance Distance(WPos origin);

    /// <summary>
    /// 获取反转的距离场计算器（形状内部为正，外部为负）
    /// </summary>
    /// <param name="origin">AOE 的原点位置</param>
    /// <returns>反转的距离场计算器</returns>
    public abstract ShapeDistance InvertedDistance(WPos origin);
}

/// <summary>
/// 扇形 AOE 形状（Cone，锥形）
/// </summary>
/// <remarks>
/// 表示以原点为圆心、指定半径和角度的扇形区域
/// 常见于 Boss 的吐息攻击、扇形顺劈等技能
/// </remarks>
/// <param name="radius">扇形半径（码）</param>
/// <param name="halfAngle">扇形半角（总角度的一半）</param>
/// <param name="direction">扇形中心方向。角度约定：0°=北，90°=东，180°=南，270°=西</param>
/// <param name="invertForbiddenZone">是否反转禁止区域</param>
[SkipLocalsInit]
public sealed class AOEShapeCone(float radius, Angle halfAngle, Angle direction = default, bool invertForbiddenZone = false) : AOEShape(invertForbiddenZone)
{
    /// <summary>扇形半径（码）</summary>
    public readonly float Radius = radius;

    /// <summary>扇形半角（总角度 = HalfAngle × 2）</summary>
    public readonly Angle HalfAngle = halfAngle;

    /// <summary>扇形中心方向</summary>
    public readonly Angle Direction = direction;

    /// <summary>返回形状的字符串描述</summary>
    public override string ToString() => $"Cone: r={Radius:f3}, angle={HalfAngle * 2f}, dir={Direction}, ifz={InvertForbiddenZone}";

    /// <summary>检测位置是否在扇形内</summary>
    public override bool Check(WPos position, WPos origin)
        => position.InCircleCone(origin, Radius, Direction, HalfAngle);

    /// <summary>获取距离计算器</summary>
    public override ShapeDistance Distance(WPos origin)
    {
        return InvertForbiddenZone
            ? new SDInvertedCone(origin, Radius, Direction, HalfAngle)
            : new SDCone(origin, Radius, Direction, HalfAngle);
    }

    /// <summary>获取反转的距离计算器</summary>
    public override ShapeDistance InvertedDistance(WPos origin)
    {
        return InvertForbiddenZone
            ? new SDCone(origin, Radius, Direction, HalfAngle)
            : new SDInvertedCone(origin, Radius, Direction, HalfAngle);
    }
}

/// <summary>
/// 圆形 AOE 形状（Circle）
/// </summary>
/// <remarks>
/// 最基本的 AOE 形状，表示以原点为圆心的圆形区域
/// 常见于范围攻击、地面 AOE 等技能
/// </remarks>
/// <param name="radius">圆形半径（码）</param>
/// <param name="invertForbiddenZone">是否反转禁止区域</param>
[SkipLocalsInit]
public sealed class AOEShapeCircle(float radius, bool invertForbiddenZone = false) : AOEShape(invertForbiddenZone)
{
    /// <summary>圆形半径（码）</summary>
    public readonly float Radius = radius;

    /// <summary>返回形状的字符串描述</summary>
    public override string ToString() => $"Circle: r={Radius:f3}, ifz={InvertForbiddenZone}";

    /// <summary>检测位置是否在圆形内</summary>
    public override bool Check(WPos position, WPos origin)
        => position.InCircle(origin, Radius);

    /// <summary>获取距离计算器</summary>
    public override ShapeDistance Distance(WPos origin)
    {
        return InvertForbiddenZone
            ? new SDInvertedCircle(origin, Radius)
            : new SDCircle(origin, Radius);
    }

    /// <summary>获取反转的距离计算器</summary>
    public override ShapeDistance InvertedDistance(WPos origin)
    {
        return InvertForbiddenZone
            ? new SDCircle(origin, Radius)
            : new SDInvertedCircle(origin, Radius);
    }
}

/// <summary>
/// 环形 AOE 形状（Donut，甜甜圈）
/// </summary>
/// <remarks>
/// 表示内外两个同心圆之间的环形区域
/// 常见于月环攻击（需要站在内圈或外圈躲避）
/// </remarks>
/// <param name="innerRadius">内圆半径（码）- 安全区域</param>
/// <param name="outerRadius">外圆半径（码）- 危险区域边界</param>
/// <param name="invertForbiddenZone">是否反转禁止区域</param>
[SkipLocalsInit]
public sealed class AOEShapeDonut(float innerRadius, float outerRadius, bool invertForbiddenZone = false) : AOEShape(invertForbiddenZone)
{
    /// <summary>内圆半径（码）- 内圈是安全区域</summary>
    public readonly float InnerRadius = innerRadius;

    /// <summary>外圆半径（码）- 外圈边界</summary>
    public readonly float OuterRadius = outerRadius;

    /// <summary>返回形状的字符串描述</summary>
    public override string ToString() => $"Donut: r={InnerRadius:f3}-{OuterRadius:f3}, ifz={InvertForbiddenZone}";

    /// <summary>检测位置是否在环形内</summary>
    public override bool Check(WPos position, WPos origin)
        => position.InDonut(origin, InnerRadius, OuterRadius);

    /// <summary>获取距离计算器</summary>
    public override ShapeDistance Distance(WPos origin)
    {
        return InvertForbiddenZone
            ? new SDInvertedDonut(origin, InnerRadius, OuterRadius)
            : new SDDonut(origin, InnerRadius, OuterRadius);
    }

    /// <summary>获取反转的距离计算器</summary>
    public override ShapeDistance InvertedDistance(WPos origin)
    {
        return InvertForbiddenZone
            ? new SDDonut(origin, InnerRadius, OuterRadius)
            : new SDInvertedDonut(origin, InnerRadius, OuterRadius);
    }
}

/// <summary>
/// 扇形环 AOE 形状（Donut Sector，环形扇形）
/// </summary>
/// <remarks>
/// 环形和扇形的组合，表示环形区域的一部分扇形
/// 常见于某些复杂的 Boss 机制
/// </remarks>
/// <param name="innerRadius">内圆半径（码）</param>
/// <param name="outerRadius">外圆半径（码）</param>
/// <param name="halfAngle">扇形半角</param>
/// <param name="direction">扇形中心方向。角度约定：0°=北，90°=东，180°=南，270°=西</param>
/// <param name="invertForbiddenZone">是否反转禁止区域</param>
[SkipLocalsInit]
public sealed class AOEShapeDonutSector(float innerRadius, float outerRadius, Angle halfAngle, Angle direction = default, bool invertForbiddenZone = false) : AOEShape(invertForbiddenZone)
{
    /// <summary>内圆半径（码）</summary>
    public readonly float InnerRadius = innerRadius;

    /// <summary>外圆半径（码）</summary>
    public readonly float OuterRadius = outerRadius;

    /// <summary>扇形半角（总角度的一半）</summary>
    public readonly Angle HalfAngle = halfAngle;

    /// <summary>扇形中心方向</summary>
    public readonly Angle Direction = direction;

    /// <summary>返回形状的字符串描述</summary>
    public override string ToString() => $"Donut sector: r={InnerRadius:f3}-{OuterRadius:f3}, angle={HalfAngle * 2f}, dir={Direction}, ifz={InvertForbiddenZone}";

    /// <summary>检测位置是否在扇形环内</summary>
    public override bool Check(WPos position, WPos origin)
        => position.InDonutCone(origin, InnerRadius, OuterRadius, Direction, HalfAngle);

    /// <summary>获取距离计算器</summary>
    public override ShapeDistance Distance(WPos origin)
    {
        return InvertForbiddenZone
            ? new SDInvertedDonutSector(origin, InnerRadius, OuterRadius, Direction, HalfAngle)
            : new SDDonutSector(origin, InnerRadius, OuterRadius, Direction, HalfAngle);
    }

    /// <summary>获取反转的距离计算器</summary>
    public override ShapeDistance InvertedDistance(WPos origin)
    {
        return InvertForbiddenZone
            ? new SDDonutSector(origin, InnerRadius, OuterRadius, Direction, HalfAngle)
            : new SDInvertedDonutSector(origin, InnerRadius, OuterRadius, Direction, HalfAngle);
    }
}

/// <summary>
/// 矩形 AOE 形状（Rectangle）
/// </summary>
/// <remarks>
/// 表示以原点为基准的矩形区域
/// 常见于直线攻击、冲锋类技能
/// </remarks>
/// <param name="lengthFront">前方长度（码）</param>
/// <param name="halfWidth">半宽度（码）- 总宽度 = halfWidth × 2</param>
/// <param name="lengthBack">后方长度（码）- 默认为 0</param>
/// <param name="direction">矩形朝向。角度约定：0°=北，90°=东，180°=南，270°=西</param>
/// <param name="invertForbiddenZone">是否反转禁止区域</param>
[SkipLocalsInit]
public sealed class AOEShapeRect(float lengthFront, float halfWidth, float lengthBack = default, Angle direction = default, bool invertForbiddenZone = false) : AOEShape(invertForbiddenZone)
{
    /// <summary>前方长度（码）- 从原点向前延伸的距离</summary>
    public readonly float LengthFront = lengthFront;

    /// <summary>半宽度（码）- 矩形总宽度的一半</summary>
    public readonly float HalfWidth = halfWidth;

    /// <summary>后方长度（码）- 从原点向后延伸的距离</summary>
    public readonly float LengthBack = lengthBack;

    /// <summary>矩形朝向</summary>
    public readonly Angle Direction = direction;

    /// <summary>返回形状的字符串描述</summary>
    public override string ToString() => $"Rect: l={LengthFront:f3}+{LengthBack:f3}, w={HalfWidth * 2f}, dir={Direction}, ifz={InvertForbiddenZone}";

    /// <summary>检测位置是否在矩形内</summary>
    public override bool Check(WPos position, WPos origin)
        => position.InRect(origin, Direction, LengthFront, LengthBack, HalfWidth);

    /// <summary>获取距离计算器</summary>
    public override ShapeDistance Distance(WPos origin)
    {
        return InvertForbiddenZone
            ? new SDInvertedRect(origin, Direction, LengthFront, LengthBack, HalfWidth)
            : new SDRect(origin, Direction, LengthFront, LengthBack, HalfWidth);
    }

    /// <summary>获取反转的距离计算器</summary>
    public override ShapeDistance InvertedDistance(WPos origin)
    {
        return InvertForbiddenZone
            ? new SDRect(origin, Direction, LengthFront, LengthBack, HalfWidth)
            : new SDInvertedRect(origin, Direction, LengthFront, LengthBack, HalfWidth);
    }
}

/// <summary>
/// 十字形 AOE 形状（Cross）
/// </summary>
/// <remarks>
/// 表示以原点为中心的十字形区域（两个垂直的矩形组合）
/// 常见于十字激光、十字斩等技能
/// </remarks>
/// <param name="length">十字臂长度（码）- 从中心到端点的距离</param>
/// <param name="halfWidth">半宽度（码）- 十字臂宽度的一半</param>
/// <param name="direction">十字朝向。角度约定：0°=北，90°=东，180°=南，270°=西</param>
/// <param name="invertForbiddenZone">是否反转禁止区域</param>
[SkipLocalsInit]
public sealed class AOEShapeCross(float length, float halfWidth, Angle direction = default, bool invertForbiddenZone = false) : AOEShape(invertForbiddenZone)
{
    /// <summary>十字臂长度（码）- 从中心到端点的距离</summary>
    public readonly float Length = length;

    /// <summary>半宽度（码）- 十字臂宽度的一半</summary>
    public readonly float HalfWidth = halfWidth;

    /// <summary>十字朝向</summary>
    public readonly Angle Direction = direction;

    /// <summary>返回形状的字符串描述</summary>
    public override string ToString() => $"Cross: l={Length:f3}, w={HalfWidth * 2f}, dir={Direction}, ifz={InvertForbiddenZone}";

    /// <summary>检测位置是否在十字形内</summary>
    public override bool Check(WPos position, WPos origin)
        => position.InCross(origin, Direction, Length, HalfWidth);

    /// <summary>获取距离计算器</summary>
    public override ShapeDistance Distance(WPos origin)
    {
        return InvertForbiddenZone
            ? new SDInvertedCross(origin, Direction, Length, HalfWidth)
            : new SDCross(origin, Direction, Length, HalfWidth);
    }

    /// <summary>获取反转的距离计算器</summary>
    public override ShapeDistance InvertedDistance(WPos origin)
    {
        return InvertForbiddenZone
            ? new SDCross(origin, Direction, Length, HalfWidth)
            : new SDInvertedCross(origin, Direction, Length, HalfWidth);
    }
}

/// <summary>
/// 三角形扇形 AOE 形状（Triangle Cone）
/// </summary>
/// <remarks>
/// 表示以原点为顶点的三角形区域
/// 与普通扇形不同，三角形扇形的边是直线而非弧线
/// </remarks>
/// <param name="sideLength">三角形边长（码）</param>
/// <param name="halfAngle">半角（总角度的一半）</param>
/// <param name="direction">三角形中心方向。角度约定：0°=北，90°=东，180°=南，270°=西</param>
/// <param name="invertForbiddenZone">是否反转禁止区域</param>
[SkipLocalsInit]
public sealed class AOEShapeTriCone(float sideLength, Angle halfAngle, Angle direction = default, bool invertForbiddenZone = false) : AOEShape(invertForbiddenZone)
{
    /// <summary>三角形边长（码）</summary>
    public readonly float SideLength = sideLength;

    /// <summary>半角（总角度的一半）</summary>
    public readonly Angle HalfAngle = halfAngle;

    /// <summary>三角形中心方向</summary>
    public readonly Angle Direction = direction;

    /// <summary>返回形状的字符串描述</summary>
    public override string ToString() => $"TriCone: side={SideLength:f3}, angle={HalfAngle * 2f}, dir={Direction}, ifz={InvertForbiddenZone}";

    /// <summary>检测位置是否在三角形内</summary>
    public override bool Check(WPos position, WPos origin)
        => position.InTri(origin,
            origin + SideLength * (Direction + HalfAngle).ToDirection(),
            origin + SideLength * (Direction - HalfAngle).ToDirection());

    /// <summary>获取距离计算器</summary>
    public override ShapeDistance Distance(WPos origin)
    {
        var left = (Direction + HalfAngle).ToDirection() * SideLength;
        var right = (Direction - HalfAngle).ToDirection() * SideLength;
        var triangle = new RelTriangle(default, left, right);
        return InvertForbiddenZone
            ? new SDInvertedTri(origin, triangle)
            : new SDTri(origin, triangle);
    }

    /// <summary>获取反转的距离计算器</summary>
    public override ShapeDistance InvertedDistance(WPos origin)
    {
        var left = (Direction + HalfAngle).ToDirection() * SideLength;
        var right = (Direction - HalfAngle).ToDirection() * SideLength;
        var triangle = new RelTriangle(default, left, right);
        return InvertForbiddenZone
            ? new SDTri(origin, triangle)
            : new SDInvertedTri(origin, triangle);
    }
}

/// <summary>
/// 胶囊形 AOE 形状（Capsule，圆角矩形）
/// </summary>
/// <remarks>
/// 表示两端为半圆的矩形区域（类似药丸形状）
/// 常见于某些冲锋类或线性攻击
/// </remarks>
/// <param name="radius">胶囊半径（码）- 两端半圆的半径</param>
/// <param name="length">胶囊长度（码）- 从原点到远端的距离</param>
/// <param name="direction">胶囊朝向。角度约定：0°=北，90°=东，180°=南，270°=西</param>
/// <param name="invertForbiddenZone">是否反转禁止区域</param>
[SkipLocalsInit]
public sealed class AOEShapeCapsule(float radius, float length, Angle direction = default, bool invertForbiddenZone = false) : AOEShape(invertForbiddenZone)
{
    /// <summary>胶囊半径（码）- 两端半圆的半径</summary>
    public readonly float Radius = radius;

    /// <summary>胶囊长度（码）- 从原点到远端的距离</summary>
    public readonly float Length = length;

    /// <summary>胶囊朝向</summary>
    public readonly Angle Direction = direction;

    /// <summary>返回形状的字符串描述</summary>
    public override string ToString() => $"Capsule: radius={Radius:f3}, length={Length}, dir={Direction}, ifz={InvertForbiddenZone}";

    /// <summary>检测位置是否在胶囊形内</summary>
    public override bool Check(WPos position, WPos origin)
        => position.InCapsule(origin, Direction.ToDirection(), Radius, Length);

    /// <summary>获取距离计算器</summary>
    public override ShapeDistance Distance(WPos origin)
    {
        return InvertForbiddenZone
            ? new SDInvertedCapsule(origin, Direction, Length, Radius)
            : new SDCapsule(origin, Direction, Length, Radius);
    }

    /// <summary>获取反转的距离计算器</summary>
    public override ShapeDistance InvertedDistance(WPos origin)
    {
        return InvertForbiddenZone
            ? new SDCapsule(origin, Direction, Length, Radius)
            : new SDInvertedCapsule(origin, Direction, Length, Radius);
    }
}

/// <summary>
/// 弧形胶囊 AOE 形状（Arc Capsule）
/// </summary>
/// <remarks>
/// 表示沿圆弧路径的胶囊形区域
/// 用于表示沿轨道移动的攻击
/// </remarks>
/// <param name="radius">胶囊半径（码）</param>
/// <param name="angularLength">弧形角度长度</param>
/// <param name="orbitCenter">轨道中心点</param>
/// <param name="invertForbiddenZone">是否反转禁止区域</param>
[SkipLocalsInit]
public sealed class AOEShapeArcCapsule(float radius, Angle angularLength, WPos orbitCenter, bool invertForbiddenZone = false) : AOEShape(invertForbiddenZone)
{
    /// <summary>胶囊半径（码）</summary>
    public readonly float Radius = radius;

    /// <summary>弧形角度长度</summary>
    public readonly Angle AngularLength = angularLength;

    /// <summary>轨道中心点</summary>
    public readonly WPos OrbitCenter = orbitCenter;

    /// <summary>返回形状的字符串描述</summary>
    public override string ToString() => $"ArcCapsule: radius={Radius:f3}, length={AngularLength}, orbitCenter={OrbitCenter}, ifz={InvertForbiddenZone}";

    /// <summary>检测位置是否在弧形胶囊内</summary>
    public override bool Check(WPos position, WPos origin)
        => position.InArcCapsule(origin, -(origin - OrbitCenter), AngularLength, Radius);

    /// <summary>获取距离计算器</summary>
    public override ShapeDistance Distance(WPos origin)
    {
        return InvertForbiddenZone
            ? new SDInvertedArcCapsule(origin, OrbitCenter, AngularLength, Radius)
            : new SDArcCapsule(origin, OrbitCenter, AngularLength, Radius);
    }

    /// <summary>获取反转的距离计算器</summary>
    public override ShapeDistance InvertedDistance(WPos origin)
    {
        return InvertForbiddenZone
            ? new SDArcCapsule(origin, OrbitCenter, AngularLength, Radius)
            : new SDInvertedArcCapsule(origin, OrbitCenter, AngularLength, Radius);
    }
}

