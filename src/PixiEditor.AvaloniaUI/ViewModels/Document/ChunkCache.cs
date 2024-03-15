using System.Collections.Generic;
using System.Linq;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

public class ChunkCache : Dictionary<ChunkResolution, ChunkSet>
{
    public ChunkCache()
    {
        this[ChunkResolution.Full] = new ChunkSet();
        this[ChunkResolution.Half] = new ChunkSet();
        this[ChunkResolution.Quarter] = new ChunkSet();
        this[ChunkResolution.Eighth] = new ChunkSet();
    }
}

public class ChunkSet : Dictionary<VecI, Chunk>
{

}

