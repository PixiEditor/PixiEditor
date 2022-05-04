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
        image.EnqueueDrawRectangle(new(new(5, 5), new(80, 80), 2, SKColors.AliceBlue, SKColors.Snow));
        using (Chunk target = Chunk.Create())
        {
            image.DrawMostUpToDateChunkOn(new(0, 0), ChunkyImageLib.DataHolders.ChunkResolution.Full, target.Surface.SkiaSurface, new(0, 0));
            image.CancelChanges();
            image.EnqueueResize(new(ChunkyImage.ChunkSize * 4, ChunkyImage.ChunkSize * 4));
            image.EnqueueDrawRectangle(new(new(0, 0), image.CommittedSize, 2, SKColors.AliceBlue, SKColors.Snow, SKBlendMode.Multiply));
            image.CommitChanges();
            image.SetBlendMode(SKBlendMode.Overlay);
            image.EnqueueDrawRectangle(new(new(0, 0), image.CommittedSize, 2, SKColors.AliceBlue, SKColors.Snow, SKBlendMode.Multiply));
            image.EnqueueDrawRectangle(new(new(0, 0), image.CommittedSize, 2, SKColors.AliceBlue, SKColors.Snow));
            image.CommitChanges();
            image.SetBlendMode(SKBlendMode.Screen);
            image.EnqueueDrawRectangle(new(new(0, 0), image.CommittedSize, 2, SKColors.AliceBlue, SKColors.Snow));
            image.CancelChanges();
            image.SetBlendMode(SKBlendMode.SrcOver);
            image.EnqueueDrawRectangle(new(new(0, 0), image.CommittedSize, 2, SKColors.AliceBlue, SKColors.Snow));
        }
        image.Dispose();

        Assert.Equal(0, Chunk.ChunkCounter);
    }
}
