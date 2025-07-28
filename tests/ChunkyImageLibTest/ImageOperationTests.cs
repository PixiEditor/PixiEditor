using System.Collections.Generic;
using ChunkyImageLib;
using ChunkyImageLib.Operations;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Numerics;
using Drawie.Skia;
using PixiEditor.Tests;
using Xunit;

namespace ChunkyImageLibTest;
public class ImageOperationTests : PixiEditorTest
{

    [Fact]
    public void FindAffectedChunks_SingleChunk_ReturnsSingleChunk()
    {
        using Surface testImage = new Surface((ChunkyImage.FullChunkSize, ChunkyImage.FullChunkSize));
        using ImageOperation operation = new((ChunkyImage.FullChunkSize, ChunkyImage.FullChunkSize), testImage);
        var chunks = operation.FindAffectedArea(new(ChunkyImage.FullChunkSize)).Chunks;
        Assert.Equal(new HashSet<VecI>() { new(1, 1) }, chunks);
    }
}
