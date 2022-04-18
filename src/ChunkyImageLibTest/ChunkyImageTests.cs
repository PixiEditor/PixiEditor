using ChunkyImageLib;
using SkiaSharp;
using Xunit;

namespace ChunkyImageLibTest;
public class ChunkyImageTests
{
    [Fact]
    public void ChunkyImage_Dispose_ReturnsAllChunks()
    {
        ChunkyImage image = new ChunkyImage(new(ChunkyImage.ChunkSize, ChunkyImage.ChunkSize));
        image.DrawRectangle(new(new(5, 5), new(80, 80), 2, SKColors.AliceBlue, SKColors.Snow));
        using (Chunk target = Chunk.Create())
        {
            image.DrawLatestChunkOn(new(0, 0), ChunkyImageLib.DataHolders.ChunkResolution.Full, target.Surface.SkiaSurface, new(0, 0));
            image.CancelChanges();
            image.Resize(new(ChunkyImage.ChunkSize * 4, ChunkyImage.ChunkSize * 4));
            image.CommitChanges();
            image.DrawRectangle(new(new(0, 0), image.CommittedSize, 2, SKColors.AliceBlue, SKColors.Snow));
            image.CommitChanges();
            image.DrawRectangle(new(new(0, 0), image.CommittedSize, 2, SKColors.AliceBlue, SKColors.Snow));
            image.CancelChanges();
        }
        image.Dispose();

        Assert.Equal(0, Chunk.ChunkCounter);
    }
}
