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
            Vector2i act = OperationHelper.GetChunkPosBiased(new(x, y), positiveX, positiveY, 32);
            Assert.Equal(expX, act.X);
            Assert.Equal(expY, act.Y);
        }
    }
}
