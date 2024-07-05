using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class InputProperty : IInputProperty
{
    public string Name { get; }
    public object Value { get; set; }
    public Node Node { get; }
    public Type ValueType { get; } 
    IReadOnlyNode INodeProperty.Node => Node;
    
    public IOutputProperty? Connection { get; set; }
    
    internal InputProperty(Node node, string name, object defaultValue, Type valueType)
    {
        Name = name;
        Value = defaultValue;
        Node = node;
        ValueType = valueType;
    }

    public InputProperty Clone(Node forNode)
    {
        if(Value is ICloneable cloneable)
            return new InputProperty(forNode, Name, cloneable.Clone(), ValueType);
        
        if(!Value.GetType().IsPrimitive && Value.GetType() != typeof(string))
            throw new InvalidOperationException("Value is not cloneable and not a primitive type");
        
        return new InputProperty(forNode, Name, Value, ValueType);
    }
}


public class InputProperty<T> : InputProperty, IInputProperty<T>
{
    public new T Value
    {
        get => (T)base.Value;
        set => base.Value = value;
    }
    
    internal InputProperty(Node node, string name, T defaultValue) : base(node, name, defaultValue, typeof(T))
    {
    }
}
