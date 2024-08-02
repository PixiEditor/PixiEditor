using System.Diagnostics;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.Surface.ImageData;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[DebuggerDisplay("Type = {GetType().Name}")]
public abstract class Node : IReadOnlyNode, IDisposable
{
    private List<InputProperty> inputs = new();
    private List<OutputProperty> outputs = new();
    protected List<KeyFrameData> keyFrames = new();

    public Guid Id { get; internal set; } = Guid.NewGuid();

    public IReadOnlyCollection<InputProperty> InputProperties => inputs;
    public IReadOnlyCollection<OutputProperty> OutputProperties => outputs;

    public Surface? CachedResult
    {
        get
        {
            if(_lastCachedResult == null || _lastCachedResult.IsDisposed) return null;
            return _lastCachedResult;
        }
        private set
        {
            _lastCachedResult = value;
        }
    }

    public virtual string InternalName => $"PixiEditor.{NodeUniqueName}";
    
    protected abstract string NodeUniqueName { get; }

    protected virtual bool AffectedByAnimation { get; }

    protected virtual bool AffectedByChunkResolution { get; }

    protected virtual bool AffectedByChunkToUpdate { get; }

    protected Node()
    {
    }

    IReadOnlyCollection<IInputProperty> IReadOnlyNode.InputProperties => inputs;
    IReadOnlyCollection<IOutputProperty> IReadOnlyNode.OutputProperties => outputs;
    public VecD Position { get; set; }
    public abstract string DisplayName { get; set; }

    private KeyFrameTime _lastFrameTime = new KeyFrameTime(-1, 0);
    private ChunkResolution? _lastResolution;
    private VecI? _lastChunkPos;
    private bool _keyFramesDirty;
    private Surface? _lastCachedResult;

    public Surface? Execute(RenderingContext context)
    {
        var result = ExecuteInternal(context);

        var copy = new Surface(result);
        return copy;
    }

    internal Surface ExecuteInternal(RenderingContext context)
    {
        if (!CacheChanged(context)) return CachedResult;

        CachedResult = OnExecute(context);
        if (CachedResult is { IsDisposed: true })
        {
            throw new ObjectDisposedException("Surface was disposed after execution.");
        }

        UpdateCache(context);
        return CachedResult;
    }

    protected abstract Surface? OnExecute(RenderingContext context);

    protected virtual bool CacheChanged(RenderingContext context)
    {
        return (!context.FrameTime.Equals(_lastFrameTime) && AffectedByAnimation)
               || (AffectedByAnimation && _keyFramesDirty)
               || (context.ChunkResolution != _lastResolution && AffectedByChunkResolution)
               || (context.ChunkToUpdate != _lastChunkPos && AffectedByChunkToUpdate)
               || inputs.Any(x => x.CacheChanged);
    }

    protected virtual void UpdateCache(RenderingContext context)
    {
        foreach (var input in inputs)
        {
            input.UpdateCache();
        }

        _lastFrameTime = context.FrameTime;
        _lastResolution = context.ChunkResolution;
        _lastChunkPos = context.ChunkToUpdate;
        _keyFramesDirty = false;
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
                    queueNodes.Enqueue(inputProperty.Node);
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

    public void RemoveKeyFrame(Guid keyFrameId)
    {
        keyFrames.RemoveAll(x => x.KeyFrameGuid == keyFrameId);
        _keyFramesDirty = true;
    }

    public void SetKeyFrameLength(Guid id, int startFrame, int duration)
    {
        KeyFrameData frame = keyFrames.FirstOrDefault(x => x.KeyFrameGuid == id);
        if (frame is not null)
        {
            frame.StartFrame = startFrame;
            frame.Duration = duration;
            _keyFramesDirty = true;
        }
    }

    public void AddFrame<T>(Guid id, T value) where T : KeyFrameData
    {
        if (keyFrames.Any(x => x.KeyFrameGuid == id))
        {
            throw new InvalidOperationException("Key frame with this id already exists.");
        }
        
        keyFrames.Add(value);
        _keyFramesDirty = true;
    }

    protected FuncInputProperty<T> CreateFuncInput<T>(string propName, string displayName, T defaultValue)
    {
        var property = new FuncInputProperty<T>(this, propName, displayName, defaultValue);
        if (InputProperties.Any(x => x.InternalPropertyName == propName))
        {
            throw new InvalidOperationException($"Input with name {propName} already exists.");
        }

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

        inputs.Add(property);
        return property;
    }

    protected FuncOutputProperty<T> CreateFieldOutput<T>(string propName, string displayName,
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

    public virtual void Dispose()
    {
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
        
        if(keyFrames is not null)
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

    public abstract Node CreateCopy();

    public Node Clone()
    {
        var clone = CreateCopy();
        clone.Id = Guid.NewGuid();
        clone.Position = Position;

        for (var i = 0; i < clone.inputs.Count; i++)
        {
            var input = inputs[i];
            var newInput = input.Clone(clone);
            input.NonOverridenValue = newInput.NonOverridenValue;
        }

        for (var i = 0; i < clone.outputs.Count; i++)
        {
            var output = outputs[i];
            var newOutput = output.Clone(clone);
            output.Value = newOutput.Value;
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
    
    IInputProperty? IReadOnlyNode.GetInputProperty(string inputProperty)
    {
        return GetInputProperty(inputProperty);
    }
    
    IOutputProperty? IReadOnlyNode.GetOutputProperty(string outputProperty)
    {
        return GetOutputProperty(outputProperty);
    }
}
