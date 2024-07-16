namespace PixiEditor.ChangeableDocument.Changeables.Graph;

internal interface IFieldInputProperty
{
    object? GetFieldConstantValue();
    
    void SetFieldConstantValue(object? value);
}
