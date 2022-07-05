using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using SkiaSharp;
using Xunit;

namespace ChunkyImageLibTest;
public class ChunkyImageTests
{
    [Fact]
    public void Dispose_ComplexImage_ReturnsAllChunks()
    {
        ChunkyImage image = new ChunkyImage(new(ChunkyImage.FullChunkSize, ChunkyImage.FullChunkSize));
        image.EnqueueDrawRectangle(new(new(5, 5), new(80, 80), 0, 2, SKColors.AliceBlue, SKColors.Snow));
        using (Chunk target = Chunk.Create())
        {
            image.DrawMostUpToDateChunkOn(new(0, 0), ChunkResolution.Full, target.Surface.SkiaSurface, VecI.Zero);
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

    [Fact]
    public void GetCommittedPixel_RedImage_ReturnsRedPixel()
    {
        const int chunkSize = ChunkyImage.FullChunkSize;
        ChunkyImage image = new ChunkyImage(new VecI(chunkSize * 2));
        image.EnqueueDrawRectangle
            (new ShapeData(new VecD(chunkSize), new VecD(chunkSize * 2), 0, 0, SKColors.Transparent, SKColors.Red));
        image.CommitChanges();
        Assert.Equal(SKColors.Red, image.GetCommittedPixel(new VecI(chunkSize + chunkSize / 2)));
        image.Dispose();
        Assert.Equal(0, Chunk.ChunkCounter);
    }

    [Fact]
    public void GetMostUpToDatePixel_BlendModeSrc_ReturnsCorrectPixel()
    {
        const int chunkSize = ChunkyImage.FullChunkSize;
        ChunkyImage image = new ChunkyImage(new VecI(chunkSize * 2));
        image.EnqueueDrawRectangle
            (new ShapeData(new VecD(chunkSize), new VecD(chunkSize * 2), 0, 0, SKColors.Transparent, SKColors.Red));
        Assert.Equal(SKColors.Red, image.GetMostUpToDatePixel(new VecI(chunkSize + chunkSize / 2)));
        image.Dispose();
        Assert.Equal(0, Chunk.ChunkCounter);
    }
    
    [Fact]
    public void GetMostUpToDatePixel_BlendModeSrcOver_ReturnsCorrectPixel()
    {
        const int chunkSize = ChunkyImage.FullChunkSize;
        ChunkyImage image = new ChunkyImage(new VecI(chunkSize * 2));
        image.EnqueueDrawRectangle
            (new ShapeData(new VecD(chunkSize), new VecD(chunkSize * 2), 0, 0, SKColors.Transparent, SKColors.Red));
        image.CommitChanges();
        image.SetBlendMode(SKBlendMode.SrcOver);
        image.EnqueueDrawRectangle(new ShapeData(
            new VecD(chunkSize),
            new VecD(chunkSize * 2),
            0, 
            0,
            SKColors.Transparent,
            new SKColor(0, 255, 0, 128)));
        Assert.Equal(new SKColor(127, 128, 0), image.GetMostUpToDatePixel(new VecI(chunkSize + chunkSize / 2)));
        image.Dispose();
        Assert.Equal(0, Chunk.ChunkCounter);
    }

    [Fact]
    public void EnqueueDrawRectangle_OutsideOfImage_PartsAreNotDrawn()
    {
        const int chunkSize = ChunkyImage.FullChunkSize;
        using ChunkyImage image = new(new VecI(chunkSize));
        image.EnqueueDrawRectangle(new ShapeData(
                VecD.Zero,
                new VecD(chunkSize * 10),
                0,
                0,
                SKColors.Transparent,
                SKColors.Red));
        image.CommitChanges();
        Assert.Collection(
            image.FindAllChunks(), 
            elem => Assert.Equal(VecI.Zero, elem));
    }
}
