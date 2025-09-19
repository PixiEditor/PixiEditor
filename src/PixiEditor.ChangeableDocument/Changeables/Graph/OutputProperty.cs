using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.Common;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public delegate void InputConnectedEvent(IInputProperty input, IOutputProperty output);

public class OutputProperty : IOutputProperty
{
    private Dictionary<Guid, List<IInputProperty>> _virtualConnections = new();
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
        }
    }

    public IReadOnlyCollection<IInputProperty> Connections => _connections;
    public IReadOnlyCollection<IInputProperty> GetVirtualConnections(Guid virtualSession) => _virtualConnections[virtualSession];

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

    public void VirtualConnectTo(IInputProperty property, Guid virtualConnectionId, RenderContext context)
    {
        if (!_virtualConnections.ContainsKey(virtualConnectionId))
        {
            _virtualConnections[virtualConnectionId] = new List<IInputProperty>();
        }

        if (property.GetVirtualConnection(virtualConnectionId) != null)
        {
            property.GetVirtualConnection(virtualConnectionId).DisconnectFromVirtual(property, virtualConnectionId);
        }

        property.SetVirtualConnection(this, virtualConnectionId, context);

        if (_virtualConnections[virtualConnectionId].Contains(property)) return;

        _virtualConnections[virtualConnectionId].Add(property);
        context.RecordVirtualConnection(this, virtualConnectionId);
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


    public void DisconnectFromVirtual(IInputProperty property, Guid virtualConnectionId)
    {
        if (!_virtualConnections.TryGetValue(virtualConnectionId, out var connection)) return;
        if (connection != property) return;

        _virtualConnections.Remove(virtualConnectionId);
        if (property.GetVirtualConnection(virtualConnectionId) == this)
        {
            property.RemoveVirtualConnection(virtualConnectionId);
        }
    }

    public int GetCacheHash()
    {
        if (Value is ICacheable cacheable)
        {
            return cacheable.GetCacheHash();
        }

        return 0;
    }

    public void RemoveAllVirtualConnections(Guid virtualSessionId)
    {
        _virtualConnections.Remove(virtualSessionId);
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
