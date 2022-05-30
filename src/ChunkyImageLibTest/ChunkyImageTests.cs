using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using SkiaSharp;
using Xunit;

namespace ChunkyImageLibTest;
public class ChunkyImageTests
{
    [Fact]
    public void ChunkyImage_Dispose_ReturnsAllChunks()
    {
        ChunkyImage image = new ChunkyImage(new(ChunkyImage.FullChunkSize, ChunkyImage.FullChunkSize));
        image.EnqueueDrawRectangle(new(new(5, 5), new(80, 80), 0, 2, SKColors.AliceBlue, SKColors.Snow));
        using (Chunk target = Chunk.Create())
        {
            image.DrawMostUpToDateChunkOn(new(0, 0), ChunkyImageLib.DataHolders.ChunkResolution.Full, target.Surface.SkiaSurface, VecI.Zero);
            image.CancelChanges();
            image.EnqueueResize(new(ChunkyImage.FullChunkSize * 4, ChunkyImage.FullChunkSize * 4));
            image.EnqueueDrawRectangle(new(VecD.Zero, image.CommittedSize, 0, 2, SKColors.AliceBlue, SKColors.Snow, SKBlendMode.Multiply));
            image.CommitChanges();
            image.SetBlendMode(SKBlendMode.Overlay);
            image.EnqueueDrawRectangle(new(VecD.Zero, image.CommittedSize, 0, 2, SKColors.AliceBlue, SKColors.Snow, SKBlendMode.Multiply));
            image.EnqueueDrawRectangle(new(VecD.Zero, image.CommittedSize, 0, 2, SKColors.AliceBlue, SKColors.Snow));
            image.CommitChanges();
            image.SetBlendMode(SKBlendMode.Screen);
            image.EnqueueDrawRectangle(new(VecD.Zero, image.CommittedSize, 0, 2, SKColors.AliceBlue, SKColors.Snow));
            image.CancelChanges();
            image.SetBlendMode(SKBlendMode.SrcOver);
            image.EnqueueDrawRectangle(new(VecD.Zero, image.CommittedSize, 0, 2, SKColors.AliceBlue, SKColors.Snow));
        }
        image.Dispose();

        Assert.Equal(0, Chunk.ChunkCounter);
    }
}
