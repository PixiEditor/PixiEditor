using System;
using System.Collections.Generic;
using ChunkyImageLib;
using ChunkyImageLib.Operations;
using Drawie.Numerics;
using Xunit;

namespace ChunkyImageLibTest;

public class OperationHelperTests
{
    [Theory]
    [InlineData(0, 0, 0, 0)]
    [InlineData(-1, -1, -1, -1)]
    [InlineData(32, 32, 1, 1)]
    [InlineData(-32, -32, -1, -1)]
    [InlineData(-33, -33, -2, -2)]
    public void GetChunkPos_32ChunkSize_ReturnsCorrectValues(int x, int y, int expX, int expY)
    {
        VecI act = OperationHelper.GetChunkPos(new(x, y), 32);
        Assert.Equal(expX, act.X);
        Assert.Equal(expY, act.Y);
    }

    [Theory]
    [InlineData(0, 0, true, true, 0, 0)]
    [InlineData(0, 0, false, true, -1, 0)]
    [InlineData(0, 0, true, false, 0, -1)]
    [InlineData(0, 0, false, false, -1, -1)]
    [InlineData(48.5, 48.5, true, true, 1, 1)]
    [InlineData(48.5, 48.5, false, true, 1, 1)]
    [InlineData(48.5, 48.5, true, false, 1, 1)]
    [InlineData(48.5, 48.5, false, false, 1, 1)]
    public void GetChunkPosBiased_32ChunkSize_ReturnsCorrectValues(double x, double y, bool positiveX, bool positiveY, int expX, int expY)
    {
        VecI act = OperationHelper.GetChunkPosBiased(new(x, y), positiveX, positiveY, 32);
        Assert.Equal(expX, act.X);
        Assert.Equal(expY, act.Y);
    }

    [Fact]
    public void CreateStretchedHexagon_NonStretched_ReturnsCorrectQuads()
    {
        var (left, right) = OperationHelper.CreateStretchedHexagon((-3, 5), 10 / Math.Sqrt(3), 1);
        Assert.Equal(right.TopLeft.X, left.TopRight.X, 6);
        Assert.Equal(right.BottomLeft.X, left.BottomRight.X, 6);

        Assert.Equal(-3, right.BottomLeft.X, 2);
        Assert.Equal(10.774, right.BottomLeft.Y, 2);

        Assert.Equal(2, right.BottomRight.X, 2);
        Assert.Equal(7.887, right.BottomRight.Y, 2);

        Assert.Equal(2, right.TopRight.X, 2);
        Assert.Equal(2.113, right.TopRight.Y, 2);

        Assert.Equal(-3, right.TopLeft.X, 2);
        Assert.Equal(-0.774, right.TopLeft.Y, 2);

        Assert.Equal(-8, left.TopLeft.X, 2);
        Assert.Equal(2.113, left.TopLeft.Y, 2);

        Assert.Equal(-8, left.BottomLeft.X, 2);
        Assert.Equal(7.887, left.BottomLeft.Y, 2);
    }

    [Fact]
    public void CreateStretchedHexagon_Stretched_ReturnsCorrectQuads()
    {
        const double x = -7;
        const double stretch = 4;
        var (left, right) = OperationHelper.CreateStretchedHexagon((x, 1), 12 / Math.Sqrt(3), stretch);
        Assert.Equal(right.TopLeft.X, left.TopRight.X, 6);
        Assert.Equal(right.BottomLeft.X, left.BottomRight.X, 6);

        Assert.Equal(-7, right.BottomLeft.X, 2);
        Assert.Equal(7.928, right.BottomLeft.Y, 2);

        Assert.Equal((-1 - x) * stretch + x, right.BottomRight.X, 2);
        Assert.Equal(4.464, right.BottomRight.Y, 2);

        Assert.Equal((-1 - x) * stretch + x, right.TopRight.X, 2);
        Assert.Equal(-2.464, right.TopRight.Y, 2);

        Assert.Equal(-7, right.TopLeft.X, 2);
        Assert.Equal(-5.928, right.TopLeft.Y, 2);

        Assert.Equal((-13 - x) * stretch + x, left.TopLeft.X, 2);
        Assert.Equal(-2.464, left.TopLeft.Y, 2);

        Assert.Equal((-13 - x) * stretch + x, left.BottomLeft.X, 2);
        Assert.Equal(4.464, left.BottomLeft.Y, 2);
    }

    [Fact]
    public void FindChunksTouchingEllipse_EllipseSpanningTwoChunks_FindsChunks()
    {
        int cS = ChunkyImage.FullChunkSize;
        var chunks = OperationHelper.FindChunksTouchingEllipse((cS, cS / 2.0), cS / 2.0, cS / 4.0, cS);
        Assert.Equal(new HashSet<VecI>() { (0, 0), (1, 0) }, chunks);
    }
}
