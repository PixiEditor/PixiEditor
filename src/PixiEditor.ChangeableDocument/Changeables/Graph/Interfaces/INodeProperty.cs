using Drawie.Backend.Core;
using PixiEditor.GraphNavigation;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

using INavigableChangeableNodeReference = INavigableNodeReference<IReadOnlyNode, IInputProperty, IOutputProperty>;
using INavigableChangeableInputProperty = INavigableInputProperty<IReadOnlyNode, IInputProperty, IOutputProperty>;
using INavigableChangeableOutputProperty = INavigableOutputProperty<IReadOnlyNode, IInputProperty, IOutputProperty>;

public interface INodeProperty : INavigableChangeableNodeReference, ICacheable
{
    public string InternalPropertyName { get; }
    public string DisplayName { get; }
    public object Value { get; }
    public IReadOnlyNode Node { get; }
    public Type ValueType { get; }

    IReadOnlyNode INavigableChangeableNodeReference.Node => Node;
}

public interface INodeProperty<T> : INodeProperty
{
    public new T Value { get; }

    Type INodeProperty.ValueType => typeof(T);
}

public interface IInputProperty : INavigableChangeableInputProperty, INodeProperty
{
    public IOutputProperty? Connection { get; set; }
    public object NonOverridenValue { get; set;  }
    public bool CanConnect(IOutputProperty internalOutputProperty);

    IOutputProperty INavigableChangeableInputProperty.ConnectedOutput => Connection;
}

public interface IOutputProperty : INavigableChangeableOutputProperty, INodeProperty
{
    public void ConnectTo(IInputProperty property);
    public void DisconnectFrom(IInputProperty property);
    IReadOnlyCollection<IInputProperty> Connections { get; }

    IEnumerable<IInputProperty> INavigableChangeableOutputProperty.ConnectedInputs =>
        Connections;
}

public interface IInputProperty<T> : IInputProperty, INodeProperty<T>
{
    public new T NonOverridenValue { get; set; }
}

public interface IOutputProperty<T> : IOutputProperty, INodeProperty<T>
{
}
