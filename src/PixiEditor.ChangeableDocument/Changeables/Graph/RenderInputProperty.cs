using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core.Surfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class RenderInputProperty : InputProperty<Painter?>
{
    internal RenderInputProperty(Node node, string internalName, string displayName, Painter? defaultValue) : base(node, internalName, displayName, defaultValue)
    {
        
    }
}
