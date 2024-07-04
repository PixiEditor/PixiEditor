namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public abstract class Node(string name) : IReadOnlyNode
{
    private List<InputProperty> inputs = new();
    private List<OutputProperty> outputs = new();
    
    private List<IReadOnlyNode> _connectedNodes = new();
    
    public string Name { get; } = name;
    
    public IReadOnlyCollection<InputProperty> InputProperties => inputs;
    public IReadOnlyCollection<OutputProperty> OutputProperties => outputs;
    public IReadOnlyCollection<IReadOnlyNode> ConnectedNodes => _connectedNodes;

    IReadOnlyCollection<IInputProperty> IReadOnlyNode.InputProperties => inputs;
    IReadOnlyCollection<IOutputProperty> IReadOnlyNode.OutputProperties => outputs;

    public void Execute(int frame)
    {
        foreach (var output in outputs)
        {
            foreach (var connection in output.Connections)
            {
                connection.Value = output.Value;
            }
        }
        
        OnExecute(frame);
    }

    public abstract void OnExecute(int frame);
    public abstract bool Validate();
    
    protected InputProperty<T> CreateInput<T>(string name, T defaultValue)
    {
        var property = new InputProperty<T>(this, name, defaultValue);
        inputs.Add(property);
        return property;
    }
    
    protected OutputProperty<T> CreateOutput<T>(string name, T defaultValue)
    {
        var property = new OutputProperty<T>(this, name, defaultValue);
        outputs.Add(property);
        property.Connected += (input, _) => _connectedNodes.Add(input.Node);
        property.Disconnected += (input, _) => _connectedNodes.Remove(input.Node);
        return property;
    }
}
