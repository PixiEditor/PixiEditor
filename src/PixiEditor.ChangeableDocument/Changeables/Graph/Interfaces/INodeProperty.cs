using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.Common;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface INodeProperty : ICacheable
{
    public string InternalPropertyName { get; }
    public string DisplayName { get; }
    public object Value { get; }
    public IReadOnlyNode Node { get; }
    public Type ValueType { get; }
}

public interface INodeProperty<T> : INodeProperty
{
    public new T Value { get; }

    Type INodeProperty.ValueType => typeof(T);
}

public interface IInputProperty : INodeProperty
{
    public IOutputProperty? GetVirtualConnection(Guid virtualConnectionId);
    public void SetVirtualConnection(IOutputProperty outputProperty, Guid virtualConnectionId, RenderContext context);
    public IOutputProperty? Connection { get; set; }
    public object NonOverridenValue { get; set;  }
    public void RemoveVirtualConnection(Guid virtualConnectionId);
    public void SetVirtualNonOverridenValue<T>(T value, Guid virtualSession, RenderContext context);
}

public interface IOutputProperty : INodeProperty
{
    public void VirtualConnectTo(IInputProperty property, Guid virtualConnectionId, RenderContext context);
    public void ConnectTo(IInputProperty property);
    public void DisconnectFrom(IInputProperty property);
    public void DisconnectFromVirtual(IInputProperty property, Guid virtualConnectionId);
    IReadOnlyCollection<IInputProperty> Connections { get; }
}

public interface IInputProperty<T> : IInputProperty, INodeProperty<T>
{
    public new T NonOverridenValue { get; set; }
}

public interface IOutputProperty<T> : IOutputProperty, INodeProperty<T>
{
}
