using Drawie.Backend.Core.Surfaces.ImageData;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IChunkRenderable
{
    public void RenderChunk(VecI chunkPos, ChunkResolution resolution, KeyFrameTime frameTime, ColorSpace processingColorSpace);
}
