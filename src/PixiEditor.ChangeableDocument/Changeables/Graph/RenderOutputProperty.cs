using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Surfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class RenderOutputProperty : OutputProperty<DrawingSurface>
{
    internal RenderOutputProperty(Node node, string internalName, string displayName, DrawingSurface defaultValue) : base(node, internalName, displayName, defaultValue)
    {
        
    }
    
    public DrawingSurface GetFirstRenderTarget(RenderContext ctx)
    {
        foreach (var connection in Connections)
        {
            if (connection is RenderInputProperty renderInput)
            {
                var target = renderInput.GetRenderTarget(ctx);
                if (target != null)
                {
                    return target;
                }
            }
        }

        return null;
    }
}
