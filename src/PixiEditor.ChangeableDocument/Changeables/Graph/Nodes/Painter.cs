using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core.Surfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class Painter(Action<RenderContext, DrawingSurface> paint)
{
    public Action<RenderContext, DrawingSurface> Paint { get; } = paint;

    public override int GetHashCode()
    {
        return Paint.GetHashCode();
    }
}
