using Drawie.Backend.Core;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public interface IReadOnlyBlackboard : ICacheable
{
    public IReadOnlyVariable? GetVariable(string variableName);
    public IReadOnlyDictionary<string, IReadOnlyVariable> Variables { get; }
}
