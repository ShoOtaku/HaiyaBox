using AOESafetyCalculator.Core;
using AOESafetyCalculator.Shapes;
using HaiyaBox.Utils;
using Xunit;

namespace AOESafetyCalculator.Tests;

public sealed class AOEShapeDebugTests
{
    [Fact]
    public void GetColorBucket_SameSize_IgnoresPosition()
    {
        var shape = new AOEShapeRect(10f, 2f, 1f);
        var bucketA = AOEShapeDebug.GetColorBucket(shape, new WPos(0f, 0f));
        var bucketB = AOEShapeDebug.GetColorBucket(shape, new WPos(100f, 50f));

        Assert.Equal(bucketA, bucketB);
    }

    [Fact]
    public void GetColorBucket_ArcCapsule_SameOrbitRadius_IgnoresTranslation()
    {
        var shapeA = new AOEShapeArcCapsule(2f, 90f.Degrees(), new WPos(10f, 0f));
        var shapeB = new AOEShapeArcCapsule(2f, 90f.Degrees(), new WPos(110f, 50f));
        var bucketA = AOEShapeDebug.GetColorBucket(shapeA, new WPos(0f, 0f));
        var bucketB = AOEShapeDebug.GetColorBucket(shapeB, new WPos(100f, 50f));

        Assert.Equal(bucketA, bucketB);
    }

    [Fact]
    public void BuildDisplayObjectsFor_AllShapes_ReturnsOutline()
    {
        var origin = new WPos(0f, 0f);
        var rotation = 45f.Degrees();
        const float height = 0f;
        const uint color = 0xFFFFFFFF;

        AOEShape[] shapes =
        [
            new AOEShapeCircle(5f),
            new AOEShapeDonut(2f, 6f),
            new AOEShapeCone(8f, 30f.Degrees()),
            new AOEShapeDonutSector(2f, 6f, 45f.Degrees()),
            new AOEShapeRect(8f, 2f, 3f),
            new AOEShapeCross(6f, 1.5f),
            new AOEShapeTriCone(6f, 25f.Degrees()),
            new AOEShapeCapsule(1.5f, 6f),
            new AOEShapeArcCapsule(1.5f, 90f.Degrees(), new WPos(8f, 0f))
        ];

        foreach (var shape in shapes)
        {
            var objects = AOEShapeDebug.BuildDisplayObjectsFor(shape, origin, rotation, height, color);
            Assert.NotEmpty(objects);
        }
    }
}
