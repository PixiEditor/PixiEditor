using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;
using PixiEditor.Common;
using Drawie.Backend.Core.Shaders.Generation;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class InputProperty : IInputProperty
{
    private object _internalValue;
    private int _lastExecuteHash = -1;
    protected int lastConnectionHash = -1;
    private PropertyValidator? validator;
    private IOutputProperty? connection;

    public event Action ConnectionChanged;
    public event Action<object> NonOverridenValueChanged;

    public string InternalPropertyName { get; }
    public string DisplayName { get; }

    public object? Value
    {
        get
        {
            if (Connection == null)
            {
                return _internalValue;
            }

            var connectionValue = Connection.Value;

            if (connectionValue is null)
            {
                return null;
            }

            if (!ValueType.IsAssignableTo(typeof(Delegate)) && connectionValue is Delegate connectionField)
            {
                return connectionField.DynamicInvoke(FuncContext.NoContext);
            }

            if (ValueType.IsAssignableTo(typeof(Delegate)) && connectionValue is not Delegate)
            {
                return FuncFactory(connectionValue);
            }

            if (connectionValue.GetType().IsAssignableTo(ValueType))
            {
                return connectionValue;
            }

            if (connectionValue is Delegate func && ValueType.IsAssignableTo(typeof(Delegate)))
            {
                return FuncFactoryDelegate(func);
            }

            object target = connectionValue;
            if (target is ShaderExpressionVariable shaderExpression)
            {
                target = shaderExpression.GetConstant();
            }

            if (!ConversionTable.TryConvert(target, ValueType, out object result))
            {
                return null;
            }

            return Validator.GetClosestValidValue(result);
        }
    }

    public object NonOverridenValue
    {
        get => _internalValue;
        set
        {
            object evaluatedValue = value;
            if (value != null)
            {
                if (!value.GetType().IsAssignableTo(ValueType))
                {
                    if (!ConversionTable.TryConvert(value, ValueType, out object result))
                    {
                        evaluatedValue = null;
                    }
                    else
                    {
                        evaluatedValue = result;
                    }
                }
            }

            _internalValue = evaluatedValue;
            NonOverridenValueChanged?.Invoke(evaluatedValue);
            NonOverridenValueSet(evaluatedValue);
        }
    }

    public PropertyValidator Validator
    {
        get
        {
            if (validator is null)
            {
                validator = new PropertyValidator(this);
            }

            return validator;
        }
    }

    protected internal virtual object FuncFactory(object toReturn)
    {
        Func<FuncContext, object> func = _ => toReturn;
        return func;
    }

    protected virtual object FuncFactoryDelegate(Delegate delegateToCast)
    {
        Func<FuncContext, object> func = f =>
        {
            return ConversionTable.TryConvert(delegateToCast.DynamicInvoke(f), ValueType, out object result)
                ? result
                : null;
        };
        return func;
    }

    public Node Node { get; }
    public Type ValueType { get; }

    internal virtual bool CacheChanged
    {
        get
        {
            if (Connection == null && lastConnectionHash != -1)
            {
                return true;
            }

            if (Connection != null && lastConnectionHash != Connection.GetHashCode())
            {
                lastConnectionHash = Connection.GetHashCode();
                return true;
            }

            if (Value is ICacheable cacheable)
            {
                return cacheable.GetCacheHash() != _lastExecuteHash;
            }

            if (Value is null)
            {
                return _lastExecuteHash != 0;
            }

            if (Value.GetType().IsValueType || Value.GetType() == typeof(string))
            {
                return Value.GetHashCode() != _lastExecuteHash;
            }

            return true;
        }
    }

    protected virtual void NonOverridenValueSet(object value)
    {
    }

    internal virtual void UpdateCache()
    {
        if (Value is null)
        {
            _lastExecuteHash = 0;
        }
        else if (Value is ICacheable cacheable)
        {
            _lastExecuteHash = cacheable.GetCacheHash();
        }
        else
        {
            _lastExecuteHash = Value.GetHashCode();
        }

        lastConnectionHash = Connection?.GetHashCode() ?? -1;
    }

    IReadOnlyNode INodeProperty.Node => Node;

    public IOutputProperty? Connection
    {
        get => connection;
        set
        {
            if (connection != value)
            {
                connection = value;
                ConnectionChanged?.Invoke();
            }
        }
    }

    internal InputProperty(Node node, string internalName, string displayName, object defaultValue, Type valueType)
    {
        InternalPropertyName = internalName;
        DisplayName = displayName;
        _internalValue = defaultValue;
        Node = node;
        ValueType = valueType;
    }

    public int GetCacheHash()
    {
        HashCode hash = new();
        hash.Add(InternalPropertyName);
        hash.Add(ValueType);
        if(Value is ICacheable cacheable)
        {
            hash.Add(cacheable.GetCacheHash());
        }
        else if (Value is Delegate func && Connection == null)
        {
            try
            {
                var constant = func.DynamicInvoke(FuncContext.NoContext);
                if (constant is ShaderExpressionVariable shaderExpression)
                {
                    hash.Add(shaderExpression.GetConstant());
                }
            }
            catch { }
        }
        else
        {
            hash.Add(Value?.GetHashCode() ?? 0);
        }

        hash.Add(Connection?.GetCacheHash() ?? 0);
        return hash.ToHashCode();
    }
}

public class InputProperty<T> : InputProperty, IInputProperty<T>
{
    public new T Value
    {
        get
        {
            object value = base.Value;
            if (value is null) return default(T);

            if (value is T tValue)
                return tValue;

            if (value is ShaderExpressionVariable shaderExpression)
            {
                value = shaderExpression.GetConstant();
            }

            if (!ConversionTable.TryConvert(value, ValueType, out object result))
            {
                result = default(T);
            }

            result = Validator.GetClosestValidValue(result);

            return (T)result;
        }
    }

    public T NonOverridenValue
    {
        get => (T)(base.NonOverridenValue ?? default(T));
        set
        {
            base.NonOverridenValue = value;
        }
    }

    protected override void NonOverridenValueSet(object value)
    {
        if (value is T casted)
        {
            NonOverridenValueSet(casted);
        }
    }

    protected virtual void NonOverridenValueSet(T value)
    {
    }

    internal InputProperty(Node node, string internalName, string displayName, T defaultValue) : base(node,
        internalName, displayName, defaultValue, typeof(T))
    {
    }

    public InputProperty<T> WithRules(Action<PropertyValidator> rules)
    {
        rules(Validator);
        return this;
    }

    public InputProperty<T> NonOverridenChanged(Action<T> callback)
    {
        NonOverridenValueChanged += value => callback((T)value);
        return this;
    }
}
