using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class RenderInputProperty : InputProperty<Painter?>
{
    internal RenderInputProperty(Node node, string internalName, string displayName, Painter? defaultValue) : base(node, internalName, displayName, defaultValue)
    {
        
    }
}
