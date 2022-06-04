using System.Collections.Generic;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using Xunit;

namespace ChunkyImageLibTest;
public class ImageOperationTests
{
    [Fact]
    public void FindAffectedChunks_SingleChunk_ReturnsSingleChunk()
    {
        using Surface testImage = new Surface((ChunkyImage.FullChunkSize, ChunkyImage.FullChunkSize));
        using ImageOperation operation = new((ChunkyImage.FullChunkSize, ChunkyImage.FullChunkSize), testImage);
        var chunks = operation.FindAffectedChunks();
        Assert.Equal(new HashSet<VecI>() { new(1, 1) }, chunks);
    }
}
