using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public delegate void InputConnectedEvent(IInputProperty input, IOutputProperty output);

public class OutputProperty : IOutputProperty
{
    private List<IInputProperty> _connections = new();
    private object _value;
    public string InternalPropertyName { get; }
    public string DisplayName { get; }
    public Node Node { get; }

    public Type ValueType { get; }

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

    internal OutputProperty(Node node, string internalName, string displayName, object defaultValue, Type valueType)
    {
        InternalPropertyName = internalName;
        DisplayName = displayName;
        Value = defaultValue;
        Node = node;
        ValueType = valueType;
    }

    public void ConnectTo(IInputProperty property)
    {
        if (Connections.Contains(property)) return;

        if (property.Connection != null)
        {
            property.Connection.DisconnectFrom(property);
        }
        
        _connections.Add(property);
        property.Connection = this;
        Connected?.Invoke(property, this);
    }

    public void DisconnectFrom(IInputProperty property)
    {
        if (!Connections.Contains(property)) return;

        _connections.Remove(property);
        if (property.Connection == this)
        {
            property.Connection = null;
        }

        Disconnected?.Invoke(property, this);
    }

    public OutputProperty Clone(Node clone)
    {
        if (Value is null)
        {
            object defaultValue = null;
            if(ValueType.IsValueType)
                defaultValue = Activator.CreateInstance(ValueType);
            return new OutputProperty(clone, InternalPropertyName, DisplayName, defaultValue, ValueType);
        }

        if (Value is Enum enumVal)
        {
            return new OutputProperty(clone, InternalPropertyName, DisplayName, enumVal, ValueType);
        }

        if (Value is not ICloneable && !Value.GetType().IsPrimitive && Value.GetType() != typeof(string))
            throw new InvalidOperationException("Value is not cloneable and not a primitive type");
     
        object value = Value is ICloneable cloneableValue ? cloneableValue.Clone() : Value;
        
        var newOutput = new OutputProperty(clone, InternalPropertyName, DisplayName, value, ValueType);

        return newOutput;
    }
}

public class OutputProperty<T> : OutputProperty, INodeProperty<T>
{
    public new T Value
    {
        get => (T)base.Value;
        set => base.Value = value;
    }

    internal OutputProperty(Node node, string internalName, string displayName, T defaultValue) : base(node, internalName, displayName, defaultValue, typeof(T))
    {
    }
}
