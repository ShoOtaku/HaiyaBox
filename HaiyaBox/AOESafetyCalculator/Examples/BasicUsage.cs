using AOESafetyCalculator.Core;
using AOESafetyCalculator.DistanceField;
using AOESafetyCalculator.SafetyZone;

namespace AOESafetyCalculator.Examples;

/// <summary>
/// AOESafetyCalculator 基础使用示例
/// </summary>
/// <remarks>
/// 演示如何使用距离场和安全区计算功能
/// </remarks>
public static class BasicUsage
{
    /// <summary>
    /// 示例1: 基础安全检查
    /// </summary>
    public static void Example1_BasicSafetyCheck()
    {
        // 创建一个圆形危险区域（中心在原点，半径10）
        var circleZone = new ForbiddenZone
        {
            Shape = new SDCircle(new WPos(0, 0), 10f),
            Activation = DateTime.MinValue // 立即激活
        };

        // 创建安全区计算器
        var calculator = new SafeZoneCalculator();
        calculator.AddForbiddenZone(circleZone);

        // 检查不同位置是否安全
        var currentTime = DateTime.Now;
        var pos1 = new WPos(5, 0);   // 在危险区域内
        var pos2 = new WPos(15, 0);  // 在危险区域外

        Console.WriteLine($"位置 (5, 0) 是否安全: {calculator.IsSafe(pos1, currentTime)}");
        Console.WriteLine($"位置 (15, 0) 是否安全: {calculator.IsSafe(pos2, currentTime)}");

        // 计算到最近危险区域的距离
        var distance1 = calculator.DistanceToNearestDanger(pos1, currentTime);
        var distance2 = calculator.DistanceToNearestDanger(pos2, currentTime);

        Console.WriteLine($"位置 (5, 0) 到危险区域的距离: {distance1:F2}");
        Console.WriteLine($"位置 (15, 0) 到危险区域的距离: {distance2:F2}");
    }

    /// <summary>
    /// 示例2: 多个危险区域
    /// </summary>
    public static void Example2_MultipleDangerZones()
    {
        var calculator = new SafeZoneCalculator();

        // 添加多个不同形状的危险区域
        // 圆形区域
        calculator.AddForbiddenZone(new ForbiddenZone
        {
            Shape = new SDCircle(new WPos(0, 0), 8f)
        });

        // 矩形区域
        calculator.AddForbiddenZone(new ForbiddenZone
        {
            Shape = new SDRect(new WPos(20, 0), new WDir(1, 0), 10f, 10f, 5f)
        });

        // 扇形区域（锥形）
        calculator.AddForbiddenZone(new ForbiddenZone
        {
            Shape = new SDCone(new WPos(-20, 0), 15f, Angle.FromDirection(new WDir(1, 0)), 45f.Degrees())
        });

        // 检查某个位置的安全性
        var testPos = new WPos(10, 0);
        var currentTime = DateTime.Now;

        if (calculator.IsSafe(testPos, currentTime))
        {
            Console.WriteLine($"位置 {testPos} 是安全的");
        }
        else
        {
            var distance = calculator.DistanceToNearestDanger(testPos, currentTime);
            Console.WriteLine($"位置 {testPos} 在危险区域内，深度: {-distance:F2}");
        }
    }

    /// <summary>
    /// 示例3: 查找安全方向和位置
    /// </summary>
    public static void Example3_FindSafeDirection()
    {
        var calculator = new SafeZoneCalculator();

        // 创建一个危险区域
        calculator.AddForbiddenZone(new ForbiddenZone
        {
            Shape = new SDCircle(new WPos(0, 0), 10f)
        });

        var currentTime = DateTime.Now;
        var currentPos = new WPos(5, 0); // 当前在危险区域内

        // 查找最安全的方向
        var safeDirection = calculator.FindSafestDirection(currentPos, currentTime, sampleCount: 16);
        Console.WriteLine($"最安全的方向: ({safeDirection.X:F2}, {safeDirection.Z:F2})");

        // 在指定区域内查找最安全的位置
        var searchCenter = new WPos(0, 0);
        var searchRadius = 20f;
        var safestPos = calculator.FindSafestPosition(searchCenter, searchRadius, currentTime, gridResolution: 1.0f);
        Console.WriteLine($"最安全的位置: ({safestPos.X:F2}, {safestPos.Z:F2})");
    }

