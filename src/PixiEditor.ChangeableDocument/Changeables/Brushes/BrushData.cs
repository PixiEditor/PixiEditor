using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Surfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering.ContextData;

namespace PixiEditor.ChangeableDocument.Changeables.Brushes;

public struct BrushData
{
    public IReadOnlyNodeGraph BrushGraph { get; set; }
    public bool AntiAliasing { get; set; }
    public float Spacing { get; set; }
    public float StrokeWidth { get; set; }

    public BlendMode BlendMode { get; set; } = BlendMode.SrcOver;


    public BrushData(IReadOnlyNodeGraph brushGraph)
    {
        BrushGraph = brushGraph;
    }
}
