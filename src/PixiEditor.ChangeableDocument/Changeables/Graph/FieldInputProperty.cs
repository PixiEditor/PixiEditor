using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class FieldInputProperty<T> : InputProperty<Func<FieldContext, T>>, IFieldInputProperty
{
    private T? constantNonOverrideValue;
    
    internal FieldInputProperty(Node node, string internalName, string displayName, T defaultValue) : base(node, internalName, displayName, null)
    {
        constantNonOverrideValue = defaultValue;
        NonOverridenValue = _ => constantNonOverrideValue;
    }

    object? IFieldInputProperty.GetFieldConstantValue() => constantNonOverrideValue;

    void IFieldInputProperty.SetFieldConstantValue(object? value)
    {
        constantNonOverrideValue = (T)value;
    }
}
