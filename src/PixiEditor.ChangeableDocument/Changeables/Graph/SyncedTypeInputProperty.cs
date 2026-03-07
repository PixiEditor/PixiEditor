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
    public IReadOnlyDictionary<Type, Func<Type, InputProperty>> Handlers => handlers;

    private Dictionary<Type, Func<Type, InputProperty>> handlers = new();
    private Func<Type, InputProperty>? genericFallbackHandler = null;
    private string internalPropertyName { get; }

    public event Action<SyncedTypeInputProperty> ConnectionChanged;
    public event Action BeforeTypeChange;
    public event Action AfterTypeChange;

    public Func<Type, Type>? TypeAdjuster
    {
        get;
        private set;
    }

    private object? pendingValue = null;

    private bool matchToBaseType = false;

    private bool shouldWaitForConnectionToSetType = false;
    private bool isListeningToConnectionChanges = false;

    private Type defaultTypeNonNull;

    public SyncedTypeInputProperty(Node node, string internalPropertyName, string displayName,
        SyncedTypeInputProperty other, Type? defaultType = null)
    {
        Other = other;
        this.internalPropertyName = internalPropertyName;
        defaultTypeNonNull = defaultType ?? typeof(object);
        object? defaultValue = defaultTypeNonNull.IsValueType ? Activator.CreateInstance(defaultTypeNonNull) : null;
        handlers[defaultTypeNonNull] =
            t => new InputProperty(node, internalPropertyName, displayName, defaultValue, defaultTypeNonNull);
        internalInputProperty = handlers[defaultTypeNonNull](defaultTypeNonNull);
        internalInputProperty.NonOverridenValueChanged += NonOverridenChanged;
        node.OnSerializeAdditionalData += OnSerializeAdditionalData;
        node.OnDeserializeAdditionalData += OnDeserializeAdditionalData;
    }

    private void OnSerializeAdditionalData(Dictionary<string, object> data)
    {
        bool isUsingTypeOfThisConnection = internalInputProperty.Connection != null &&
                                           internalInputProperty.Connection.ValueType ==
                                           internalInputProperty.ValueType;
        data[internalPropertyName + "_isUsingTypeOfThisConnection"] = isUsingTypeOfThisConnection;
    }

    private void OnDeserializeAdditionalData(IReadOnlyDictionary<string, object> data, List<IChangeInfo> changeInfos)
    {
        if (data.TryGetValue(internalPropertyName + "_isUsingTypeOfThisConnection", out object value))
        {
            if (value is false)
            {
                shouldWaitForConnectionToSetType = true;
            }
        }
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
        if (isListeningToConnectionChanges) return;

        internalInputProperty.ConnectionChanged += UpdateType;
        internalInputProperty.ConnectionChanged += InvokeConnectionChanged;
        isListeningToConnectionChanges = true;
    }

    public void StopListeningToConnectionChanges()
    {
        internalInputProperty.ConnectionChanged -= UpdateType;
        internalInputProperty.ConnectionChanged -= InvokeConnectionChanged;
        isListeningToConnectionChanges = false;
    }

    private void UpdateType()
    {
        // pending values should be before the first type update
        internalInputProperty.NonOverridenValueChanged -= NonOverridenChanged;
        UpdateTypeInternal(false);
    }

    private void UpdateTypeInternal(bool updatedFromSync)
    {
        if (shouldWaitForConnectionToSetType && !updatedFromSync)
        {
            return;
        }

        IOutputProperty? target = null;

        if (Other == null)
        {
            target = internalInputProperty.Connection;
        }
        else if (shouldWaitForConnectionToSetType && Other.InternalProperty.Connection != null)
        {
            target = Other.InternalProperty.Connection;
        }
        else if (Other.InternalProperty.Connection != null && internalInputProperty.Connection == null)
        {
            target = Other.InternalProperty.Connection;
        }
        else if (Other.IsWaitingForTypeChange && internalInputProperty.Connection != null)
        {
            target = internalInputProperty.Connection;
        }
        else if (Other.InternalProperty.Connection == null && internalInputProperty.Connection != null)
        {
            target = internalInputProperty.Connection;
        }
        else if (Other.InternalProperty.Connection != null && internalInputProperty.Connection != null)
        {
            target = Other.InternalProperty.Connection;
        }

        Type newType = target?.ValueType ?? defaultTypeNonNull;

        if (TypeAdjuster != null)
        {
            newType = TypeAdjuster(newType);
        }

        if (matchToBaseType && newType.IsClass && InternalProperty.Connection != null)
        {
            if (newType != InternalProperty.Connection.ValueType)
            {
                Type currentType = newType;
                while (currentType != null && !InternalProperty.Connection.ValueType.IsAssignableTo(currentType) &&
                       !currentType.IsAssignableTo(typeof(Delegate)))
                {
                    currentType = currentType.BaseType;
                }

                if (currentType != null)
                {
                    newType = currentType;
                }
            }
        }

        var foundHandler = handlers.TryGetValue(newType, out Func<Type, InputProperty> handler) || genericFallbackHandler != null;
        if (!foundHandler)
        {
            var compatibleTypes = handlers.Keys.Where(t => t.IsAssignableFrom(newType)).ToList();
            handler = compatibleTypes.Select(t => handlers[t]).LastOrDefault();
        }

        if (internalInputProperty.ValueType != newType && newType != null && handlers.Count > 0 && foundHandler != null)
        {
            BeforeTypeChange?.Invoke();
            internalInputProperty.ConnectionChanged -= UpdateType;
            internalInputProperty.ConnectionChanged -= InvokeConnectionChanged;
            var connection = internalInputProperty.Connection;
            internalInputProperty.Connection?.DisconnectFrom(internalInputProperty);
            internalInputProperty.Connection = null;

            internalInputProperty = handler != null ? handler(newType) : genericFallbackHandler(newType);
            AfterTypeChange();

            if (pendingValue != null)
            {
                GraphUtils.SetNonOverwrittenValue(internalInputProperty, pendingValue);
            }

            if (internalInputProperty.InternalPropertyName != internalPropertyName)
            {
                throw new InvalidOperationException(
                    $"The handler for type {newType} returned an OutputProperty with an invalid internal name ({internalInputProperty.InternalPropertyName} instead of {internalPropertyName})");
            }

            if (connection != null)
            {
                connection = connection.Node.GetOutputProperty(connection.InternalPropertyName);
                if (connection != null && internalInputProperty.CanConnect(connection))
                {
                    connection.ConnectTo(internalInputProperty);
                }
            }

            internalInputProperty.ConnectionChanged += UpdateType;
            internalInputProperty.ConnectionChanged += InvokeConnectionChanged;
            if (!updatedFromSync || Other.InternalProperty.ValueType != internalInputProperty.ValueType)
                Other?.UpdateTypeInternal(true);
        }

        if (updatedFromSync)
        {
            shouldWaitForConnectionToSetType = false;
        }
    }

    public bool IsWaitingForTypeChange => shouldWaitForConnectionToSetType;

    public SyncedTypeInputProperty AddTypeHandler<T>(Func<Type, InputProperty> handler)
    {
        handlers[typeof(T)] = handler;

        if (Other != null && !Other.Handlers.ContainsKey(typeof(T)))
        {
            throw new InvalidOperationException(
                $"The corresponding SyncedTypeOutputProperty does not have a handler for type {typeof(T)}");
        }

        return this;
    }

    public SyncedTypeInputProperty AddTypeHandler<T>(bool allowInheritedTypes = false)
    {
        return AddTypeHandler<T>(t =>
        {
            Type targetType = typeof(T);
            if (allowInheritedTypes && t.IsAssignableTo(typeof(T)))
            {
                targetType = t;
            }

            var input = new InputProperty(
                internalInputProperty.Node,
                internalPropertyName,
                internalInputProperty.DisplayName,
                targetType.IsValueType ? Activator.CreateInstance(targetType) : null,
                targetType);

            input.AddCustomCanConnect(output => output.ValueType.IsAssignableTo(typeof(T)));

            return input;
        });
    }


    private void InvokeConnectionChanged()
    {
        ConnectionChanged?.Invoke(this);
    }

    public SyncedTypeInputProperty? AllowGenericFallback(bool allowUseCommonAncestorType)
    {
        genericFallbackHandler = delegate(Type type)
        {
            var defaultValue = type.IsValueType ? Activator.CreateInstance(type) : null;
            var prop = new InputProperty(
                internalInputProperty.Node,
                internalPropertyName,
                internalInputProperty.DisplayName,
                defaultValue,
                type);
            if (allowUseCommonAncestorType)
            {
                prop.AddCustomCanConnect(_ => true);
            }

            return prop;
        };

        matchToBaseType = allowUseCommonAncestorType;
        if (matchToBaseType && Other != null)
        {
            UpdateTypeInternal(false);
        }

        return this;
    }

    public SyncedTypeInputProperty? WithTypeAdjuster(Func<Type, Type> func)
    {
        TypeAdjuster = func;
        return this;
    }

    public void ForceUpdateType()
    {
        UpdateTypeInternal(false);
    }
}
