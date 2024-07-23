namespace PixiEditor.ChangeableDocument.Changeables.Graph;

internal interface IFuncInputProperty
{
    object? GetFuncConstantValue();
    
    void SetFuncConstantValue(object? value);
}
