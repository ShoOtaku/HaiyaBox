using AOESafetyCalculator.Core;
using AOESafetyCalculator.DistanceField;
using AOESafetyCalculator.SafetyZone;

namespace AOESafetyCalculator.Examples;

/// <summary>
/// 约束安全位置查询示例
/// </summary>
/// <remarks>
/// 演示如何使用链式API查找满足特定约束的安全位置
/// </remarks>
public static class ConstrainedSafetyExample
{
    /// <summary>
    /// 示例1：简单查询 - 查找5个安全点
    /// </summary>
    public static void Example1_SimpleQuery()
    {
        var calculator = new SafeZoneCalculator();

        // 添加一些危险区域
        calculator.AddForbiddenZone(new ForbiddenZone
        {
            Shape = new SDCircle(new WPos(0, 0), 10f)
        });

        var currentTime = DateTime.Now;
        var arenaCenter = new WPos(0, 0);

        // 简单查询：在40米半径内查找5个安全点
        var safePoints = calculator.FindSafePositions(5, arenaCenter, 40f, currentTime)
            .Execute();

        Console.WriteLine($"找到 {safePoints.Count} 个安全点:");
        foreach (var point in safePoints)
        {
            Console.WriteLine($"  ({point.X:F1}, {point.Z:F1})");
        }
    }

    /// <summary>
    /// 示例2：靠近目标点 - 查找靠近Boss的安全点
    /// </summary>
    public static void Example2_NearTarget()
    {
        var calculator = new SafeZoneCalculator();

        // Boss位置
        var bossPos = new WPos(0, 0);

        // Boss周围的危险区域
        calculator.AddForbiddenZone(new ForbiddenZone
        {
            Shape = new SDCircle(bossPos, 8f)  // Boss脚下8米危险
        });

        var currentTime = DateTime.Now;

        // 查找靠近Boss的安全点（最远不超过20米）
        var safePoints = calculator.FindSafePositions(5, bossPos, 40f, currentTime)
            .NearTarget(bossPos, maxDistance: 20f)  // 限制最大距离20米
            .Execute();  // 结果已按距离Boss的距离排序（近的在前）

        Console.WriteLine("靠近Boss的安全点（按距离排序）:");
        foreach (var point in safePoints)
        {
            var distance = (point - bossPos).Length();
            Console.WriteLine($"  ({point.X:F1}, {point.Z:F1}) - 距离: {distance:F1}米");
        }
    }

    /// <summary>
    /// 示例3：分散分布 - 8个玩家站位
    /// </summary>
    public static void Example3_SpreadPositions()
    {
        var calculator = new SafeZoneCalculator();

        var arenaCenter = new WPos(0, 0);

        // 添加多个危险区域
        calculator.AddForbiddenZone(new ForbiddenZone
        {
            Shape = new SDCircle(new WPos(10, 0), 8f)
        });
        calculator.AddForbiddenZone(new ForbiddenZone
        {
            Shape = new SDCircle(new WPos(-10, 0), 8f)
        });

        var currentTime = DateTime.Now;

        // 查找8个分散的安全点（玩家之间至少相距5米）
        var safePoints = calculator.FindSafePositions(8, arenaCenter, 40f, currentTime)
            .MinDistanceBetween(5f)  // 点之间最小距离5米
            .Execute();

        Console.WriteLine($"找到 {safePoints.Count} 个分散的安全点:");
        foreach (var point in safePoints)
        {
            Console.WriteLine($"  ({point.X:F1}, {point.Z:F1})");
        }
    }

    /// <summary>
    /// 示例4：角度约束 - 8个方向分散站位
    /// </summary>
    public static void Example4_AngleConstraint()
    {
        var calculator = new SafeZoneCalculator();
        var arenaCenter = new WPos(0, 0);

        // 添加危险区域
        calculator.AddForbiddenZone(new ForbiddenZone
        {
            Shape = new SDCircle(arenaCenter, 5f)
        });

        var currentTime = DateTime.Now;

        // 查找8个安全点，相对于中心点的角度至少相差45度
        var safePoints = calculator.FindSafePositions(8, arenaCenter, 40f, currentTime)
            .WithMinAngle(arenaCenter, 45f.Degrees())  // 最小角度间隔45度
            .MinDistanceBetween(3f)
            .Execute();

        Console.WriteLine("8个方向分散的安全点:");
        foreach (var point in safePoints)
        {
            var angle = Angle.FromDirection(point - arenaCenter);
            Console.WriteLine($"  ({point.X:F1}, {point.Z:F1}) - 角度: {angle.Deg:F0}°");
        }
    }

    /// <summary>
    /// 示例5：组合约束 - 实际战斗场景
    /// </summary>
    public static void Example5_CombinedConstraints()
    {
        var calculator = new SafeZoneCalculator();
        var bossPos = new WPos(0, 0);
        var tankPos = new WPos(15, 0);  // 坦克位置

        // 模拟40个危险区域
        for (int i = 0; i < 40; i++)
        {
            var angle = i * (360f / 40f);
            var pos = bossPos + new WDir(
                MathF.Sin(angle * Angle.DegToRad) * 10f,
                MathF.Cos(angle * Angle.DegToRad) * 10f
            );
            calculator.AddForbiddenZone(new ForbiddenZone
            {
                Shape = new SDCircle(pos, 3f)
            });
        }

        var currentTime = DateTime.Now;

        // 查找8个安全点：靠近坦克，彼此分散，按距离坦克排序
        var safePoints = calculator.FindSafePositions(8, bossPos, 40f, currentTime)
            .NearTarget(tankPos, maxDistance: 20f)  // 距离坦克不超过20米
            .MinDistanceBetween(4f)                  // 玩家之间至少4米
            .Execute();                              // 自动按距离坦克排序

        Console.WriteLine($"找到 {safePoints.Count} 个安全点（按距离坦克排序）:");
        for (int i = 0; i < safePoints.Count; i++)
        {
            var point = safePoints[i];
            var distToTank = (point - tankPos).Length();
            Console.WriteLine($"  {i + 1}. ({point.X:F1}, {point.Z:F1}) - 距离坦克: {distToTank:F1}米");
        }
    }
}
