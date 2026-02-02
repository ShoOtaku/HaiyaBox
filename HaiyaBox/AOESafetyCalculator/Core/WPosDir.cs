using System.Numerics;
using System.Runtime.CompilerServices;

namespace AOESafetyCalculator.Core;

/// <summary>
/// 世界方向向量（World Direction）
/// 表示 XZ 平面上的二维方向向量
/// </summary>
/// <remarks>
/// 使用右手坐标系：
/// - X 轴：东西方向（正值向东）
/// - Y 轴：上下方向（正值向上，本结构不使用）
/// - Z 轴：南北方向（正值向南）
///
/// 本结构用于表示方向和位移，不包含 Y 坐标
/// 常用于：
/// - 角色朝向计算
/// - AOE 方向判断
/// - 移动方向计算
/// </remarks>
[SkipLocalsInit]
public readonly struct WDir(float x, float z)
{
    /// <summary>X 坐标分量（东西方向）</summary>
    public readonly float X = x;

    /// <summary>Z 坐标分量（南北方向）</summary>
    public readonly float Z = z;

    /// <summary>
    /// 从 Vector2 构造方向向量
    /// </summary>
    /// <param name="v">二维向量，X 对应 X，Y 对应 Z</param>
    public WDir(Vector2 v) : this(v.X, v.Y) { }

    /// <summary>
    /// 转换为 Vector2
    /// </summary>
    /// <returns>二维向量 (X, Z)</returns>
    public readonly Vector2 ToVec2() => new(X, Z);

    /// <summary>
    /// 转换为 Vector3
    /// </summary>
    /// <param name="y">Y 坐标（高度），默认为 0</param>
    /// <returns>三维向量 (X, y, Z)</returns>
    public readonly Vector3 ToVec3(float y = default) => new(X, y, Z);

    /// <summary>
    /// 转换为 Vector4
    /// </summary>
    /// <param name="y">Y 坐标（高度），默认为 0</param>
    /// <param name="w">W 分量，默认为 0</param>
    /// <returns>四维向量 (X, y, Z, w)</returns>
    public readonly Vector4 ToVec4(float y = default, float w = default) => new(X, y, Z, w);

    /// <summary>
    /// 转换为世界位置（将方向向量视为从原点出发的位置）
    /// </summary>
    /// <returns>世界位置</returns>
    public readonly WPos ToWPos() => new(X, Z);

    /// <summary>
    /// 从 Vector3 创建 WDir（忽略 Y 坐标）
    /// </summary>
    /// <param name="v">三维向量</param>
    /// <returns>世界方向（X, Z）</returns>
    public static WDir FromVec3(Vector3 v) => new(v.X, v.Z);

    #region 运算符重载

    /// <summary>相等比较</summary>
    public static bool operator ==(WDir left, WDir right) => left.X == right.X && left.Z == right.Z;

    /// <summary>不等比较</summary>
    public static bool operator !=(WDir left, WDir right) => left.X != right.X || left.Z != right.Z;

    /// <summary>向量加法</summary>
    public static WDir operator +(WDir a, WDir b) => new(a.X + b.X, a.Z + b.Z);

    /// <summary>向量减法</summary>
    public static WDir operator -(WDir a, WDir b) => new(a.X - b.X, a.Z - b.Z);

    /// <summary>向量取反（反方向）</summary>
    public static WDir operator -(WDir a) => new(-a.X, -a.Z);

    /// <summary>方向减去位置（特殊用法）</summary>
    public static WDir operator -(WDir a, WPos b) => new(a.X - b.X, a.Z - b.Z);

    /// <summary>向量标量乘法</summary>
    public static WDir operator *(WDir a, float b) => new(a.X * b, a.Z * b);

    /// <summary>标量向量乘法</summary>
    public static WDir operator *(float a, WDir b) => new(a * b.X, a * b.Z);

    /// <summary>向量标量除法</summary>
    public static WDir operator /(WDir a, float b)
    {
        // 使用乘法代替除法以提高性能
        var invB = 1f / b;
        return new(a.X * invB, a.Z * invB);
    }

    #endregion

    #region 向量操作方法

    /// <summary>
    /// 取绝对值（各分量取绝对值）
    /// </summary>
    public readonly WDir Abs() => new(Math.Abs(X), Math.Abs(Z));

    /// <summary>
    /// 取符号（各分量取符号：-1、0 或 1）
    /// </summary>
    public readonly WDir Sign() => new(Math.Sign(X), Math.Sign(Z));

    /// <summary>
    /// 获取左侧正交向量（逆时针旋转 90 度）
    /// </summary>
    /// <remarks>
    /// 用于计算垂直于当前方向的左侧方向
    /// 例如：计算 AOE 矩形的宽度方向
    /// </remarks>
    public readonly WDir OrthoL() => new(Z, -X);

    /// <summary>
    /// 获取右侧正交向量（顺时针旋转 90 度）
    /// </summary>
    /// <remarks>
    /// 用于计算垂直于当前方向的右侧方向
    /// </remarks>
    public readonly WDir OrthoR() => new(-Z, X);

    /// <summary>
    /// X 轴镜像（翻转 X 分量）
    /// </summary>
    public readonly WDir MirrorX() => new(-X, Z);

    /// <summary>
    /// Z 轴镜像（翻转 Z 分量）
    /// </summary>
    public readonly WDir MirrorZ() => new(X, -Z);

    /// <summary>
    /// 点积（内积）
    /// </summary>
    /// <param name="a">另一个向量</param>
    /// <returns>点积结果，用于计算投影和夹角</returns>
    /// <remarks>
    /// 点积 = |a| * |b| * cos(θ)
    /// 用于判断两向量的夹角关系：
    /// - 正值：夹角小于 90 度
    /// - 零：垂直
    /// - 负值：夹角大于 90 度
    /// </remarks>
    public readonly float Dot(WDir a) => X * a.X + Z * a.Z;

    /// <summary>
    /// 叉积（外积）的 Z 分量
    /// </summary>
    /// <param name="b">另一个向量</param>
    /// <returns>叉积的 Z 分量，用于判断旋转方向</returns>
    /// <remarks>
    /// 二维叉积结果是标量，表示两向量构成的平行四边形面积
    /// 正值表示 b 在 a 的逆时针方向
    /// </remarks>
    public readonly float Cross(WDir b) => X * b.Z - Z * b.X;

    /// <summary>
    /// 按指定方向旋转向量
    /// </summary>
    /// <param name="dir">旋转方向（单位向量表示的角度）</param>
    /// <returns>旋转后的向量</returns>
    /// <remarks>
    /// 使用旋转矩阵进行旋转：
    /// [cos -sin] [X]   [X*cos + Z*sin]
    /// [sin  cos] [Z] = [Z*cos - X*sin]
    /// 其中 dir.Z = cos(θ), dir.X = sin(θ)
    /// </remarks>
    public readonly WDir Rotate(WDir dir)
    {
        var dirZ = dir.Z;
        var dirX = dir.X;
        return new(X * dirZ + Z * dirX, Z * dirZ - X * dirX);
    }

    /// <summary>
    /// 按指定角度旋转向量
    /// </summary>
    /// <param name="dir">旋转角度</param>
    /// <returns>旋转后的向量</returns>
    public readonly WDir Rotate(Angle dir) => Rotate(dir.ToDirection());

    /// <summary>
    /// 计算向量长度的平方
    /// </summary>
    /// <returns>长度的平方</returns>
    /// <remarks>
    /// 避免开方运算，用于距离比较时更高效
    /// 例如：判断是否在某半径内时，比较 LengthSq() 和 radius*radius
    /// </remarks>
    public readonly float LengthSq() => X * X + Z * Z;

    /// <summary>
    /// 计算向量长度
    /// </summary>
    /// <returns>向量长度</returns>
    public readonly float Length() => MathF.Sqrt(LengthSq());

    /// <summary>
    /// 获取单位向量（归一化）
    /// </summary>
    /// <returns>长度为 1 的同方向向量，零向量返回默认值</returns>
    /// <remarks>
    /// 用于获取纯方向信息，去除长度影响
    /// </remarks>
    public readonly WDir Normalized()
    {
        var length = MathF.Sqrt(X * X + Z * Z);
        return length > 0f ? this / length : default;
    }

    /// <summary>
    /// 近似相等判断
    /// </summary>
    /// <param name="b">比较目标</param>
    /// <param name="eps">容差值</param>
    /// <returns>各分量差值都在容差范围内返回 true</returns>
    public readonly bool AlmostEqual(WDir b, float eps) => Math.Abs(X - b.X) <= eps && Math.Abs(Z - b.Z) <= eps;

    /// <summary>
    /// 缩放向量
    /// </summary>
    /// <param name="multiplier">缩放倍数</param>
    /// <returns>缩放后的向量</returns>
    public readonly WDir Scaled(float multiplier) => new(X * multiplier, Z * multiplier);

    /// <summary>
    /// 四舍五入到整数
    /// </summary>
    public readonly WDir Rounded() => new(MathF.Round(X), MathF.Round(Z));

    /// <summary>
    /// 按指定精度四舍五入
    /// </summary>
    /// <param name="precision">精度（如 0.1 表示保留一位小数）</param>
    public readonly WDir Rounded(float precision) => Scaled(1f / precision).Rounded().Scaled(precision);

    /// <summary>
    /// 向下取整
    /// </summary>
    public readonly WDir Floor() => new(MathF.Floor(X), MathF.Floor(Z));

    /// <summary>
    /// 转换为角度
    /// </summary>
    /// <returns>从 Z 轴正方向（北）顺时针测量的角度</returns>
    /// <remarks>
    /// 使用 atan2(X, Z) 计算，结果范围 [-π, π]
    /// 0 度指向北（Z 正方向）
    /// </remarks>
    public readonly Angle ToAngle() => new(MathF.Atan2(X, Z));

    #endregion

    #region Object 重写

    public override readonly string ToString() => $"({X:f3}, {Z:f3})";
    public readonly bool Equals(WDir other) => this == other;
    public override readonly bool Equals(object? obj) => obj is WDir other && Equals(other);
    public override readonly int GetHashCode() => (X, Z).GetHashCode();

    #endregion

    #region 区域检测方法

    /// <summary>
    /// 检测点是否在矩形区域内（假设 this 是相对于形状中心的偏移）
    /// </summary>
    /// <param name="direction">矩形的朝向（单位向量）</param>
    /// <param name="lenFront">前方长度</param>
    /// <param name="lenBack">后方长度</param>
    /// <param name="halfWidth">半宽度</param>
    /// <returns>在矩形内返回 true</returns>
    /// <remarks>
    /// 用于检测 AOE 矩形范围
    /// 矩形以 direction 为前方，总长度为 lenFront + lenBack
    /// </remarks>
    public readonly bool InRect(WDir direction, float lenFront, float lenBack, float halfWidth)
    {
        var dotDir = Dot(direction);
        var dotNormal = Dot(direction.OrthoL());
        return dotDir >= -lenBack && dotDir <= lenFront && Math.Abs(dotNormal) <= halfWidth;
    }

    /// <summary>
    /// 检测点是否在十字形区域内
    /// </summary>
    /// <param name="direction">十字的朝向</param>
    /// <param name="length">臂长</param>
    /// <param name="halfWidth">臂的半宽度</param>
    /// <returns>在十字形内返回 true</returns>
    public readonly bool InCross(WDir direction, float length, float halfWidth)
    {
        var dotDir = Dot(direction);
        var absDotNormal = Math.Abs(Dot(direction.OrthoL()));
        var inVerticalArm = dotDir >= -length && dotDir <= length && absDotNormal <= halfWidth;
        var inHorizontalArm = dotDir >= -halfWidth && dotDir <= halfWidth && absDotNormal <= length;
        return inVerticalArm || inHorizontalArm;
    }

    #endregion
}

