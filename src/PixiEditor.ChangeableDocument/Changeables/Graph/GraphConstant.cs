namespace PixiEditor.ChangeableDocument.Changeables.Graph;

internal class GraphConstant(Guid id, Type type) : IReadOnlyGraphConstant
{
    public Guid Id { get; set; } = id;
    
    public object Value { get; set; }

    public Type Type { get; set; } = type;
}
