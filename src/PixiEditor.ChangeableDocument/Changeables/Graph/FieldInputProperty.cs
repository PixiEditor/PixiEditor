using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class FieldInputProperty<T> : InputProperty<Func<FieldContext, T>>
{
    internal FieldInputProperty(Node node, string internalName, string displayName, Func<FieldContext, T> defaultValue) : base(node, internalName, displayName, defaultValue)
    {
    }
}
