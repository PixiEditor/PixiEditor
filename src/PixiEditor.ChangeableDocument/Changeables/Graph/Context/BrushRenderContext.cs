﻿using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Brushes;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Context;

internal class BrushRenderContext : RenderContext
{
    public BrushData BrushData { get; }
    public Texture TargetSampledTexture { get; }

    public BrushRenderContext(DrawingSurface renderSurface, KeyFrameTime frameTime, ChunkResolution chunkResolution, VecI renderOutputSize, VecI documentSize, ColorSpace processingColorSpace, SamplingOptions desiredSampling, BrushData brushData, Texture targetSampledTexture, double opacity = 1) : base(renderSurface, frameTime, chunkResolution, renderOutputSize, documentSize, processingColorSpace, desiredSampling, opacity)
    {
        BrushData = brushData;
        TargetSampledTexture = targetSampledTexture;
    }
}
