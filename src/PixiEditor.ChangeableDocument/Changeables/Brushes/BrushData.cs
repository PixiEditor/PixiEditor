using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.Changeables.Brushes;

public struct BrushData
{
    public IReadOnlyNodeGraph BrushGraph { get; set; }
    public bool AntiAliasing { get; set; }
    public float StrokeWidth { get; set; }
    public Guid TargetBrushNodeId { get; set; }


    public BrushData(IReadOnlyNodeGraph brushGraph, Guid targetBrushNodeId)
    {
        BrushGraph = brushGraph;
        TargetBrushNodeId = targetBrushNodeId;
    }
}
