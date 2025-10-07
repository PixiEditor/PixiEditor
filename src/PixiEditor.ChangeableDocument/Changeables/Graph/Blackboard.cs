namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class Blackboard : IReadOnlyBlackboard
{
    private Dictionary<string, Variable> variables = new Dictionary<string, Variable>();
    public IReadOnlyDictionary<string, Variable> Variables => variables;

    public void SetVariable(string name, Type type, object value)
    {
        if (variables.ContainsKey(name))
        {
            variables[name].Type = type;
            variables[name].Value = value;
        }
        else
        {
            variables[name] = new Variable { Type = type, Value = value };
        }
    }

    public Variable? GetVariable(string name)
    {
        return variables.GetValueOrDefault(name);
    }

    public bool RemoveVariable(string name)
    {
        return variables.Remove(name);
    }

    IReadOnlyVariable IReadOnlyBlackboard.GetVariable(string variableName)
    {
        return GetVariable(variableName);
    }
}

public class Variable : IReadOnlyVariable
{
    public Type Type { get; set; }
    public object Value { get; set; }
}

public interface IReadOnlyVariable
{
    public Type Type { get; }
    public object Value { get; }
}
