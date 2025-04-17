using System.Diagnostics;
using System.Reflection;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.Common;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Shaders.Generation;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[DebuggerDisplay("Type = {GetType().Name}")]
public abstract class Node : IReadOnlyNode, IDisposable
{
    private string displayName;
    private List<InputProperty> inputs = new();
    private List<OutputProperty> outputs = new();
    protected List<KeyFrameData> keyFrames = new();
    public Guid Id { get; internal set; } = Guid.NewGuid();

    public IReadOnlyList<InputProperty> InputProperties => inputs;
    public IReadOnlyList<OutputProperty> OutputProperties => outputs;
    public IReadOnlyList<KeyFrameData> KeyFrames => keyFrames;
    public event Action ConnectionsChanged;

    IReadOnlyList<IInputProperty> IReadOnlyNode.InputProperties => inputs;
    IReadOnlyList<IOutputProperty> IReadOnlyNode.OutputProperties => outputs;
    IReadOnlyList<IReadOnlyKeyFrameData> IReadOnlyNode.KeyFrames => keyFrames;
    public VecD Position { get; set; }

    public virtual string DisplayName
    {
        get => displayName;
        set => displayName = value;
    }

    protected virtual bool ExecuteOnlyOnCacheChange => false;
    protected virtual CacheTriggerFlags CacheTrigger => CacheTriggerFlags.Inputs;

    private KeyFrameTime lastFrameTime;

    protected internal bool IsDisposed => _isDisposed;
    private bool _isDisposed;

    private int lastContentCacheHash = -1;

    protected virtual int GetContentCacheHash()
    {
        return 0;
    }

    public void Execute(RenderContext context)
    {
        ExecuteInternal(context);
    }

    internal void ExecuteInternal(RenderContext context)
    {
        if (_isDisposed) throw new ObjectDisposedException("Node was disposed before execution.");

        if (!context.FullRerender && ExecuteOnlyOnCacheChange && !CacheChanged(context))
        {
            return;
        }

        OnExecute(context);

        if (ExecuteOnlyOnCacheChange)
        {
            UpdateCache(context);
        }
    }

    protected abstract void OnExecute(RenderContext context);

    protected virtual bool CacheChanged(RenderContext context)
    {
        bool changed = false;

        if (CacheTrigger.HasFlag(CacheTriggerFlags.Inputs))
        {
            changed |= inputs.Any(x => x.CacheChanged);
        }

        if (CacheTrigger.HasFlag(CacheTriggerFlags.Timeline))
        {
            changed |= lastFrameTime.Frame != context.FrameTime.Frame ||
                       Math.Abs(lastFrameTime.NormalizedTime - context.FrameTime.NormalizedTime) > float.Epsilon;
        }

        int contentCacheHash = GetContentCacheHash();

        changed |= contentCacheHash != lastContentCacheHash;

        return changed;
    }

    protected virtual void UpdateCache(RenderContext context)
    {
        foreach (var input in inputs)
        {
            input.UpdateCache();
        }

        lastFrameTime = context.FrameTime;

        lastContentCacheHash = GetContentCacheHash();
    }

    public void TraverseBackwards(Func<IReadOnlyNode, IInputProperty, bool> action)
    {
        var visited = new HashSet<IReadOnlyNode>();
        var queueNodes = new Queue<(IReadOnlyNode, IInputProperty)>();
        queueNodes.Enqueue((this, null));

        while (queueNodes.Count > 0)
        {
            var node = queueNodes.Dequeue();

            if (!visited.Add((node.Item1)))
            {
                continue;
            }

            if (!action(node.Item1, node.Item2))
            {
                return;
            }

            foreach (var inputProperty in node.Item1.InputProperties)
            {
                if (inputProperty.Connection != null)
                {
                    queueNodes.Enqueue((inputProperty.Connection.Node, inputProperty));
                }
            }
        }
    }

