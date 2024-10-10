﻿using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IChunkRenderable
{
    public void RenderChunk(VecI chunkPos, ChunkResolution resolution, KeyFrameTime frameTime);
}