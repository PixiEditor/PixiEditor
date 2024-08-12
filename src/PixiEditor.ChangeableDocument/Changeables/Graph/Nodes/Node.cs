using System.Diagnostics;
using System.Reflection;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Shaders;
using PixiEditor.Numerics;

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

    public Texture? CachedResult
    {
        get
        {
            if (_lastCachedResult == null || _lastCachedResult.IsDisposed) return null;
            return _lastCachedResult;
        }
        private set
        {
            _lastCachedResult = value;
        }
    }

    protected virtual bool AffectedByAnimation { get; }

    protected virtual bool AffectedByChunkResolution { get; }

    protected virtual bool AffectedByChunkToUpdate { get; }

    protected Node()
    {
        displayName = GetType().GetCustomAttribute<NodeInfoAttribute>().DisplayName;
    }

    IReadOnlyList<IInputProperty> IReadOnlyNode.InputProperties => inputs;
    IReadOnlyList<IOutputProperty> IReadOnlyNode.OutputProperties => outputs;
    IReadOnlyList<IReadOnlyKeyFrameData> IReadOnlyNode.KeyFrames => keyFrames;
    public VecD Position { get; set; }

    public virtual string DisplayName
    {
        get => displayName;
        set => displayName = value;
    }

    private KeyFrameTime _lastFrameTime = new KeyFrameTime(-1, 0);
    private ChunkResolution? _lastResolution;
    private VecI? _lastChunkPos;
    private bool _keyFramesDirty;
    private Texture? _lastCachedResult;
    private bool _isDisposed;
    
    private Dictionary<int, Texture> _managedTextures = new();

    public Texture? Execute(RenderingContext context)
    {
        var result = ExecuteInternal(context);

        var copy = new Texture(result);
        return copy;
    }

    internal Texture ExecuteInternal(RenderingContext context)
    {
        if(_isDisposed) throw new ObjectDisposedException("Node was disposed before execution.");
        
        if (!CacheChanged(context)) return CachedResult;

        CachedResult = OnExecute(context);
        if (CachedResult is { IsDisposed: true })
        {
            throw new ObjectDisposedException("Texture was disposed after execution.");
        }
        
        UpdateCache(context);
        return CachedResult;
    }

    protected abstract Texture? OnExecute(RenderingContext context);

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

    protected Texture RequestTexture(int id, VecI size, bool clear = true)
    {
        if (_managedTextures.TryGetValue(id, out var texture))
        {
            if (texture.Size != size)
            {
                texture.Dispose();
                texture = new Texture(size);
                _managedTextures[id] = texture;
                return texture;
            }
            
            if (clear)
            {
                texture.DrawingSurface.Canvas.Clear();
            }
            
            return texture;
        }
        
        _managedTextures[id] = new Texture(size);
        return _managedTextures[id];
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

    public void SetKeyFrameVisibility(Guid id, bool isVisible)
    {
        KeyFrameData frame = keyFrames.FirstOrDefault(x => x.KeyFrameGuid == id);
        if (frame is not null)
        {
            frame.IsVisible = isVisible;
            _keyFramesDirty = true;
        }
    }

    public void AddFrame(Guid id, KeyFrameData value)
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
        clone.Id = Guid.NewGuid();
        clone.Position = Position;

        for (var i = 0; i < clone.inputs.Count; i++)
        {
            var cloneInput = inputs[i];
            var newInput = cloneInput.Clone(clone);
            clone.inputs[i].NonOverridenValue = newInput.NonOverridenValue;
        }

        for (var i = 0; i < clone.outputs.Count; i++)
        {
            var cloneOutput = outputs[i];
            var newOutput = cloneOutput.Clone(clone);
            clone.outputs[i].Value = newOutput.Value; 
        }
        
        foreach (var keyFrame in keyFrames)
        {
            KeyFrameData newKeyFrame = new KeyFrameData(keyFrame.KeyFrameGuid, keyFrame.StartFrame, keyFrame.Duration,
                keyFrame.AffectedElement)
            {
                IsVisible = keyFrame.IsVisible, Duration = keyFrame.Duration,
                Data = keyFrame.Data is ICloneable cloneable ? cloneable.Clone() : keyFrame.Data
            };
            
            clone.keyFrames.Add(newKeyFrame);
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

    internal virtual void DeserializeData(IReadOnlyDocument target, IReadOnlyDictionary<string, object> data)
    {
    }
}
