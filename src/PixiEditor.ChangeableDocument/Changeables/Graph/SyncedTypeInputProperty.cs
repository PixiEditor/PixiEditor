using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Shaders.Generation;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class SyncedTypeInputProperty
{
    private InputProperty internalInputProperty;
    public InputProperty InternalProperty => internalInputProperty;
    public SyncedTypeInputProperty Other { get; set; }
    public object Value => internalInputProperty.Value;
    public IReadOnlyDictionary<Type, Func<InputProperty>> Handlers => handlers;

    private Dictionary<Type, Func<InputProperty>> handlers = new();

    public event Action ConnectionChanged;
    public event Action BeforeTypeChange;
    public event Action AfterTypeChange;

    public SyncedTypeInputProperty(Node node, string internalPropertyName, string displayName,
        SyncedTypeInputProperty other)
    {
        Other = other;
        internalInputProperty = new InputProperty(node, internalPropertyName, displayName, null, typeof(object));
    }

    internal void BeginListeningToConnectionChanges()
    {
        internalInputProperty.ConnectionChanged += UpdateType;
        internalInputProperty.ConnectionChanged += InvokeConnectionChanged;
    }

    private void UpdateType()
    {
        UpdateTypeInternal(false);
    }

    private void UpdateTypeInternal(bool updatedFromSync)
    {
        IOutputProperty? target = null;
        if (Other.InternalProperty.Connection != null && internalInputProperty.Connection == null)
        {
            target = Other.InternalProperty.Connection;
        }
        else if (Other.InternalProperty.Connection == null && internalInputProperty.Connection != null)
        {
            target = internalInputProperty.Connection;
        }
        else if (Other.InternalProperty.Connection != null && internalInputProperty.Connection != null)
        {
            target = Other.InternalProperty.Connection;
        }

        Type newType = target?.ValueType ?? typeof(object);
        if (internalInputProperty.ValueType != newType && newType != null && handlers.Count > 0 &&
            handlers.TryGetValue(newType, out Func<InputProperty> handler))
        {
            BeforeTypeChange?.Invoke();
            internalInputProperty.ConnectionChanged -= UpdateType;
            internalInputProperty.ConnectionChanged -= InvokeConnectionChanged;
            var connection = internalInputProperty.Connection;
            internalInputProperty.Connection?.DisconnectFrom(internalInputProperty);
            internalInputProperty.Connection = null;

            internalInputProperty = handler();

            if (connection != null && GraphUtils.CheckTypeCompatibility(internalInputProperty, connection))
            {
                connection.ConnectTo(internalInputProperty);
            }

            internalInputProperty.ConnectionChanged += UpdateType;
            internalInputProperty.ConnectionChanged += InvokeConnectionChanged;
            AfterTypeChange();
            if (!updatedFromSync)
                Other?.UpdateTypeInternal(true);
        }
    }

    public SyncedTypeInputProperty AddTypeHandler<T>(Func<InputProperty> handler)
    {
        handlers[typeof(T)] = handler;

        if (Other != null && !Other.Handlers.ContainsKey(typeof(T)))
        {
            throw new InvalidOperationException(
                $"The corresponding SyncedTypeOutputProperty does not have a handler for type {typeof(T)}");
        }

        return this;
    }


    private void InvokeConnectionChanged()
    {
        ConnectionChanged?.Invoke();
    }
}
