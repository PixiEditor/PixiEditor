using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class FuncInputProperty<T> : InputProperty<Func<FuncContext, T>>, IFuncInputProperty
{
    private T? constantNonOverrideValue;
    
    internal FuncInputProperty(Node node, string internalName, string displayName, T defaultValue) : base(node, internalName, displayName, null)
    {
        constantNonOverrideValue = defaultValue;
        NonOverridenValue = _ => constantNonOverrideValue;
    }

    object? IFuncInputProperty.GetFuncConstantValue() => constantNonOverrideValue;

    void IFuncInputProperty.SetFuncConstantValue(object? value)
    {
        constantNonOverrideValue = (T)value;
    }
}
