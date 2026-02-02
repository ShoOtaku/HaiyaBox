using AOESafetyCalculator.Core;
using AOESafetyCalculator.DistanceField;
using AOESafetyCalculator.SafetyZone;

namespace AOESafetyCalculator.Examples;

/// <summary>
/// 场地边界使用示例
/// </summary>
public static class ArenaBoundsExample
{
    /// <summary>
    /// 示例1：圆形场地
    /// </summary>
    public static void Example1_CircleArena()
    {
        var calculator = new SafeZoneCalculator();

        // 设置圆形场地（中心点 (0,0)，半径 40米）
        calculator.SetArenaBounds(new CircleArenaBounds(new WPos(0, 0), 40f));

        // 添加危险区域
        calculator.AddForbiddenZone(new ForbiddenZone
        {
            Shape = new SDCircle(new WPos(0, 0), 10f)
        });

        var currentTime = DateTime.Now;

        // 查找安全点（自动使用场地边界）
        var safePoints = calculator.FindSafePositions(8, currentTime)
            .MinDistanceBetween(5f)
            .Execute();

        Console.WriteLine($"圆形场地内找到 {safePoints.Count} 个安全点");
        foreach (var point in safePoints)
        {
            Console.WriteLine($"  ({point.X:F1}, {point.Z:F1})");
        }
    }

    /// <summary>
    /// 示例2：矩形场地
    /// </summary>
    public static void Example2_RectArena()
    {
        var calculator = new SafeZoneCalculator();

        // 设置矩形场地（中心 (0,0)，朝向东，40米×30米）
        calculator.SetArenaBounds(new RectArenaBounds(
            center: new WPos(0, 0),
            direction: new WDir(1, 0),  // 朝向东
            halfWidth: 15f,              // 半宽 15米（总宽30米）
            halfLength: 20f              // 半长 20米（总长40米）
        ));

        // 添加危险区域
        calculator.AddForbiddenZone(new ForbiddenZone
        {
            Shape = new SDCircle(new WPos(0, 0), 8f)
        });

        var currentTime = DateTime.Now;

        // 查找安全点
        var safePoints = calculator.FindSafePositions(8, currentTime)
            .MinDistanceBetween(4f)
            .Execute();

        Console.WriteLine($"矩形场地内找到 {safePoints.Count} 个安全点");
    }

    /// <summary>
    /// 示例3：场地变化
    /// </summary>
    public static void Example3_ChangingArena()
    {
        var calculator = new SafeZoneCalculator();
        var currentTime = DateTime.Now;

        // 阶段1：大场地（半径 40米）
        calculator.SetArenaBounds(new CircleArenaBounds(new WPos(0, 0), 40f));
        calculator.AddForbiddenZone(new ForbiddenZone
        {
            Shape = new SDCircle(new WPos(10, 0), 8f)
        });

        var phase1Points = calculator.FindSafePositions(8, currentTime)
            .Execute();
        Console.WriteLine($"阶段1（大场地）: {phase1Points.Count} 个安全点");

        // 阶段2：场地缩小（半径 25米）
        calculator.SetArenaBounds(new CircleArenaBounds(new WPos(0, 0), 25f));

        var phase2Points = calculator.FindSafePositions(8, currentTime)
            .Execute();
        Console.WriteLine($"阶段2（小场地）: {phase2Points.Count} 个安全点");
    }

    /// <summary>
    /// 示例4：实战场景 - Boss战斗
    /// </summary>
    public static void Example4_BossFight()
    {
        var calculator = new SafeZoneCalculator();
        var bossPos = new WPos(0, 0);
        var tankPos = new WPos(15, 0);

        // 设置矩形竞技场
        calculator.SetArenaBounds(new RectArenaBounds(
            center: new WPos(0, 0),
            direction: new WDir(1, 0),
            halfWidth: 20f,
            halfLength: 20f
        ));

        // 添加40个危险区域
        for (int i = 0; i < 40; i++)
        {
            var angle = i * (360f / 40f) * Angle.DegToRad;
            var pos = bossPos + new WDir(
                MathF.Sin(angle) * 10f,
                MathF.Cos(angle) * 10f
            );
            calculator.AddForbiddenZone(new ForbiddenZone
            {
                Shape = new SDCircle(pos, 3f)
            });
        }

        var currentTime = DateTime.Now;

        // 查找8个安全点：靠近坦克，彼此分散
        var safePoints = calculator.FindSafePositions(8, currentTime)
            .NearTarget(tankPos, maxDistance: 20f)
            .MinDistanceBetween(4f)
            .Execute();

        Console.WriteLine($"找到 {safePoints.Count} 个安全点（矩形场地内）");
        for (int i = 0; i < safePoints.Count; i++)
        {
            var point = safePoints[i];
            var distToTank = (point - tankPos).Length();
            Console.WriteLine($"  {i + 1}. ({point.X:F1}, {point.Z:F1}) - 距离坦克: {distToTank:F1}米");
        }
    }
}
