using System.Runtime.CompilerServices;
using System.Text;
using AOESafetyCalculator.Core;

namespace AOESafetyCalculator.Shapes;

/// <summary>
/// 形状基类
/// 定义各种几何形状，用于AOE范围显示和碰撞检测
/// </summary>
[SkipLocalsInit]
public abstract record class Shape
{
    /// <summary>常量0.5，用于计算中点</summary>
    public const float Half = 0.5f;

    /// <summary>
    /// 生成形状的轮廓点列表
    /// </summary>
    /// <param name="center">参考中心点</param>
    /// <returns>相对于中心的轮廓点列表</returns>
    public abstract List<WDir> Contour(WPos center);
}

/// <summary>
/// 圆形
/// </summary>
/// <param name="Center">圆心世界坐标</param>
/// <param name="Radius">半径</param>
[SkipLocalsInit]
public sealed record class Circle(WPos Center, float Radius) : Shape
{
    /// <summary>
    /// 生成圆形轮廓（使用32个顶点近似）
    /// </summary>
    public override List<WDir> Contour(WPos center)
    {
        const int segments = 32;
        var vertices = new List<WDir>(segments);
        var offset = Center - center;
        var angleIncrement = 2f * MathF.PI / segments;

        for (var i = 0; i < segments; ++i)
        {
            var angle = i * angleIncrement;
            var (sin, cos) = ((float, float))Math.SinCos(angle);
            vertices.Add(new WDir(Radius * sin, Radius * cos) + offset);
        }
        return vertices;
    }

    public override string ToString() => $"Circle:{Center},{Radius}";
}

/// <summary>
/// 自定义多边形（使用世界坐标顶点）
/// </summary>
/// <param name="Vertices">顶点数组（世界坐标）</param>
[SkipLocalsInit]
public sealed record class PolygonCustom(WPos[] Vertices) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var vertices = Vertices;
        var len = vertices.Length;
        var result = new List<WDir>(len);
        for (var i = 0; i < len; ++i)
        {
            result.Add(vertices[i] - center);
        }
        return result;
    }

    public override string ToString()
    {
        var vertices = Vertices;
        var len = vertices.Length;
        var sb = new StringBuilder("PolygonCustom:", 14 + len * 9);
        for (var i = 0; i < len; ++i)
        {
            sb.Append(vertices[i]).Append(';');
        }
        --sb.Length;
        return sb.ToString();
    }
}

/// <summary>
/// 自定义多边形（使用相对坐标顶点）
/// </summary>
/// <param name="Vertices">顶点数组（相对坐标）</param>
[SkipLocalsInit]
public sealed record class PolygonCustomRel(WDir[] Vertices) : Shape
{
    public override List<WDir> Contour(WPos center) => [.. Vertices];

    public override string ToString()
    {
        var vertices = Vertices;
        var len = vertices.Length;
        var sb = new StringBuilder("PolygonCustomRel:", 17 + len * 9);
        for (var i = 0; i < len; ++i)
        {
            sb.Append(vertices[i]).Append(';');
        }
        --sb.Length;
        return sb.ToString();
    }
}

/// <summary>
/// 环形（甜甜圈）
/// </summary>
/// <param name="Center">圆心世界坐标</param>
/// <param name="InnerRadius">内半径</param>
/// <param name="OuterRadius">外半径</param>
[SkipLocalsInit]
public sealed record class Donut(WPos Center, float InnerRadius, float OuterRadius) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        const int segments = 32;
        var vertices = new List<WDir>(segments * 2);
        var offset = Center - center;
        var angleIncrement = 2f * MathF.PI / segments;

        // 外圈顶点
        for (var i = 0; i < segments; ++i)
        {
            var angle = i * angleIncrement;
            var (sin, cos) = ((float, float))Math.SinCos(angle);
            vertices.Add(new WDir(OuterRadius * sin, OuterRadius * cos) + offset);
        }

        // 内圈顶点（逆序）
        for (var i = segments - 1; i >= 0; --i)
        {
            var angle = i * angleIncrement;
            var (sin, cos) = ((float, float))Math.SinCos(angle);
            vertices.Add(new WDir(InnerRadius * sin, InnerRadius * cos) + offset);
        }

        return vertices;
    }

    public override string ToString() => $"Donut:{Center},{InnerRadius},{OuterRadius}";
}

