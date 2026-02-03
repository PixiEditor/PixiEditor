using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Shaders.Generation;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class SyncedTypeInputProperty
{
    private InputProperty internalInputProperty;
    public InputProperty InternalProperty => internalInputProperty;
    public SyncedTypeInputProperty Other { get; set; }
    public object Value => internalInputProperty.Value;
    public IReadOnlyDictionary<Type, Func<InputProperty>> Handlers => handlers;

    private Dictionary<Type, Func<InputProperty>> handlers = new();
    private Func<Type, InputProperty>? genericFallbackHandler = null;
    private string internalPropertyName { get; }

    public event Action ConnectionChanged;
    public event Action BeforeTypeChange;
    public event Action AfterTypeChange;

    private object? pendingValue = null;

    private static HashSet<Type> TypesToAlwaysUseForInherited = new()
    {
        typeof(ShapeVectorData)
    };

    public SyncedTypeInputProperty(Node node, string internalPropertyName, string displayName,
        SyncedTypeInputProperty other)
    {
        Other = other;
        this.internalPropertyName = internalPropertyName;
        handlers[typeof(object)] = () => new InputProperty(node, internalPropertyName, displayName, null, typeof(object));
        internalInputProperty = handlers[typeof(object)]();
        internalInputProperty.NonOverridenValueChanged += NonOverridenChanged;
    }

    private void NonOverridenChanged(object obj)
    {
        if (internalInputProperty.ValueType != typeof(object))
        {
            internalInputProperty.NonOverridenValueChanged -= NonOverridenChanged;
            return;
        }

        pendingValue = obj;
        internalInputProperty.NonOverridenValueChanged -= NonOverridenChanged;
    }

    internal void BeginListeningToConnectionChanges()
    {
        internalInputProperty.ConnectionChanged += UpdateType;
        internalInputProperty.ConnectionChanged += InvokeConnectionChanged;
    }

    private void UpdateType()
    {
        // pending values should be before the first type update
        internalInputProperty.NonOverridenValueChanged -= NonOverridenChanged;
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

        if(newType.IsClass && target != null)
        {
            foreach(var type in TypesToAlwaysUseForInherited)
            {
                if(newType.IsAssignableTo(type) && (handlers.ContainsKey(type) || genericFallbackHandler != null))
                {
                    newType = type;
                    break;
                }
            }
        }

        if (internalInputProperty.ValueType != newType && newType != null && handlers.Count > 0 &&
            (handlers.TryGetValue(newType, out Func<InputProperty> handler) || genericFallbackHandler != null))
        {
            BeforeTypeChange?.Invoke();
            internalInputProperty.ConnectionChanged -= UpdateType;
            internalInputProperty.ConnectionChanged -= InvokeConnectionChanged;
            var connection = internalInputProperty.Connection;
            internalInputProperty.Connection?.DisconnectFrom(internalInputProperty);
            internalInputProperty.Connection = null;

            internalInputProperty = handler != null ? handler() : genericFallbackHandler(newType);

            if (pendingValue != null)
            {
                GraphUtils.SetNonOverwrittenValue(internalInputProperty, pendingValue);
            }

            if (internalInputProperty.InternalPropertyName != internalPropertyName)
            {
                throw new InvalidOperationException(
                    $"The handler for type {newType} returned an OutputProperty with an invalid internal name ({internalInputProperty.InternalPropertyName} instead of {internalPropertyName})");
            }

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

    public SyncedTypeInputProperty? AllowGenericFallback()
    {
        genericFallbackHandler = delegate(Type type)
        {
            var defaultValue = type.IsValueType ? Activator.CreateInstance(type) : null;
            return new InputProperty(
                internalInputProperty.Node,
                internalPropertyName,
                internalInputProperty.DisplayName,
                defaultValue,
                type);
        };
        
        return this;
    }
}
