using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using Xunit;

namespace ChunkyImageLibTest
{
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
            Vector2i act = OperationHelper.GetChunkPos(new(x, y), 32);
            Assert.Equal(expX, act.X);
            Assert.Equal(expY, act.Y);
        }
    }
}
