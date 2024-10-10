using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Surfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class RenderInputProperty : InputProperty<DrawingSurface?>
{
    Func<RenderContext, DrawingSurface> getRenderTarget;
    internal RenderInputProperty(Node node, string internalName, string displayName, DrawingSurface? defaultValue, Func<RenderContext, DrawingSurface> getRenderTarget) : base(node, internalName, displayName, defaultValue)
    {
        this.getRenderTarget = getRenderTarget;
    }

    public DrawingSurface GetRenderTarget(RenderContext context)
    {
        return getRenderTarget(context);
    }
}