/// <summary>
/// 世界位置（World Position）
/// 表示 XZ 平面上的二维世界坐标
/// </summary>
/// <remarks>
/// 使用右手坐标系：
/// - X 轴：东西方向（正值向东）
/// - Y 轴：上下方向（正值向上，本结构不使用）
/// - Z 轴：南北方向（正值向南）
///
/// 本结构用于表示游戏世界中的位置，不包含 Y 坐标
/// 常用于：
/// - 角色位置
/// - AOE 中心点
/// - 场地边界
/// - 寻路目标点
/// </remarks>
[SkipLocalsInit]
public readonly struct WPos(float x, float z)
{
    /// <summary>X 坐标（东西方向）</summary>
    public readonly float X = x;

    /// <summary>Z 坐标（南北方向）</summary>
    public readonly float Z = z;

    /// <summary>
    /// 从 Vector2 构造位置
    /// </summary>
    /// <param name="v">二维向量，X 对应 X，Y 对应 Z</param>
    public WPos(Vector2 v) : this(v.X, v.Y) { }

    /// <summary>
    /// 转换为 Vector2
    /// </summary>
    public readonly Vector2 ToVec2() => new(X, Z);

    /// <summary>
    /// 转换为 Vector3
    /// </summary>
    /// <param name="y">Y 坐标（高度），默认为 0</param>
    public readonly Vector3 ToVec3(float y = 0) => new(X, y, Z);

    /// <summary>
    /// 转换为 Vector4
    /// </summary>
    /// <param name="y">Y 坐标（高度），默认为 0</param>
    /// <param name="w">W 分量，默认为 0</param>
    public readonly Vector4 ToVec4(float y = 0, float w = 0) => new(X, y, Z, w);

    /// <summary>
    /// 转换为方向向量（将位置视为从原点出发的方向）
    /// </summary>
    public readonly WDir ToWDir() => new(X, Z);

    /// <summary>
    /// 从 Vector3 创建 WPos（忽略 Y 坐标）
    /// </summary>
    /// <param name="v">三维向量</param>
    /// <returns>世界位置（X, Z）</returns>
    public static WPos FromVec3(Vector3 v) => new(v.X, v.Z);

    #region 运算符重载

    /// <summary>相等比较</summary>
    public static bool operator ==(WPos left, WPos right) => left.X == right.X && left.Z == right.Z;

    /// <summary>不等比较</summary>
    public static bool operator !=(WPos left, WPos right) => left.X != right.X || left.Z != right.Z;

    /// <summary>位置标量乘法</summary>
    public static WPos operator *(WPos a, float b) => new(a.X * b, a.Z * b);

    /// <summary>位置标量加法（各分量加同一值）</summary>
    public static WPos operator +(WPos a, float b) => new(a.X + b, a.Z + b);

    /// <summary>位置整数除法</summary>
    public static WPos operator /(WPos a, int b)
    {
        var invB = 1f / b;
        return new(a.X * invB, a.Z * invB);
    }

    /// <summary>位置浮点除法</summary>
    public static WPos operator /(WPos a, float b)
    {
        var invB = 1f / b;
        return new(a.X * invB, a.Z * invB);
    }

    /// <summary>位置加方向（移动位置）</summary>
    public static WPos operator +(WPos a, WDir b) => new(a.X + b.X, a.Z + b.Z);

    /// <summary>方向加位置（移动位置）</summary>
    public static WPos operator +(WDir a, WPos b) => new(a.X + b.X, a.Z + b.Z);

    /// <summary>位置减方向（反向移动位置）</summary>
    public static WPos operator -(WPos a, WDir b) => new(a.X - b.X, a.Z - b.Z);

    /// <summary>
    /// 两位置相减得到方向向量
    /// </summary>
    /// <remarks>
    /// 结果是从 b 指向 a 的方向向量
    /// 常用于计算两点之间的方向和距离
    /// </remarks>
    public static WDir operator -(WPos a, WPos b) => new(a.X - b.X, a.Z - b.Z);

    #endregion

    #region 位置操作方法

    /// <summary>
    /// 近似相等判断
    /// </summary>
    /// <param name="b">比较目标</param>
    /// <param name="eps">容差值</param>
    /// <returns>各分量差值都在容差范围内返回 true</returns>
    public readonly bool AlmostEqual(WPos b, float eps) => Math.Abs(X - b.X) <= eps && Math.Abs(Z - b.Z) <= eps;

    /// <summary>
    /// 缩放位置（相对于原点）
    /// </summary>
    /// <param name="multiplier">缩放倍数</param>
    public readonly WPos Scaled(float multiplier) => new(X * multiplier, Z * multiplier);

    /// <summary>
    /// 四舍五入到整数
    /// </summary>
    public readonly WPos Rounded() => new(MathF.Round(X), MathF.Round(Z));

    /// <summary>
    /// 按指定精度四舍五入
    /// </summary>
    /// <param name="precision">精度（如 0.1 表示保留一位小数）</param>
    public readonly WPos Rounded(float precision) => Scaled(1f / precision).Rounded().Scaled(precision);

    /// <summary>
    /// 线性插值
    /// </summary>
    /// <param name="from">起始位置</param>
    /// <param name="to">目标位置</param>
    /// <param name="progress">进度（0-1）</param>
    /// <returns>插值后的位置</returns>
    /// <remarks>
    /// progress=0 返回 from，progress=1 返回 to
    /// 用于平滑移动、动画等
    /// </remarks>
    public static WPos Lerp(WPos from, WPos to, float progress) => new(from.ToVec2() * (1f - progress) + to.ToVec2() * progress);

    /// <summary>
    /// 量化位置到游戏网格
    /// </summary>
    /// <returns>量化后的位置</returns>
    /// <remarks>
    /// AOE 位置会被游戏服务器量化到网格中心
    /// 当无法使用 spell.LocXZ 时，可用此方法修正位置
    /// 网格大小约为 2000/65535 ≈ 0.0305
    /// </remarks>
    public readonly WPos Quantized()
    {
        const float gridSize = 2000f / 65535f;
        const float gridSizeInv = 1f / gridSize;
        return new(((int)MathF.Round(X * gridSizeInv) - 0.5f) * gridSize, ((int)MathF.Round(Z * gridSizeInv) - 0.5f) * gridSize);
    }

    /// <summary>
    /// 绕原点旋转位置
    /// </summary>
    /// <param name="rotateByDegrees">旋转角度（度）</param>
    /// <param name="origin">旋转中心</param>
    /// <param name="point">要旋转的点</param>
    /// <returns>旋转后的位置</returns>
    public static WPos RotateAroundOrigin(float rotateByDegrees, WPos origin, WPos point)
    {
        var (sin, cos) = ((float, float))Math.SinCos(rotateByDegrees * Angle.DegToRad);
        var deltaX = point.X - origin.X;
        var deltaZ = point.Z - origin.Z;
        var rotatedX = cos * deltaX - sin * deltaZ;
        var rotatedZ = sin * deltaX + cos * deltaZ;
        return new(origin.X + rotatedX, origin.Z + rotatedZ);
    }

    /// <summary>
    /// 批量旋转顶点数组
    /// </summary>
    /// <param name="center">旋转中心</param>
    /// <param name="vertices">顶点数组</param>
    /// <param name="rotationAngle">旋转角度（度）</param>
    /// <returns>旋转后的顶点数组</returns>
    public static WPos[] GenerateRotatedVertices(WPos center, WPos[] vertices, float rotationAngle)
    {
        var len = vertices.Length;
        var rotatedVertices = new WPos[len];
        for (var i = 0; i < len; ++i)
            rotatedVertices[i] = RotateAroundOrigin(rotationAngle, center, vertices[i]);
        return rotatedVertices;
    }

    #endregion

    #region Object 重写

    public override readonly string ToString() => $"[{X:f3}, {Z:f3}]";
    public readonly bool Equals(WPos other) => this == other;
    public override readonly bool Equals(object? obj) => obj is WPos other && Equals(other);
    public override readonly int GetHashCode() => (X, Z).GetHashCode();

    #endregion

    #region 区域检测方法 - 基础形状

    /// <summary>
    /// 检测点是否在三角形内
    /// </summary>
    public readonly bool InTri(WPos v1, WPos v2, WPos v3)
    {
        var s = (v2.X - v1.X) * (Z - v1.Z) - (v2.Z - v1.Z) * (X - v1.X);
        var t = (v3.X - v2.X) * (Z - v2.Z) - (v3.Z - v2.Z) * (X - v2.X);
        if ((s < 0f) != (t < 0f) && s != 0f && t != 0f)
            return false;
        var d = (v1.X - v3.X) * (Z - v3.Z) - (v1.Z - v3.Z) * (X - v3.X);
        return d == 0f || (d < 0f) == (s + t <= 0f);
    }

    /// <summary>
    /// 检测点是否在矩形内（使用方向向量）
    /// </summary>
    public readonly bool InRect(WPos origin, WDir direction, float lenFront, float lenBack, float halfWidth)
        => (this - origin).InRect(direction, lenFront, lenBack, halfWidth);

    /// <summary>
    /// 检测点是否在矩形内（使用角度）
    /// </summary>
    public readonly bool InRect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth)
        => (this - origin).InRect(direction.ToDirection(), lenFront, lenBack, halfWidth);

    /// <summary>
    /// 检测点是否在矩形内（使用起点到终点向量）
    /// </summary>
    public readonly bool InRect(WPos origin, WDir startToEnd, float halfWidth)
    {
        var len = startToEnd.Length();
        if (len <= 0f)
        {
            return false;
        }
        return InRect(origin, startToEnd / len, len, default, halfWidth);
    }

    /// <summary>
    /// 检测点是否在矩形内（使用起点和终点）
    /// </summary>
    public readonly bool InRect(WPos origin, WPos end, float halfWidth) => InRect(origin, end - origin, halfWidth);

    /// <summary>
    /// 检测点是否在旋转的正方形内
    /// </summary>
    public readonly bool InSquare(WPos origin, float halfWidth, Angle rotation)
        => (this - origin).InRect(rotation.ToDirection(), halfWidth, halfWidth, halfWidth);

    /// <summary>
    /// 检测点是否在旋转的正方形内（使用方向向量）
    /// </summary>
    public readonly bool InSquare(WPos origin, float halfWidth, WDir rotation)
        => (this - origin).InRect(rotation, halfWidth, halfWidth, halfWidth);

    /// <summary>
    /// 检测点是否在轴对齐的正方形内（AABB）
    /// </summary>
    public readonly bool InSquare(WPos origin, float halfWidth)
        => Math.Abs(X - origin.X) <= halfWidth && Math.Abs(Z - origin.Z) <= halfWidth;

    /// <summary>
    /// 检测点是否在轴对齐的矩形内（AABB）
    /// </summary>
    public readonly bool InRect(WPos origin, float halfWidth, float halfHeight)
        => Math.Abs(X - origin.X) <= halfWidth && Math.Abs(Z - origin.Z) <= halfHeight;

    /// <summary>
    /// 检测点是否在十字形内（使用角度）
    /// </summary>
    public readonly bool InCross(WPos origin, Angle direction, float length, float halfWidth)
        => (this - origin).InCross(direction.ToDirection(), length, halfWidth);

    /// <summary>
    /// 检测点是否在十字形内（使用方向向量）
    /// </summary>
    public readonly bool InCross(WPos origin, WDir direction, float length, float halfWidth)
        => (this - origin).InCross(direction, length, halfWidth);

    #endregion

    #region 区域检测方法 - 圆形和扇形

    /// <summary>
    /// 检测点是否在圆形内
    /// </summary>
    public readonly bool InCircle(WPos origin, float radius) => (this - origin).LengthSq() <= radius * radius;

    /// <summary>
    /// 检测点是否在圆环内
    /// </summary>
    public readonly bool InDonut(WPos origin, float innerRadius, float outerRadius)
        => InCircle(origin, outerRadius) && !InCircle(origin, innerRadius);

    /// <summary>
    /// 检测点是否在扇形内（使用方向向量）
    /// </summary>
    public readonly bool InCone(WPos origin, WDir direction, Angle halfAngle)
        => (this - origin).Normalized().Dot(direction) >= halfAngle.Cos();

    /// <summary>
    /// 检测点是否在扇形内（使用角度）
    /// </summary>
    public readonly bool InCone(WPos origin, Angle direction, Angle halfAngle)
        => InCone(origin, direction.ToDirection(), halfAngle);

    /// <summary>
    /// 检测点是否在扇形圆内（圆形 + 扇形）
    /// </summary>
    public readonly bool InCircleCone(WPos origin, float radius, WDir direction, Angle halfAngle)
        => InCircle(origin, radius) && InCone(origin, direction, halfAngle);

    /// <summary>
    /// 检测点是否在扇形圆内（使用角度）
    /// </summary>
    public readonly bool InCircleCone(WPos origin, float radius, Angle direction, Angle halfAngle)
        => InCircle(origin, radius) && InCone(origin, direction, halfAngle);

    /// <summary>
    /// 检测点是否在扇形环内（圆环 + 扇形）
    /// </summary>
    public readonly bool InDonutCone(WPos origin, float innerRadius, float outerRadius, WDir direction, Angle halfAngle)
        => InDonut(origin, innerRadius, outerRadius) && InCone(origin, direction, halfAngle);

    /// <summary>
    /// 检测点是否在扇形环内（使用角度）
    /// </summary>
    public readonly bool InDonutCone(WPos origin, float innerRadius, float outerRadius, Angle direction, Angle halfAngle)
        => InDonut(origin, innerRadius, outerRadius) && InCone(origin, direction, halfAngle);

    #endregion

    #region 区域检测方法 - 胶囊形和弧形

    /// <summary>
    /// 检测点是否在胶囊形内
    /// </summary>
    public readonly bool InCapsule(WPos origin, WDir direction, float radius, float length)
    {
        var offset = this - origin;
        var t = Math.Clamp(offset.Dot(direction), 0f, length);
        var proj = origin + t * direction;
        return (this - proj).LengthSq() <= radius * radius;
    }

    /// <summary>
    /// 检测点是否在弧形胶囊内（使用方向向量）
    /// </summary>
    public readonly bool InArcCapsule(WPos start, WDir toOrbitCenter, Angle angularLength, float tubeRadius)
        => InArcCapsule(start, start + toOrbitCenter, angularLength, tubeRadius);

    /// <summary>
    /// 检测点是否在弧形胶囊内（使用轨道中心）
    /// </summary>
    public readonly bool InArcCapsule(WPos start, WPos orbitCenter, Angle angularLength, float tubeRadius)
    {
        var tube2 = tubeRadius * tubeRadius;

        var ox = orbitCenter.X;
        var oz = orbitCenter.Z;
        var s = start;
        var r0 = new WDir(s.X - ox, s.Z - oz);
        var R = r0.Length();

        var r1 = r0.Rotate(angularLength);
        var end = new WPos(ox + r1.X, oz + r1.Z);

        var ds = (this - start).LengthSq() <= tube2;
        if (ds)
            return true;

        var de = (this - end).LengthSq() <= tube2;
        if (de)
            return true;

        var vx = X - ox;
        var vz = Z - oz;
        var r2 = vx * vx + vz * vz;
        var Rin = R - tubeRadius;
        var Rout = R + tubeRadius;
        if (r2 < Rin * Rin || r2 > Rout * Rout)
            return false;

        var half = angularLength.Abs() * 0.5f;
        var coneFactor = half.Rad > Angle.HalfPi ? -1f : 1f;
        var mid = Angle.FromDirection(r0) + angularLength * 0.5f;
        var a90 = 90f.Degrees();
        var nl = coneFactor * (mid + half + a90).ToDirection();
        var nr = coneFactor * (mid - half - a90).ToDirection();
        var sL = vx * nl.X + vz * nl.Z;
        var sR = vx * nr.X + vz * nr.Z;

        return coneFactor > 0f ? (sL <= 0f && sR <= 0f) : (sL >= 0f || sR >= 0f);
    }

    #endregion
}
