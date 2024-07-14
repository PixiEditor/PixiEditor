using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class FieldOutputProperty<T> : OutputProperty<Func<IFieldContext, T>>
{
    internal FieldOutputProperty(Node node, string internalName, string displayName, Func<IFieldContext, T> defaultValue) : base(node, internalName, displayName, defaultValue)
    {
    }
}
