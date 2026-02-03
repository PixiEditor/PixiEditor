using Drawie.Backend.Core.Shaders.Generation;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class SyncedTypeOutputProperty
{
    private OutputProperty internalOutputProperty;
    public OutputProperty InternalProperty => internalOutputProperty;
    public SyncedTypeInputProperty Other { get; set; }

    public object Value
    {
        get => internalOutputProperty.Value;
        set => internalOutputProperty.Value = value;
    }

    public event Action BeforeTypeChange;
    public event Action AfterTypeChange;

    private string internalPropertyName { get; }
    private Dictionary<Type, Func<OutputProperty>> handlers = new();
    private Func<Type, OutputProperty>? genericFallbackHandler = null;

    public SyncedTypeOutputProperty(Node node, string internalPropertyName, string displayName,
        SyncedTypeInputProperty other)
    {
        Other = other;
        this.internalPropertyName = internalPropertyName;
        handlers[typeof(object)] = () => new OutputProperty(node, internalPropertyName, displayName, null, typeof(object));
        internalOutputProperty = handlers[typeof(object)]();
        Other.AfterTypeChange += UpdateType;
    }

    private void UpdateType()
    {
        if (Other == null)
            return;

        Type newType = Other.InternalProperty?.ValueType ?? typeof(object);
        if (internalOutputProperty.ValueType != newType && newType != null && handlers.Count > 0 &&
            (handlers.TryGetValue(newType, out Func<OutputProperty> handler) || genericFallbackHandler != null))
        {
            BeforeTypeChange?.Invoke();
            var connections = new List<IInputProperty>(internalOutputProperty.Connections);
            for (int i = internalOutputProperty.Connections.Count - 1; i >= 0; i--)
            {
                var connectionNode = internalOutputProperty.Connections.ElementAt(i);
                internalOutputProperty.DisconnectFrom(connectionNode);
            }

            internalOutputProperty = handler != null ? handler() : genericFallbackHandler(newType);
            if(internalOutputProperty.InternalPropertyName != internalPropertyName)
            {
                throw new InvalidOperationException(
                    $"The handler for type {newType} returned an OutputProperty with an invalid internal name ({internalOutputProperty.InternalPropertyName} instead of {internalPropertyName})");
            }

            if (newType.IsAssignableTo(typeof(Delegate)) &&
                handlers.TryGetValue(typeof(ShaderExpressionVariable), out var del))
            {
                internalOutputProperty.Value = del;
            }

            foreach (var input in connections)
            {
                if (GraphUtils.IsLoop(input, internalOutputProperty) ||
                    !GraphUtils.CheckTypeCompatibility(input, internalOutputProperty))
                {
                    continue;
                }

                internalOutputProperty.ConnectTo(input);
            }

            AfterTypeChange();
        }
    }

    public SyncedTypeOutputProperty? AddTypeHandler<T>(Func<OutputProperty> handleOutput)
    {
        if (!Other.Handlers.ContainsKey(typeof(T)))
            throw new InvalidOperationException(
                $"The corresponding SyncedTypeInputProperty does not have a handler for type {typeof(T)}");

        handlers[typeof(T)] = handleOutput;
        return this;
    }

    public SyncedTypeOutputProperty? AllowGenericFallback()
    {
        genericFallbackHandler = type =>
        {
            var defaultValue = type.IsValueType ? Activator.CreateInstance(type) : null;
            return new OutputProperty(
                internalOutputProperty.Node,
                internalPropertyName,
                internalOutputProperty.DisplayName,
                defaultValue,
                type);
        };
        return this;
    }
}
