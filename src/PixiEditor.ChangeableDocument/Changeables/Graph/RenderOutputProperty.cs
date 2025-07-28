using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core.Surfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class RenderOutputProperty : OutputProperty<Painter>
{ 
    public Func<Painter?>? FirstInChain { get; set; }
    public Func<Painter?>? NextInChain { get; set; }
    
    internal RenderOutputProperty(Node node, string internalName, string displayName, Painter defaultValue) : base(node, internalName, displayName, defaultValue)
    {
        
    }

    public void ChainToPainterValue()
    {
        if (FirstInChain != null)
        {
            Value = new Painter((ctx, surface) =>
            {
                FirstInChain()?.Paint(ctx, surface);
                NextInChain()?.Paint(ctx, surface);
            });
        }
        else
        {
            Value = NextInChain?.Invoke();
        }
    }
}
