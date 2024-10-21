﻿using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Surfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class Painter(Action<RenderContext, DrawingSurface> paint)
{
    public Action<RenderContext, DrawingSurface> Paint { get; } = paint;
}