    /// <summary>
    /// 示例4: 延迟激活的危险区域
    /// </summary>
    public static void Example4_DelayedActivation()
    {
        var calculator = new SafeZoneCalculator();
        var currentTime = DateTime.Now;

        // 立即激活的危险区域
        calculator.AddForbiddenZone(new ForbiddenZone
        {
            Shape = new SDCircle(new WPos(0, 0), 8f),
            Activation = currentTime
        });

        // 3秒后激活的危险区域
        calculator.AddForbiddenZone(new ForbiddenZone
        {
            Shape = new SDCircle(new WPos(15, 0), 8f),
            Activation = currentTime.AddSeconds(3)
        });

        // 5秒后激活的危险区域
        calculator.AddForbiddenZone(new ForbiddenZone
        {
            Shape = new SDCircle(new WPos(30, 0), 8f),
            Activation = currentTime.AddSeconds(5)
        });

        var testPos = new WPos(15, 0);

        // 检查当前时间的安全性
        Console.WriteLine($"当前时间，位置 {testPos} 是否安全: {calculator.IsSafe(testPos, currentTime)}");

        // 检查3秒后的安全性
        var futureTime = currentTime.AddSeconds(3);
        Console.WriteLine($"3秒后，位置 {testPos} 是否安全: {calculator.IsSafe(testPos, futureTime)}");

        // 获取活跃的危险区域数量
        Console.WriteLine($"当前活跃的危险区域数量: {calculator.GetActiveZoneCount(currentTime)}");
        Console.WriteLine($"3秒后活跃的危险区域数量: {calculator.GetActiveZoneCount(futureTime)}");
    }

    /// <summary>
    /// 示例5: 使用距离场操作（并集和交集）
    /// </summary>
    public static void Example5_DistanceFieldOperations()
    {
        // 创建两个圆形距离场
        var circle1 = new SDCircle(new WPos(0, 0), 10f);
        var circle2 = new SDCircle(new WPos(15, 0), 10f);

        // 并集：两个圆形的并集区域
        var union = new SDUnion([circle1, circle2]);
        var unionZone = new ForbiddenZone { Shape = union };

        // 交集：两个圆形的交集区域
        var intersection = new SDIntersection([circle1, circle2]);
        var intersectionZone = new ForbiddenZone { Shape = intersection };

        // 反转并集：两个圆形外部的区域
        var invertedUnion = new SDInvertedUnion([circle1, circle2]);
        var safeZone = new ForbiddenZone { Shape = invertedUnion };

        var calculator = new SafeZoneCalculator();
        calculator.AddForbiddenZone(unionZone);

        var testPos = new WPos(7.5f, 0); // 在两个圆形之间
        var currentTime = DateTime.Now;

        Console.WriteLine($"位置 {testPos} 在并集区域内: {!calculator.IsSafe(testPos, currentTime)}");
        Console.WriteLine($"到并集边界的距离: {calculator.DistanceToNearestDanger(testPos, currentTime):F2}");
    }

    /// <summary>
    /// 示例6: 复杂场景 - 环形安全区
    /// </summary>
    public static void Example6_DonutSafeZone()
    {
        var calculator = new SafeZoneCalculator();

        // 创建一个环形危险区域（内圈半径5，外圈半径15）
        var donutZone = new ForbiddenZone
        {
            Shape = new SDDonut(new WPos(0, 0), 5f, 15f)
        };

        calculator.AddForbiddenZone(donutZone);

        var currentTime = DateTime.Now;

        // 测试不同位置
        var positions = new[]
        {
            new WPos(0, 0),    // 中心（安全）
            new WPos(3, 0),    // 内圈内（安全）
            new WPos(10, 0),   // 环形内（危险）
            new WPos(20, 0)    // 外圈外（安全）
        };

        foreach (var pos in positions)
        {
            var isSafe = calculator.IsSafe(pos, currentTime);
            var distance = calculator.DistanceToNearestDanger(pos, currentTime);
            Console.WriteLine($"位置 ({pos.X:F1}, {pos.Z:F1}): {(isSafe ? "安全" : "危险")}, 距离: {distance:F2}");
        }
    }

    /// <summary>
    /// 运行所有示例
    /// </summary>
    public static void RunAllExamples()
    {
        Console.WriteLine("=== 示例1: 基础安全检查 ===");
        Example1_BasicSafetyCheck();
        Console.WriteLine();

        Console.WriteLine("=== 示例2: 多个危险区域 ===");
        Example2_MultipleDangerZones();
        Console.WriteLine();

        Console.WriteLine("=== 示例3: 查找安全方向和位置 ===");
        Example3_FindSafeDirection();
        Console.WriteLine();

        Console.WriteLine("=== 示例4: 延迟激活的危险区域 ===");
        Example4_DelayedActivation();
        Console.WriteLine();

        Console.WriteLine("=== 示例5: 距离场操作 ===");
        Example5_DistanceFieldOperations();
        Console.WriteLine();

        Console.WriteLine("=== 示例6: 环形安全区 ===");
        Example6_DonutSafeZone();
    }
}
