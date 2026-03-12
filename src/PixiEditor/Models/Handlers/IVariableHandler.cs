namespace PixiEditor.Models.Handlers;

public interface IVariableHandler
{
    public Type Type { get; }
    public object Value { get; }
    public string Name { get; }
}
