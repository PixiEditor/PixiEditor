using System.Collections;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public interface IReadOnlyBlackboard
{
    public IReadOnlyVariable? GetVariable(string variableName);
    public IReadOnlyDictionary<string, IReadOnlyVariable> Variables { get; }
}