    public void TraverseBackwards(Func<IReadOnlyNode, IReadOnlyNode?, IInputProperty, bool> action)
    {
        var visited = new HashSet<IReadOnlyNode>();
        var queueNodes = new Queue<(IReadOnlyNode, IReadOnlyNode, IInputProperty)>();
        queueNodes.Enqueue((this, null, null));

        while (queueNodes.Count > 0)
        {
            var node = queueNodes.Dequeue();

            if (!visited.Add((node.Item1)))
            {
                continue;
            }

            if (!action(node.Item1, node.Item2, node.Item3))
            {
                return;
            }

            foreach (var inputProperty in node.Item1.InputProperties)
            {
                if (inputProperty.Connection != null)
                {
                    queueNodes.Enqueue((inputProperty.Connection.Node, node.Item1, inputProperty));
                }
            }
        }
    }

    public void TraverseBackwards(Func<IReadOnlyNode, bool> action)
    {
        var visited = new HashSet<IReadOnlyNode>();
        var queueNodes = new Queue<IReadOnlyNode>();
        queueNodes.Enqueue(this);

        while (queueNodes.Count > 0)
        {
            var node = queueNodes.Dequeue();

            if (!visited.Add(node))
            {
                continue;
            }

            if (!action(node))
            {
                return;
            }

            foreach (var inputProperty in node.InputProperties)
            {
                if (inputProperty.Connection != null)
                {
                    queueNodes.Enqueue(inputProperty.Connection.Node);
                }
            }
        }
    }

    public void TraverseForwards(Func<IReadOnlyNode, bool> action)
    {
        var visited = new HashSet<IReadOnlyNode>();
        var queueNodes = new Queue<IReadOnlyNode>();
        queueNodes.Enqueue(this);

        while (queueNodes.Count > 0)
        {
            var node = queueNodes.Dequeue();

            if (!visited.Add(node))
            {
                continue;
            }

            if (!action(node))
            {
                return;
            }

            foreach (var outputProperty in node.OutputProperties)
            {
                foreach (var connection in outputProperty.Connections)
                {
                    if (connection.Connection != null)
                    {
                        queueNodes.Enqueue(connection.Node);
                    }
                }
            }
        }
    }

    public void TraverseForwards(Func<IReadOnlyNode, IInputProperty, bool> action)
    {
        var visited = new HashSet<IReadOnlyNode>();
        var queueNodes = new Queue<(IReadOnlyNode, IInputProperty)>();
        queueNodes.Enqueue((this, null));

        while (queueNodes.Count > 0)
        {
            var node = queueNodes.Dequeue();

            if (!visited.Add((node.Item1)))
            {
                continue;
            }

            if (!action(node.Item1, node.Item2))
            {
                return;
            }

            foreach (var outputProperty in node.Item1.OutputProperties)
            {
                foreach (var connection in outputProperty.Connections)
                {
                    if (connection.Connection != null)
                    {
                        queueNodes.Enqueue((connection.Node, connection));
                    }
                }
            }
        }
    }

    public void TraverseForwards(Func<IReadOnlyNode, IInputProperty, IOutputProperty, bool> action)
    {
        var visited = new HashSet<IReadOnlyNode>();
        var queueNodes = new Queue<(IReadOnlyNode, IInputProperty, IOutputProperty)>();
        queueNodes.Enqueue((this, null, null));

        while (queueNodes.Count > 0)
        {
            var node = queueNodes.Dequeue();

            if (!visited.Add((node.Item1)))
            {
                continue;
            }

            if (!action(node.Item1, node.Item2, node.Item3))
            {
                return;
            }

            foreach (var outputProperty in node.Item1.OutputProperties)
            {
                foreach (var connection in outputProperty.Connections)
                {
                    if (connection.Connection != null)
                    {
                        queueNodes.Enqueue((connection.Node, connection, outputProperty));
                    }
                }
            }
        }
    }

    public void RemoveKeyFrame(Guid keyFrameId)
    {
        keyFrames.RemoveAll(x => x.KeyFrameGuid == keyFrameId);
    }

    public void SetKeyFrameLength(Guid id, int startFrame, int duration)
    {
        KeyFrameData frame = keyFrames.FirstOrDefault(x => x.KeyFrameGuid == id);
        if (frame is not null)
        {
            frame.StartFrame = startFrame;
            frame.Duration = duration;
        }
    }

