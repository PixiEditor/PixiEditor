using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using System.Collections.Generic;
using Xunit;

namespace ChunkyImageLibTest
{
    public class ClearRegionOperationTests
    {
        const int chunkSize = ChunkPool.FullChunkSize;
        [Fact]
        public void FindAffectedChunks_SingleChunk_ReturnsSingleChunk()
        {
            ClearRegionOperation operation = new(new(chunkSize, chunkSize), new(chunkSize, chunkSize));
            var expected = new HashSet<Vector2i>() { new(1, 1) };
            var actual = operation.FindAffectedChunks();
            Assert.Equal(expected, actual);
        }

 // to keep expected aligned
#pragma warning disable format
        [Fact]
        public void FindAffectedChunks_BigArea_ReturnsCorrectChunks()
        {
            int from = -chunkSize - chunkSize / 2;
            int to = chunkSize + chunkSize / 2;
            ClearRegionOperation operation = new(new(from, from), new(to - from, to - from));
            var expected = new HashSet<Vector2i>() 
            { 
                new(-2, -2), new(-1, -2), new(0, -2), new(1, -2),
                new(-2, -1), new(-1, -1), new(0, -1), new(1, -1),
                new(-2, -0), new(-1, -0), new(0, -0), new(1, -0),
                new(-2,  1), new(-1,  1), new(0,  1), new(1,  1),
            };
            var actual = operation.FindAffectedChunks();
            Assert.Equal(expected, actual);
        }
#pragma warning restore format
    }
}
