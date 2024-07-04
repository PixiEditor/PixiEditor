using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public delegate void InputConnectedEvent(IInputProperty input, IOutputProperty output);
public class OutputProperty : IOutputProperty
{
    private List<IInputProperty> _connections = new();
    private object _value;
    public string Name { get; }
    
    public Node Node { get; }
    IReadOnlyNode INodeProperty.Node => Node;

    public object Value
    {
        get => _value;
        set
        {
            _value = value;
            foreach (var connection in Connections)
            {
                connection.Value = value;
            }
        }    
    }

    public IReadOnlyCollection<IInputProperty> Connections => _connections;
    
    public event InputConnectedEvent Connected;
    public event InputConnectedEvent Disconnected;
    
    internal OutputProperty(Node node, string name, object defaultValue)
    {
        Name = name;
        Value = defaultValue;
        _connections = new List<IInputProperty>();
        Node = node;
    }
    
    public void ConnectTo(IInputProperty property)
    {
        if(Connections.Contains(property)) return;
        
        _connections.Add(property);
        property.Connection = this;
        Connected?.Invoke(property, this);
    }
    
    public void DisconnectFrom(IInputProperty property)
    {
        if(!Connections.Contains(property)) return;
        
        _connections.Remove(property);
        if(property.Connection == this)
        {
            property.Connection = null;
        }
        
        Disconnected?.Invoke(property, this);
    }
}

public class OutputProperty<T> : OutputProperty, INodeProperty<T>
{
    public new T Value
    {
        get => (T)base.Value;
        set => base.Value = value;
    }
    
    internal OutputProperty(Node node ,string name, T defaultValue) : base(node, name, defaultValue)
    {
    }
}
