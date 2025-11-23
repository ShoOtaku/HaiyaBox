namespace HaiyaBox.Utils
{
    public abstract class DangerArea
    {
        public abstract bool IsPointInDanger(Point point);
    }

    public class CircleDangerArea : DangerArea
    {
        public Point Center { get; set; }
        public double Radius { get; set; }

        public override bool IsPointInDanger(Point point)
        {
            double distance = Math.Sqrt(Math.Pow(point.X - Center.X, 2) + Math.Pow(point.Y - Center.Y, 2));
            return distance <= Radius;
        }
    }

    public class RectangleDangerArea : DangerArea
    {
        public double MinX { get; set; }
        public double MaxX { get; set; }
        public double MinY { get; set; }
        public double MaxY { get; set; }

        public override bool IsPointInDanger(Point point)
        {
            return point.X >= MinX && point.X <= MaxX && point.Y >= MinY && point.Y <= MaxY;
        }
    }

    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double GetDistanceTo(Point other)
        {
            return Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
        }

        public override string ToString()
        {
            return $"({X:F1}, {Y:F1})";
        }
    }

    public class SafePointCalculator
    {
        /// <summary>
        /// 查找安全点（分两组：贴近参考点+自由分布）
        /// </summary>
        /// <param name="limitMinX">限制范围左边界</param>
        /// <param name="limitMaxX">限制范围右边界</param>
        /// <param name="limitMinY">限制范围下边界</param>
        /// <param name="limitMaxY">限制范围上边界</param>
        /// <param name="dangerAreas">危险区域列表</param>
        /// <param name="referencePoint">参考点</param>
        /// <param name="minSafePointDistance">安全点之间最小间距</param>
        /// <param name="closeToRefCount">需要贴近参考点的数量</param>
        /// <param name="maxFarDistance">自由分布组的最大远离距离（相对于参考点）</param>
        /// <param name="sampleStep">采样步长</param>
        /// <param name="totalSafePointCount">总安全点数量</param>
        /// <returns>分组合并后的安全点列表</returns>
        public List<Point> FindSafePoints(
            double limitMinX, double limitMaxX,
            double limitMinY, double limitMaxY,
            List<DangerArea> dangerAreas,
            Point referencePoint,
            double minSafePointDistance = 3.0,
            int closeToRefCount = 3, // 核心：3个点优先贴近参考点
            double maxFarDistance = 25.0, // 核心：自由分布组距参考点≤25单位
            double sampleStep = 0.5,
            int totalSafePointCount = 8)
        {
            // 1. 网格采样+筛选基础安全点（不在危险区）
            List<Point> allSamplePoints = new List<Point>();
            for (double x = limitMinX; x <= limitMaxX; x += sampleStep)
            {
                for (double y = limitMinY; y <= limitMaxY; y += sampleStep)
                {
                    allSamplePoints.Add(new Point(x, y));
                }
            }

            List<Point> baseSafePoints = allSamplePoints
                .Where(p => !dangerAreas.Any(area => area.IsPointInDanger(p)))
                .ToList();

            if (baseSafePoints.Count == 0)
                throw new Exception("无可用安全点，请调整参数");

            // 2. 第一组：优先贴近参考点的安全点（按距离排序+间距筛选）
            List<Point> closeToRefPoints = new List<Point>();
            var sortedByRefDistance = baseSafePoints.OrderBy(p => p.GetDistanceTo(referencePoint)).ToList();
            foreach (var point in sortedByRefDistance)
            {
                if (closeToRefPoints.All(p => p.GetDistanceTo(point) >= minSafePointDistance))
                {
                    closeToRefPoints.Add(point);
                    if (closeToRefPoints.Count == closeToRefCount)
                        break;
                }
            }

            // 3. 第二组：自由分布安全点（距参考点≤maxFarDistance+间距筛选）
            List<Point> freePoints = new List<Point>();
            // 筛选出：在maxFarDistance范围内 + 不在第一组 + 满足间距
            var candidateFreePoints = baseSafePoints
                .Where(p => p.GetDistanceTo(referencePoint) <= maxFarDistance)
                .Except(closeToRefPoints) // 排除已选的贴近点
                .OrderBy(_ => Guid.NewGuid()) // 随机排序，实现自由分布
                .ToList();

            foreach (var point in candidateFreePoints)
            {
                // 需同时满足：与第一组、第二组已选点的间距≥阈值
                bool isFarEnough = closeToRefPoints.All(p => p.GetDistanceTo(point) >= minSafePointDistance)
                                 && freePoints.All(p => p.GetDistanceTo(point) >= minSafePointDistance);
                if (isFarEnough)
                {
                    freePoints.Add(point);
                    // 满足总数量后停止
                    if (closeToRefPoints.Count + freePoints.Count == totalSafePointCount)
                        break;
                }
            }

            // 4. 合并结果（贴近组在前，自由组在后）
            var result = closeToRefPoints.Concat(freePoints).ToList();
            if (result.Count < totalSafePointCount)
                Console.WriteLine($"警告：仅找到{result.Count}个符合要求的安全点（需{totalSafePointCount}个），可缩小最小间距或调整maxFarDistance");

            return result;
        }
    }

    // 示例调用（35×35范围+贴合需求）
    class Program
    {
        static void Main(string[] args)
        {
            // 1. 35×35限制范围
            double limitMinX = 0, limitMaxX = 35;
            double limitMinY = 0, limitMaxY = 35;
            Point referencePoint = new Point(18, 18); // 参考点（中心）

            // 2. 核心参数（按需调整）
            int totalSafePointCount = 8; // 总安全点数量
            int closeToRefCount = 3; // 3个点贴近参考点
            double minSafePointDistance = 3.0; // 安全点间距≥3单位
            double maxFarDistance = 25.0; // 自由组距参考点≤25单位
            double sampleStep = 0.5;

            // 3. 危险区域（示例）
            List<DangerArea> dangerAreas = new List<DangerArea>
            {
                new CircleDangerArea { Center = new Point(10, 10), Radius = 2.0 },
                new RectangleDangerArea { MinX = 20, MaxX = 25, MinY = 15, MaxY = 20 },
                new CircleDangerArea { Center = new Point(30, 30), Radius = 1.5 }
            };

            // 4. 计算安全点
            SafePointCalculator calculator = new SafePointCalculator();
            List<Point> safePoints = calculator.FindSafePoints(
                limitMinX, limitMaxX, limitMinY, limitMaxY,
                dangerAreas, referencePoint,
                minSafePointDistance, closeToRefCount,
                maxFarDistance, sampleStep, totalSafePointCount);

            // 5. 输出结果（标注分组）
            Console.WriteLine($"参考点：{referencePoint}");
            Console.WriteLine($"配置：{closeToRefCount}个贴近点 + {totalSafePointCount - closeToRefCount}个自由分布点（距参考点≤{maxFarDistance}单位）");
            Console.WriteLine($"安全点列表（前{closeToRefCount}个为贴近参考点）：");
            for (int i = 0; i < safePoints.Count; i++)
            {
                double distToRef = safePoints[i].GetDistanceTo(referencePoint);
                string group = i < closeToRefCount ? "[贴近组]" : "[自由组]";
                Console.WriteLine($"{i+1}. {group} {safePoints[i]} （距参考点：{distToRef:F2}单位）");
            }

            Console.ReadKey();
        }
    }
}
