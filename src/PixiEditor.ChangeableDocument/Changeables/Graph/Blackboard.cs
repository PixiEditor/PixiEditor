namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class Blackboard : IReadOnlyBlackboard
{
    private Dictionary<string, Variable> variables = new Dictionary<string, Variable>();
    public IReadOnlyDictionary<string, Variable> Variables => variables;

    IReadOnlyDictionary<string, IReadOnlyVariable> IReadOnlyBlackboard.Variables =>
        variables.ToDictionary(kv => kv.Key, kv => (IReadOnlyVariable)kv.Value);

    public void SetVariable(string name, Type type, object value)
    {
        if (variables.ContainsKey(name))
        {
            variables[name].Name = name;
            variables[name].Type = type;
            variables[name].Value = value;
        }
        else
        {
            variables[name] = new Variable { Type = type, Value = value, Name = name };
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

    public void RenameVariable(string oldName, string newName)
    {
        if (!variables.ContainsKey(oldName) || variables.ContainsKey(newName))
            throw new ArgumentException("Invalid variable names for renaming.");

        var variable = variables[oldName];
        variables.Remove(oldName);
        variable.Name = newName;
        variables[newName] = variable;
    }
}

public class Variable : IReadOnlyVariable
{
    public string Name { get; set; }
    public Type Type { get; set; }
    public object Value { get; set; }
}

public interface IReadOnlyVariable
{
    public string Name { get; }
    public Type Type { get; }
    public object Value { get; }
}