    public void SetKeyFrameVisibility(Guid id, bool isVisible)
    {
        KeyFrameData frame = keyFrames.FirstOrDefault(x => x.KeyFrameGuid == id);
        if (frame is not null)
        {
            frame.IsVisible = isVisible;
        }
    }

    public void AddFrame(Guid id, KeyFrameData value)
    {
        if (keyFrames.Any(x => x.KeyFrameGuid == id))
        {
            throw new InvalidOperationException("Key frame with this id already exists.");
        }

        keyFrames.Add(value);
    }

    protected RenderOutputProperty? CreateRenderOutput(string internalName, string displayName,
        Func<Painter?>? nextInChain, Func<Painter?>? previous = null)
    {
        RenderOutputProperty prop = new RenderOutputProperty(this, internalName, displayName, null);
        prop.FirstInChain = previous;
        prop.NextInChain = nextInChain;
        AddOutputProperty(prop);

        return prop;
    }

    protected RenderInputProperty CreateRenderInput(string internalName, string displayName)
    {
        RenderInputProperty prop = new RenderInputProperty(this, internalName, displayName, null);
        AddInputProperty(prop);

        return prop;
    }


    protected FuncInputProperty<T> CreateFuncInput<T>(string propName, string displayName, T defaultValue)
    {
        var property = new FuncInputProperty<T>(this, propName, displayName, defaultValue);
        if (InputProperties.Any(x => x.InternalPropertyName == propName))
        {
            throw new InvalidOperationException($"Input with name {propName} already exists.");
        }

        property.ConnectionChanged += InvokeConnectionsChanged;
        inputs.Add(property);
        return property;
    }

    protected InputProperty<T> CreateInput<T>(string propName, string displayName, T defaultValue)
    {
        var property = new InputProperty<T>(this, propName, displayName, defaultValue);
        if (InputProperties.Any(x => x.InternalPropertyName == propName))
        {
            throw new InvalidOperationException($"Input with name {propName} already exists.");
        }

        property.ConnectionChanged += InvokeConnectionsChanged;
        inputs.Add(property);
        return property;
    }

    protected FuncOutputProperty<T> CreateFuncOutput<T>(string propName, string displayName,
        Func<FuncContext, T> defaultFunc)
    {
        var property = new FuncOutputProperty<T>(this, propName, displayName, defaultFunc);
        outputs.Add(property);
        return property;
    }

    protected OutputProperty<T> CreateOutput<T>(string propName, string displayName, T defaultValue)
    {
        var property = new OutputProperty<T>(this, propName, displayName, defaultValue);
        outputs.Add(property);
        return property;
    }

    protected void RemoveInputProperty(InputProperty property)
    {
        if (inputs.Remove(property))
        {
            property.ConnectionChanged -= InvokeConnectionsChanged;
        }
    }

    protected void AddOutputProperty(OutputProperty property)
    {
        outputs.Add(property);
    }

    protected void AddInputProperty(InputProperty property)
    {
        if (InputProperties.Any(x => x.InternalPropertyName == property.InternalPropertyName))
        {
            throw new InvalidOperationException($"Input with name {property.InternalPropertyName} already exists.");
        }

        property.ConnectionChanged += InvokeConnectionsChanged;
        inputs.Add(property);
    }

    public virtual void Dispose()
    {
        _isDisposed = true;
        DisconnectAll();
        foreach (var input in inputs)
        {
            if (input is { Connection: null, NonOverridenValue: IDisposable disposable })
            {
                disposable.Dispose();
                input.NonOverridenValue = default;
            }
        }

        foreach (var output in outputs)
        {
            if (output.Connections.Count == 0 && output.Value is IDisposable disposable)
            {
                disposable.Dispose();
                output.Value = default;
            }
        }

        if (keyFrames is not null)
        {
            foreach (var keyFrame in keyFrames)
            {
                keyFrame.Dispose();
            }
        }
    }

    public void DisconnectAll()
    {
        foreach (var input in inputs)
        {
            input.Connection?.DisconnectFrom(input);
        }

        foreach (var output in outputs)
        {
            var connections = output.Connections.ToArray();
            for (var i = 0; i < connections.Length; i++)
            {
                var conn = connections[i];
                output.DisconnectFrom(conn);
            }
        }
    }

