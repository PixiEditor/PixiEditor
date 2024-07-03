using Nodes.Nodes;

namespace Nodes;

public class InputProperty : IInputProperty
{
    public string Name { get; }
    public object Value { get; set; }
    public Node Node { get; }
    IReadOnlyNode INodeProperty.Node => Node;
    
    public IOutputProperty Connection { get; set; }
    
    internal InputProperty(Node node, string name, object defaultValue)
    {
        Name = name;
        Value = defaultValue;
        Node = node;
    }

}


public class InputProperty<T> : InputProperty, IInputProperty<T>
{
    public new T Value
    {
        get => (T)base.Value;
        set => base.Value = value;
    }
    
    internal InputProperty(Node node, string name, T defaultValue) : base(node, name, defaultValue)
    {
    }
}
