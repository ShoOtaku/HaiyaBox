using System.Numerics;
using AOESafetyCalculator.Core;

namespace HaiyaBox.Rendering;

public static class DistanceFieldContourBuilder
{
    public static List<DisplayObjectLine> Build(
        Func<WPos, float> distance,
        WPos center,
        float radius,
        float step,
        float height,
        uint color,
        float thickness)
    {
        var result = new List<DisplayObjectLine>();
        if (distance == null) return result;
        if (radius <= 0f) return result;
        if (step <= 0f) return result;

        var minX = center.X - radius;
        var minZ = center.Z - radius;
        var size = radius * 2f;
        var cols = (int)MathF.Ceiling(size / step);
        if (cols <= 0) return result;

        for (var ix = 0; ix < cols; ix++)
        {
            var x = minX + ix * step;
            for (var iz = 0; iz < cols; iz++)
            {
                var z = minZ + iz * step;

                var a = new WPos(x, z);
                var b = new WPos(x + step, z);
                var c = new WPos(x + step, z + step);
                var d = new WPos(x, z + step);

                var da = distance(a);
                var db = distance(b);
                var dc = distance(c);
                var dd = distance(d);

                var aIn = da <= 0f;
                var bIn = db <= 0f;
                var cIn = dc <= 0f;
                var dIn = dd <= 0f;

                var mask = 0;
                if (aIn) mask |= 1;
                if (bIn) mask |= 2;
                if (cIn) mask |= 4;
                if (dIn) mask |= 8;

                if (mask == 0 || mask == 15) continue;

                var edge = new Vector3?[4];
                if (aIn != bIn) edge[0] = Interp(a, da, b, db, height);
                if (bIn != cIn) edge[1] = Interp(b, db, c, dc, height);
                if (cIn != dIn) edge[2] = Interp(c, dc, d, dd, height);
                if (dIn != aIn) edge[3] = Interp(d, dd, a, da, height);

                switch (mask)
                {
                    case 1:
                        Add(result, edge[3], edge[0], color, thickness);
                        break;
                    case 2:
                        Add(result, edge[0], edge[1], color, thickness);
                        break;
                    case 3:
                        Add(result, edge[3], edge[1], color, thickness);
                        break;
                    case 4:
                        Add(result, edge[1], edge[2], color, thickness);
                        break;
                    case 5:
                        Add(result, edge[3], edge[0], color, thickness);
                        Add(result, edge[1], edge[2], color, thickness);
                        break;
                    case 6:
                        Add(result, edge[0], edge[2], color, thickness);
                        break;
                    case 7:
                        Add(result, edge[3], edge[2], color, thickness);
                        break;
                    case 8:
                        Add(result, edge[2], edge[3], color, thickness);
                        break;
                    case 9:
                        Add(result, edge[0], edge[2], color, thickness);
                        break;
                    case 10:
                        Add(result, edge[0], edge[1], color, thickness);
                        Add(result, edge[2], edge[3], color, thickness);
                        break;
                    case 11:
                        Add(result, edge[1], edge[2], color, thickness);
                        break;
                    case 12:
                        Add(result, edge[1], edge[3], color, thickness);
                        break;
                    case 13:
                        Add(result, edge[0], edge[1], color, thickness);
                        break;
                    case 14:
                        Add(result, edge[3], edge[0], color, thickness);
                        break;
                }
            }
        }

        return result;
    }

    private static Vector3 Interp(WPos p1, float d1, WPos p2, float d2, float height)
    {
        var t = d1 / (d1 - d2);
        if (float.IsNaN(t) || float.IsInfinity(t))
        {
            t = 0.5f;
        }
        t = Math.Clamp(t, 0f, 1f);
        var x = p1.X + (p2.X - p1.X) * t;
        var z = p1.Z + (p2.Z - p1.Z) * t;
        return new Vector3(x, height, z);
    }

    private static void Add(List<DisplayObjectLine> list, Vector3? a, Vector3? b, uint color, float thickness)
    {
        if (!a.HasValue || !b.HasValue) return;
        list.Add(new DisplayObjectLine(a.Value, b.Value, color, thickness));
    }
}