    public string GetNodeTypeUniqueName()
    {
        NodeInfoAttribute? attribute = GetType().GetCustomAttribute<NodeInfoAttribute>();
        if (attribute is null)
        {
            throw new InvalidOperationException("Node does not have NodeInfo attribute.");
        }

        return attribute.UniqueName;
    }

    public abstract Node CreateCopy();

    public Node Clone()
    {
        var clone = CreateCopy();

        clone.DisplayName = DisplayName;
        clone.Id = Guid.NewGuid();
        clone.Position = Position;

        for (var i = 0; i < clone.inputs.Count; i++)
        {
            var toClone = inputs[i];
            object value = CloneValue(toClone.NonOverridenValue, clone.inputs[i]);
            clone.inputs[i].NonOverridenValue = value;
        }

        // This makes shader outputs copy old delegate, also I don't think it's required because output is calculated based on inputs,
        // leaving commented in case I'm wrong

        /*for (var i = 0; i < clone.outputs.Count; i++)
        {
            var cloneOutput = outputs[i];
            clone.outputs[i].Value = CloneValue(cloneOutput.Value, null);
        }*/

        foreach (var keyFrame in keyFrames)
        {
            KeyFrameData newKeyFrame = new KeyFrameData(keyFrame.KeyFrameGuid, keyFrame.StartFrame, keyFrame.Duration,
                keyFrame.AffectedElement)
            {
                IsVisible = keyFrame.IsVisible,
                Duration = keyFrame.Duration,
                Data = keyFrame.Data is ICloneable cloneable ? cloneable.Clone() : keyFrame.Data
            };

            clone.AddFrame(newKeyFrame.KeyFrameGuid, newKeyFrame);
        }

        return clone;
    }

    public InputProperty? GetInputProperty(string inputProperty)
    {
        return inputs.FirstOrDefault(x => x.InternalPropertyName == inputProperty);
    }

    public OutputProperty? GetOutputProperty(string outputProperty)
    {
        return outputs.FirstOrDefault(x => x.InternalPropertyName == outputProperty);
    }


    public bool HasInputProperty(string propertyName)
    {
        return inputs.Any(x => x.InternalPropertyName == propertyName);
    }

    public bool HasOutputProperty(string propertyName)
    {
        return outputs.Any(x => x.InternalPropertyName == propertyName);
    }

    IInputProperty? IReadOnlyNode.GetInputProperty(string inputProperty)
    {
        return GetInputProperty(inputProperty);
    }

    IOutputProperty? IReadOnlyNode.GetOutputProperty(string outputProperty)
    {
        return GetOutputProperty(outputProperty);
    }

    public virtual void SerializeAdditionalData(Dictionary<string, object> additionalData)
    {
    }

    internal virtual void DeserializeAdditionalData(IReadOnlyDocument target,
        IReadOnlyDictionary<string, object> data, List<IChangeInfo> infos)
    {
    }

    private void InvokeConnectionsChanged()
    {
        ConnectionsChanged?.Invoke();
    }

    private static object CloneValue(object? value, InputProperty? input)
    {
        if (value is null)
        {
            return null;
        }

        if (input != null && value is Delegate del)
        {
            object constant = del.DynamicInvoke(FuncContext.NoContext);
            if (constant is ShaderExpressionVariable expr)
            {
                return input.FuncFactory(expr.GetConstant());
            }
        }

        if (value is ICloneable cloneable)
        {
            return cloneable.Clone();
        }

        Type type = value.GetType();
        if (type.IsValueType || type == typeof(string))
        {
            return value;
        }

        return default;
    }

    public int GetCacheHash()
    {
        HashCode hash = new();
        hash.Add(GetType());
        hash.Add(DisplayName);
        hash.Add(Position);
        foreach (var input in inputs)
        {
            hash.Add(input.GetCacheHash());
        }

        foreach (var output in outputs)
        {
            hash.Add(output.GetCacheHash());
        }

        foreach (var frame in keyFrames)
        {
            hash.Add(frame.GetCacheHash());
        }

        hash.Add(GetContentCacheHash());

        return hash.ToHashCode();
    }
}
