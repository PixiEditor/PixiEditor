using Drawie.Backend.Core.Shaders.Generation;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
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

    private bool iterationInProgress = false;
    private Guid virtualSessionId;
    private Queue<IReadOnlyNode> unrolledQueue;
    private List<IReadOnlyNode> clonedNodes = new List<IReadOnlyNode>();

    public RepeatNodeStart()
    {
        Iterations = CreateInput<int>("Iterations", "ITERATIONS", 1);
        Input = CreateInput<object>("Input", "INPUT", null);
        CurrentIteration = CreateOutput<int>("CurrentIteration", "CURRENT_ITERATION", 1);
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

        if (iterationInProgress)
        {
            return;
        }

        iterationInProgress = true;

        if (iterations == 0)
        {
            Output.Value = null;
            CurrentIteration.Value = 0;
            iterationInProgress = false;
            return;
        }

        if (iterations > 1)
        {
            ClearLastUnrolledNodes();
            virtualSessionId = Guid.NewGuid();
            context.BeginVirtualConnectionScope(virtualSessionId);
            var unrollQueue = GraphUtils.CalculateExecutionQueue(endNode, false, true,
                property => property.Connection?.Node != this);
            queue = UnrollLoop(iterations, unrollQueue, context);
        }

        CurrentIteration.Value = 1;
        Output.Value = Input.Value;

        foreach (var node in queue)
        {
            context.SetActiveVirtualConnectionScope(virtualSessionId);
            node.Execute(context);
        }

        iterationInProgress = false;
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
        var originalConnectedToIteration = CurrentIteration.Connections;

        Queue<IReadOnlyNode> lastQueue = new Queue<IReadOnlyNode>(executionQueue.Where(x => x != this && x != endNode));
        for (int i = 0; i < iterations - 1; i++)
        {
            var mapping = new Dictionary<Guid, Node>();
            CloneNodes(lastQueue, mapping, virtualSessionId, context, i + 2);
            connectPreviousTo =
                ReplaceConnections(connectToNextStart, connectPreviousTo, mapping, virtualSessionId, context);
            connectToNextStart = mapping[connectToNextStart.Node.Id].OutputProperties
                .FirstOrDefault(y => y.InternalPropertyName == connectToNextStart.InternalPropertyName);

            originalConnectedToIteration =
                SetIterationConstants(mapping, originalConnectedToIteration, i + 2, virtualSessionId, context);

            clonedNodes.AddRange(mapping.Values);
            lastQueue = new Queue<IReadOnlyNode>(mapping.Values);
        }

        connectToNextStart.VirtualConnectTo(endNode.Input, virtualSessionId, context);

        var finalQueue = GraphUtils.CalculateExecutionQueue(endNode, true, true,
            property => property.Connection?.Node != this);
        return finalQueue;
    }

    private List<IInputProperty> SetIterationConstants(Dictionary<Guid, Node> mapping,
        IReadOnlyCollection<IInputProperty> originalConnectedToIteration, int iteration, Guid virtualConnectionId,
        RenderContext context)
    {
        List<IInputProperty> iterationInputs = new List<IInputProperty>();
        foreach (var input in originalConnectedToIteration)
        {
            if (mapping.TryGetValue(input.Node.Id, out var mappedNode))
            {
                var mappedInput =
                    mappedNode.InputProperties.FirstOrDefault(i =>
                        i.InternalPropertyName == input.InternalPropertyName);

                if (mappedInput == null) continue;
                mappedInput.SetVirtualNonOverridenValue(iteration, virtualConnectionId, context);
                iterationInputs.Add(mappedInput);
            }
        }

        return iterationInputs;
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

    private void CloneNodes(Queue<IReadOnlyNode> originalQueue, Dictionary<Guid, Node> mapping, Guid virtualSession,
        RenderContext context, int i)
    {
        foreach (var node in originalQueue)
        {
            if (node is not Node n) continue;
            Node clonedNode;
            clonedNode = n.Clone();

            mapping[node.Id] = clonedNode;
        }

        ConnectRelatedNodes(originalQueue, mapping, virtualSession, context, i);
    }

    private void ConnectRelatedNodes(Queue<IReadOnlyNode> originalQueue, Dictionary<Guid, Node> mapping,
        Guid virtualSession, RenderContext context, int i1)
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
                        output.ConnectTo(inputProp); // No need for virtual connection as it is a cloned node anyway
                    }
                }
                // Leaving this in case external connections are not working as intended. It might help, but no guarantees.
                /*else if (input.Connection != null)
                {
                    var clonedInput =
                        clonedNode.InputProperties.FirstOrDefault(i =>
                            i.InternalPropertyName == input.InternalPropertyName);
                    if (clonedInput is { Connection: null } && input.Connection != Output &&
                        !mapping.TryGetValue(input.Connection.Node.Id, out _))
                    {
                        if (input.Connection == CurrentIteration)
                        {
                            clonedInput.SetVirtualNonOverridenValue(i1, virtualSession, context);
                            continue;
                        }

                        input.Connection.VirtualConnectTo(clonedInput, virtualSession, context);
                    }
                }*/
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
