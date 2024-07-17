using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class FieldOutputProperty<T> : OutputProperty<Func<FieldContext, T>>
{
    internal FieldOutputProperty(Node node, string internalName, string displayName, Func<FieldContext, T> defaultValue) : base(node, internalName, displayName, defaultValue)
    {
    }
}
