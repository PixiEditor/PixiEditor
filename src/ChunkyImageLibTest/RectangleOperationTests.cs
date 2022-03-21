using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using SkiaSharp;
using System.Collections.Generic;
using Xunit;

namespace ChunkyImageLibTest
{
    public class RectangleOperationTests
    {
        const int chunkSize = ChunkPool.FullChunkSize;
// to keep expected rectangles aligned
#pragma warning disable format
        [Fact]
        public void FindAffectedChunks_SmallStrokeOnly_FindsCorrectChunks()
        {
            var (x, y, w, h) = (0, 0, chunkSize, chunkSize);
            RectangleOperation operation = new(new(new(x, y), new(w, h), 1, SKColors.Black, SKColors.Transparent));

            HashSet<Vector2i> expected = new() { new(0, 0) };
            var actual = operation.FindAffectedChunks();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FindAffectedChunks_2by2StrokeOnly_FindsCorrectChunks()
        {
            var (x, y, w, h) = (-chunkSize, -chunkSize, chunkSize * 2, chunkSize * 2);
            RectangleOperation operation = new(new(new(x, y), new(w, h), 1, SKColors.Black, SKColors.Transparent));

            HashSet<Vector2i> expected = new() { new(-1, -1), new(0, -1), new(-1, 0), new(0, 0) };
            var actual = operation.FindAffectedChunks();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FindAffectedChunks_3x3PositiveStrokeOnly_FindsCorrectChunks()
        {
            var (x, y, w, h) = (chunkSize + chunkSize / 2, chunkSize + chunkSize / 2, chunkSize * 2, chunkSize * 2);
            RectangleOperation operation = new(new(new(x, y), new(w, h), 1, SKColors.Black, SKColors.Transparent));

            HashSet<Vector2i> expected = new()
            {
                new(1, 1), new(2, 1), new(3, 1),
                new(1, 2),            new(3, 2),
                new(1, 3), new(2, 3), new(3, 3),
            };
            var actual = operation.FindAffectedChunks();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FindAffectedChunks_3x3NegativeStrokeOnly_FindsCorrectChunks()
        {
            var (x, y, w, h) = (-chunkSize * 3 - chunkSize / 2, -chunkSize * 3 - chunkSize / 2, chunkSize * 2, chunkSize * 2);
            RectangleOperation operation = new(new(new(x, y), new(w, h), 1, SKColors.Black, SKColors.Transparent));

            HashSet<Vector2i> expected = new()
            {
                new(-4, -4), new(-3, -4), new(-2, -4),
                new(-4, -3),              new(-2, -3),
                new(-4, -2), new(-3, -2), new(-2, -2),
            };
            var actual = operation.FindAffectedChunks();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FindAffectedChunks_3x3PositiveFilled_FindsCorrectChunks()
        {
            var (x, y, w, h) = (chunkSize + chunkSize / 2, chunkSize + chunkSize / 2, chunkSize * 2, chunkSize * 2);
            RectangleOperation operation = new(new(new(x, y), new(w, h), 1, SKColors.Black, SKColors.White));

            HashSet<Vector2i> expected = new()
            {
                new(1, 1), new(2, 1), new(3, 1), 
                new(1, 2), new(2, 2), new(3, 2),
                new(1, 3), new(2, 3), new(3, 3),
            };
            var actual = operation.FindAffectedChunks();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FindAffectedChunks_ThickPositiveStroke_FindsCorrectChunks()
        {
            var (x, y, w, h) = (chunkSize / 2, chunkSize / 2, chunkSize * 4, chunkSize * 4);
            RectangleOperation operation = new(new(new(x, y), new(w, h), chunkSize, SKColors.Black, SKColors.Transparent));

            HashSet<Vector2i> expected = new()
            {
                new(0, 0), new(1, 0), new(2, 0), new(3, 0), new(4, 0),
                new(0, 1), new(1, 1), new(2, 1), new(3, 1), new(4, 1),
                new(0, 2), new(1, 2),            new(3, 2), new(4, 2),
                new(0, 3), new(1, 3), new(2, 3), new(3, 3), new(4, 3),
                new(0, 4), new(1, 4), new(2, 4), new(3, 4), new(4, 4),
            };
            var actual = operation.FindAffectedChunks();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FindAffectedChunks_SmallButThick_FindsCorrectChunks()
        {
            var (x, y, w, h) = (chunkSize / 2, chunkSize / 2, 1, 1);
            RectangleOperation operation = new(new(new(x, y), new(w, h), 256, SKColors.Black, SKColors.White));

            HashSet<Vector2i> expected = new() { new(0, 0) };
            var actual = operation.FindAffectedChunks();

            Assert.Equal(expected, actual);
        }
#pragma warning restore format
    }
}