/// <summary>
/// 矩形
/// </summary>
/// <param name="Center">中心点世界坐标</param>
/// <param name="HalfWidth">半宽</param>
/// <param name="HalfHeight">半高</param>
/// <param name="Rotation">旋转角度（默认为0）</param>
[SkipLocalsInit]
public record class Rectangle(WPos Center, float HalfWidth, float HalfHeight, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var dir = Rotation != default ? Rotation.ToDirection() : new(default, 1f);
        var dx = dir.OrthoL() * HalfWidth;
        var dz = dir * HalfHeight;

        WDir[] vertices =
        [
            dx - dz,
            -dx - dz,
            -dx + dz,
            dx + dz
        ];

        var offset = Center - center;
        var result = new List<WDir>(4);
        for (var i = 0; i < 4; ++i)
        {
            result.Add(vertices[i] + offset);
        }
        return result;
    }

    public override string ToString() => $"Rectangle:{Center},{HalfWidth},{HalfHeight},{Rotation}";
}

/// <summary>
/// 正方形
/// </summary>
/// <param name="Center">中心点世界坐标</param>
/// <param name="HalfSize">半边长</param>
/// <param name="Rotation">旋转角度</param>
[SkipLocalsInit]
public sealed record class Square(WPos Center, float HalfSize, Angle Rotation = default) : Rectangle(Center, HalfSize, HalfSize, Rotation);

/// <summary>
/// 正多边形
/// </summary>
/// <param name="Center">中心点世界坐标</param>
/// <param name="Radius">外接圆半径</param>
/// <param name="Edges">边数</param>
/// <param name="Rotation">旋转角度</param>
[SkipLocalsInit]
public sealed record class Polygon(WPos Center, float Radius, int Edges, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var edges = Edges;
        var angleIncrement = Angle.DoublePI / edges;
        var initialRotation = Rotation.Rad;
        var radius = Radius;
        var vertices = new List<WDir>(edges);
        var offset = Center - center;
        var offsetX = offset.X;
        var offsetZ = offset.Z;
        for (var i = 0; i < edges; ++i)
        {
            var (sin, cos) = ((float, float))Math.SinCos(i * angleIncrement + initialRotation);
            vertices.Add(new(radius * sin + offsetX, radius * cos + offsetZ));
        }
        return vertices;
    }

    public override string ToString() => $"Polygon:{Center},{Radius},{Edges},{Rotation}";
}

/// <summary>
/// 扇形
/// </summary>
/// <param name="Center">圆心世界坐标</param>
/// <param name="Radius">半径</param>
/// <param name="StartAngle">起始角度</param>
/// <param name="EndAngle">结束角度</param>
[SkipLocalsInit]
public record class Cone(WPos Center, float Radius, Angle StartAngle, Angle EndAngle) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        const int segmentsPerRadian = 8;
        var angleSpan = (EndAngle - StartAngle).Rad;
        var segments = Math.Max(2, (int)(Math.Abs(angleSpan) * segmentsPerRadian));
        var vertices = new List<WDir>(segments + 2);
        var offset = Center - center;

        // 添加圆心
        vertices.Add(offset);

        // 添加圆弧顶点
        var angleIncrement = angleSpan / segments;
        for (var i = 0; i <= segments; ++i)
        {
            var angle = StartAngle.Rad + i * angleIncrement;
            var (sin, cos) = ((float, float))Math.SinCos(angle);
            vertices.Add(new WDir(Radius * sin, Radius * cos) + offset);
        }

        return vertices;
    }

    public override string ToString() => $"Cone:{Center},{Radius},{StartAngle},{EndAngle}";
}

/// <summary>
/// 扇形（通过中心方向和半角定义）
/// </summary>
/// <param name="Center">圆心世界坐标</param>
/// <param name="Radius">半径</param>
/// <param name="CenterDir">中心方向角</param>
/// <param name="HalfAngle">半角</param>
[SkipLocalsInit]
public sealed record class ConeHA(WPos Center, float Radius, Angle CenterDir, Angle HalfAngle) : Cone(Center, Radius, CenterDir - HalfAngle, CenterDir + HalfAngle);
