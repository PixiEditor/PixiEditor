namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public interface IReadOnlyGraphConstant
{
    public Guid Id { get; }
    
    public object? Value { get; }
    
    public Type Type { get; }
}
