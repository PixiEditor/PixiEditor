using Drawie.Backend.Core.Shaders.Generation;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Utility;

[NodeInfo("RepeatStart")]
[PairNode(typeof(RepeatNodeEnd), "RepeatZone", true)]
public class RepeatNodeStart : Node, IPairNode
{
    public InputProperty<int> Iterations { get; }
    public InputProperty<object> Input { get; }
    public OutputProperty<int> CurrentIteration { get; }
    public OutputProperty<object> Output { get; }

    public Guid OtherNode { get; set; }
    private RepeatNodeEnd? endNode;


    private Guid virtualSessionId;
    private Queue<IReadOnlyNode> unrolledQueue;
    private List<IReadOnlyNode> clonedNodes = new List<IReadOnlyNode>();

    public RepeatNodeStart()
    {
        Iterations = CreateInput<int>("Iterations", "ITERATIONS", 1);
        Input = CreateInput<object>("Input", "INPUT", null);
        CurrentIteration = CreateOutput<int>("CurrentIteration", "CURRENT_ITERATION", 0);
        Output = CreateOutput<object>("Output", "OUTPUT", null);
    }

    protected override void OnExecute(RenderContext context)
    {
        endNode = FindEndNode();
        if (endNode == null)
        {
            return;
        }

        OtherNode = endNode?.Id ?? Guid.Empty;

        int iterations = Iterations.Value;
        var queue = GraphUtils.CalculateExecutionQueue(endNode, true, true,
            property => property.Connection?.Node != this);

        if (iterations == 0)
        {
            Output.Value = null;
            CurrentIteration.Value = 0;
            return;
        }

        if (iterations > 1)
        {
            virtualSessionId = Guid.NewGuid();
            context.BeginVirtualConnectionScope(virtualSessionId);
            ClearLastUnrolledNodes();
            queue = UnrollLoop(iterations, queue, context);
        }

        Output.Value = Input.Value;
        CurrentIteration.Value = 0; // TODO: Increment iteration in unrolled nodes

        foreach (var node in queue)
        {
            node.Execute(context);
        }
    }

    private void ClearLastUnrolledNodes()
    {
        if (clonedNodes.Count > 0)
        {
            foreach (var node in clonedNodes)
            {
                if (node is IDisposable disposable) disposable.Dispose();
            }

            clonedNodes.Clear();
        }
    }

    private Queue<IReadOnlyNode> UnrollLoop(int iterations, Queue<IReadOnlyNode> executionQueue, RenderContext context)
    {
        var connectToNextStart = endNode.Input.Connection;
        var connectPreviousTo = Output.Connections;

        Queue<IReadOnlyNode> lastQueue = new Queue<IReadOnlyNode>(executionQueue.Where(x => x != this && x != endNode));
        for (int i = 0; i < iterations - 1; i++)
        {
            var mapping = new Dictionary<Guid, Node>();
            CloneNodes(lastQueue, mapping);
            connectPreviousTo =
                ReplaceConnections(connectToNextStart, connectPreviousTo, mapping, virtualSessionId, context);
            connectToNextStart = mapping[connectToNextStart.Node.Id].OutputProperties
                .FirstOrDefault(y => y.InternalPropertyName == connectToNextStart.InternalPropertyName);

            clonedNodes.AddRange(mapping.Values);
            lastQueue = new Queue<IReadOnlyNode>(mapping.Values);
        }

        connectToNextStart.VirtualConnectTo(endNode.Input, virtualSessionId, context);

        return GraphUtils.CalculateExecutionQueue(endNode, true, true,
            property => property.Connection?.Node != this);
    }

    private IReadOnlyCollection<IInputProperty> ReplaceConnections(IOutputProperty? connectToNextStart,
        IReadOnlyCollection<IInputProperty> connectPreviousTo, Dictionary<Guid, Node> mapping, Guid virtualConnectionId,
        RenderContext context)
    {
        var connectPreviousToMapped = new List<IInputProperty>();
        foreach (var input in connectPreviousTo)
        {
            if (mapping.TryGetValue(input.Node.Id, out var mappedNode))
            {
                var mappedInput =
                    mappedNode.InputProperties.FirstOrDefault(i =>
                        i.InternalPropertyName == input.InternalPropertyName);
                if (mappedInput != null)
                {
                    connectPreviousToMapped.Add(mappedInput);
                }
            }
        }

        foreach (var input in connectPreviousToMapped)
        {
            connectToNextStart?.VirtualConnectTo(input, virtualConnectionId, context);
        }

        return connectPreviousToMapped;
    }

    private void CloneNodes(Queue<IReadOnlyNode> originalQueue, Dictionary<Guid, Node> mapping)
    {
        foreach (var node in originalQueue)
        {
            if (node is not Node n) continue;
            var clonedNode = n.Clone();
            mapping[node.Id] = clonedNode;
        }

        ConnectRelatedNodes(originalQueue, mapping);
    }

    private void ConnectRelatedNodes(Queue<IReadOnlyNode> originalQueue, Dictionary<Guid, Node> mapping)
    {
        foreach (var node in originalQueue)
        {
            if (node is not Node n) continue;
            var clonedNode = mapping[node.Id];

            foreach (var input in n.InputProperties)
            {
                if (input.Connection != null &&
                    mapping.TryGetValue(input.Connection.Node.Id, out var connectedClonedNode))
                {
                    var output = connectedClonedNode.OutputProperties.FirstOrDefault(o =>
                        o.InternalPropertyName == input.Connection.InternalPropertyName);
                    if (output != null)
                    {
                        var inputProp = clonedNode.InputProperties.FirstOrDefault(i =>
                            i.InternalPropertyName == input.InternalPropertyName);
                        output.ConnectTo(inputProp);
                    }
                }
            }
        }
    }

    private RepeatNodeEnd FindEndNode()
    {
        RepeatNodeEnd repeatNodeEnd = null;
        int nestingCount = 0;
        HashSet<Guid> visitedNodes = new HashSet<Guid>();
        TraverseForwards(node =>
        {
            if (node is RepeatNodeStart && node != this)
            {
                nestingCount++;
            }

            if (node is RepeatNodeEnd rightNode && nestingCount == 0 && rightNode.OtherNode == Id)
            {
                repeatNodeEnd = rightNode;
                return false;
            }

            if (node is RepeatNodeEnd && visitedNodes.Add(node.Id))
            {
                nestingCount--;
            }

            return true;
        });

        return repeatNodeEnd;
    }

    public override Node CreateCopy()
    {
        return new RepeatNodeStart();
    }
}
