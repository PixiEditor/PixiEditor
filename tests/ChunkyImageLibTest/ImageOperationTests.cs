using System.Collections.Generic;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Skia;
using PixiEditor.Numerics;
using Xunit;

namespace ChunkyImageLibTest;
public class ImageOperationTests
{
    public ImageOperationTests()
    {
        try
        {
            DrawingBackendApi.SetupBackend(new SkiaDrawingBackend(), null);
        }
        catch { }
    }

    [Fact]
    public void FindAffectedChunks_SingleChunk_ReturnsSingleChunk()
    {
        using Surface testImage = new Surface((ChunkyImage.FullChunkSize, ChunkyImage.FullChunkSize));
        using ImageOperation operation = new((ChunkyImage.FullChunkSize, ChunkyImage.FullChunkSize), testImage);
        var chunks = operation.FindAffectedArea(new(ChunkyImage.FullChunkSize)).Chunks;
        Assert.Equal(new HashSet<VecI>() { new(1, 1) }, chunks);
    }
}
