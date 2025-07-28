using System.Collections.Generic;
using ChunkyImageLib;
using ChunkyImageLib.Operations;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Numerics;
using Drawie.Skia;
using PixiEditor.Tests;
using Xunit;

namespace ChunkyImageLibTest;

public class RectangleOperationTests : PixiEditorTest
{
    const int chunkSize = ChunkPool.FullChunkSize;

// to keep expected rectangles aligned
#pragma warning disable format
    [Fact]
    public void FindAffectedArea_SmallStrokeOnly_FindsCorrectChunks()
    {
        var (x, y, w, h) = (chunkSize / 2, chunkSize / 2, chunkSize, chunkSize);
        RectangleOperation operation = new(new(new(x, y), new(w, h), 0, 0, 1, Colors.Black, Colors.Transparent));

        HashSet<VecI> expected = new() { new(0, 0) };
        var actual = operation.FindAffectedArea(new(chunkSize)).Chunks;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void FindAffectedArea_2by2StrokeOnly_FindsCorrectChunks()
    {
        var (x, y, w, h) = (0, 0, chunkSize * 2, chunkSize * 2);
        RectangleOperation operation = new(new(new(x, y), new(w, h), 0, 0, 1, Colors.Black, Colors.Transparent));

        HashSet<VecI> expected = new() { new(-1, -1), new(0, -1), new(-1, 0), new(0, 0) };
        var actual = operation.FindAffectedArea(new(chunkSize)).Chunks;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void FindAffectedArea_3x3PositiveStrokeOnly_FindsCorrectChunks()
    {
        var (x, y, w, h) = (2 * chunkSize + chunkSize / 2, 2 * chunkSize + chunkSize / 2, chunkSize * 2, chunkSize * 2);
        RectangleOperation operation = new(new(new(x, y), new(w, h), 0, 0, 1, Colors.Black, Colors.Transparent));

        HashSet<VecI> expected = new()
        {
            new(1, 1), new(2, 1), new(3, 1),
            new(1, 2), new(3, 2),
            new(1, 3), new(2, 3), new(3, 3),
        };
        var actual = operation.FindAffectedArea(new(chunkSize)).Chunks;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void FindAffectedArea_3x3NegativeStrokeOnly_FindsCorrectChunks()
    {
        var (x, y, w, h) = (-chunkSize * 2 - chunkSize / 2, -chunkSize * 2 - chunkSize / 2, chunkSize * 2,
            chunkSize * 2);
        RectangleOperation operation = new(new(new(x, y), new(w, h), 0, 0, 1, Colors.Black, Colors.Transparent));

        HashSet<VecI> expected = new()
        {
            new(-4, -4), new(-3, -4), new(-2, -4),
            new(-4, -3), new(-2, -3),
            new(-4, -2), new(-3, -2), new(-2, -2),
        };
        var actual = operation.FindAffectedArea(new(chunkSize)).Chunks;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void FindAffectedArea_3x3PositiveFilled_FindsCorrectChunks()
    {
        var (x, y, w, h) = (2 * chunkSize + chunkSize / 2, 2 * chunkSize + chunkSize / 2, chunkSize * 2, chunkSize * 2);
        RectangleOperation operation = new(new(new(x, y), new(w, h), 0, 0, 1, Colors.Black, Colors.White));

        HashSet<VecI> expected = new()
        {
            new(1, 1), new(2, 1), new(3, 1),
            new(1, 2), new(2, 2), new(3, 2),
            new(1, 3), new(2, 3), new(3, 3),
        };
        var actual = operation.FindAffectedArea(new(chunkSize)).Chunks;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void FindAffectedArea_ThickPositiveStroke_FindsCorrectChunks()
    {
        var (x, y, w, h) = (2 * chunkSize + chunkSize / 2, 2 * chunkSize + chunkSize / 2, chunkSize * 4, chunkSize * 4);
        RectangleOperation operation =
            new(new(new(x, y), new(w, h), 0, 0, chunkSize, Colors.Black, Colors.Transparent));

        HashSet<VecI> expected = new()
        {
            new(0, 0), new(1, 0), new(2, 0), new(3, 0), new(4, 0),
            new(0, 1), new(1, 1), new(2, 1), new(3, 1), new(4, 1),
            new(0, 2), new(1, 2), new(3, 2), new(4, 2),
            new(0, 3), new(1, 3), new(2, 3), new(3, 3), new(4, 3),
            new(0, 4), new(1, 4), new(2, 4), new(3, 4), new(4, 4),
        };
        var actual = operation.FindAffectedArea(new(chunkSize)).Chunks;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void FindAffectedArea_SmallButThick_FindsCorrectChunks()
    {
        var (x, y, w, h) = (chunkSize / 2f - 0.5, chunkSize / 2f - 0.5, 1, 1);
        RectangleOperation operation = new(new(new(x, y), new(w, h), 0, 0, chunkSize, Colors.Black, Colors.White));

        HashSet<VecI> expected = new() { new(0, 0) };
        var actual = operation.FindAffectedArea(new(chunkSize)).Chunks;

        Assert.Equal(expected, actual);
    }
#pragma warning restore format
}