using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public abstract class Node(Guid? id = null) : IReadOnlyNode, IDisposable
{
    private List<InputProperty> inputs = new();
    private List<OutputProperty> outputs = new();

    private List<IReadOnlyNode> _connectedNodes = new();

    public Guid Id { get; internal set; } = id ?? Guid.NewGuid();

    public IReadOnlyCollection<InputProperty> InputProperties => inputs;
    public IReadOnlyCollection<OutputProperty> OutputProperties => outputs;
    public IReadOnlyCollection<IReadOnlyNode> ConnectedOutputNodes => _connectedNodes;

    IReadOnlyCollection<IInputProperty> IReadOnlyNode.InputProperties => inputs;
    IReadOnlyCollection<IOutputProperty> IReadOnlyNode.OutputProperties => outputs;
    public VecD Position { get; set; }

    public ChunkyImage? Execute(KeyFrameTime frameTime)
    {
        foreach (var output in outputs)
        {
            foreach (var connection in output.Connections)
            {
                connection.Value = output.Value;
            }
        }

        return OnExecute(frameTime);
    }

    public abstract ChunkyImage? OnExecute(KeyFrameTime frameTime);
    public abstract bool Validate();

    public void RemoveKeyFrame(Guid keyFrameGuid)
    {
        // TODO: Implement
    }

    public void SetKeyFrameLength(Guid keyFrameGuid, int startFrame, int duration)
    {
        // TODO: Implement
    }

    public void AddFrame<T>(Guid keyFrameGuid, int startFrame, int duration, T value)
    {
        // TODO: Implement
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
                foreach (var outputNode in ConnectedOutputNodes)
                {
                    queueNodes.Enqueue(outputNode);
                }
            }
        }
    }

    protected InputProperty<T> CreateInput<T>(string name, T defaultValue)
    {
        var property = new InputProperty<T>(this, name, defaultValue);
        inputs.Add(property);
        return property;
    }

    protected OutputProperty<T> CreateOutput<T>(string name, T defaultValue)
    {
        var property = new OutputProperty<T>(this, name, defaultValue);
        outputs.Add(property);
        property.Connected += (input, _) => _connectedNodes.Add(input.Node);
        property.Disconnected += (input, _) => _connectedNodes.Remove(input.Node);
        return property;
    }

    public virtual void Dispose()
    {
        foreach (var input in inputs)
        {
            if (input.Value is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        foreach (var output in outputs)
        {
            if (output.Value is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
    
    public virtual Node Clone()
    {
        var clone = (Node)MemberwiseClone();
        clone.Id = Guid.NewGuid();
        clone.inputs = new List<InputProperty>();
        clone.outputs = new List<OutputProperty>();
        clone._connectedNodes = new List<IReadOnlyNode>();
        foreach (var input in inputs)
        {
            var newInput = input.Clone(clone);
            clone.inputs.Add(newInput);
        }
        foreach (var output in outputs)
        {
            var newOutput = output.Clone(clone);
            clone.outputs.Add(newOutput);
        }
        return clone;
    }
}
