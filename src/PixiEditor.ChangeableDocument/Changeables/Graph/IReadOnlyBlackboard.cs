namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public interface IReadOnlyBlackboard
{
    public IReadOnlyVariable? GetVariable(string variableName);
}
