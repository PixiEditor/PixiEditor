namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface INodeProperty
{
    public string InternalPropertyName { get; }
    public string DisplayName { get; }
    public object Value { get; set; }
    public IReadOnlyNode Node { get; }
    public Type ValueType { get; }
}

public interface INodeProperty<T> : INodeProperty
{
    public new T Value { get; set; }

    Type INodeProperty.ValueType => typeof(T);
}

public interface IInputProperty : INodeProperty
{
    public IOutputProperty? Connection { get; set; }
}

public interface IOutputProperty : INodeProperty
{
    public void ConnectTo(IInputProperty property);
    public void DisconnectFrom(IInputProperty property);
}

public interface IInputProperty<T> : IInputProperty, INodeProperty<T>
{
}

public interface IOutputProperty<T> : IOutputProperty, INodeProperty<T>
{
}
