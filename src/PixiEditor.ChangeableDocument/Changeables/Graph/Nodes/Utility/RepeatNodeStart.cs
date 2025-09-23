using System.Diagnostics;
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
    private Queue<IReadOnlyNode> unrolledQueue;
    private List<IReadOnlyNode> clonedNodes = new List<IReadOnlyNode>();

    private Queue<IReadOnlyNode> cachedExecutionQueue;
    private int lastHash = 0;

    public RepeatNodeStart()
    {
        Iterations = CreateInput<int>("Iterations", "ITERATIONS", 1)
            .WithRules(x => x.Min(0));
        Input = CreateInput<object>("Input", "INPUT", null);
        CurrentIteration = CreateOutput<int>("CurrentIteration", "CURRENT_ITERATION", 1);
        Output = CreateOutput<object>("Output", "OUTPUT", null);
    }

    protected override void OnExecute(RenderContext context)
    {
        try
        {
            if (iterationInProgress)
            {
                return;
            }

            iterationInProgress = true;

            endNode = FindEndNode();
            if (endNode == null)
            {
                return;
            }

            OtherNode = endNode?.Id ?? Guid.Empty;
            endNode.startNode = this;
            int iterations = Iterations.Value;

            CurrentIteration.Value = 0;

            if (iterations <= 0)
            {
                Output.Value = null;
                iterationInProgress = false;
                return;
            }

            if (iterations > 1)
            {
                /*var unrollQueue = GraphUtils.CalculateExecutionQueue(endNode, false, true,
                    property => property.Connection?.Node != this);*/
                var unrollQueue = endNode.HandledNodes;


                //int currentHash = GetGraphCache(unrollQueue);
                //if (cachedExecutionQueue == null || lastHash != currentHash)
                {
                    ClearLastUnrolledNodes();

                    context.BeginVirtualConnectionScope(context.ContextVirtualSession);
                    cachedExecutionQueue =
                        UnrollLoop(iterations, unrollQueue, context, context.ContextVirtualSession);


                    //lastHash = currentHash;
                }
            }
            else
            {
                cachedExecutionQueue = GraphUtils.CalculateExecutionQueue(endNode, true, true,
                    property => property.Connection?.Node != this);
            }

            CurrentIteration.Value = 1;
            Output.Value = Input.Value;

            foreach (var node in cachedExecutionQueue)
            {
                context.SetActiveVirtualConnectionScope(context.ContextVirtualSession);
                node.Execute(context);
                if (node is RepeatNodeEnd)
                {
                    CurrentIteration.Value = Math.Min(CurrentIteration.Value + 1, iterations);
                }
            }
        }
        catch (Exception ex)
        {
            iterationInProgress = false;
            throw;
        }

        iterationInProgress = false;
    }

    private int GetGraphCache(Queue<IReadOnlyNode> standardGraph)
    {
        HashCode hash = new HashCode();
        if (standardGraph == null) return 0;
        foreach (var node in standardGraph)
        {
            hash.Add(node.GetCacheHash());
        }

        return hash.ToHashCode();
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

    internal Queue<IReadOnlyNode> UnrollLoop(int iterations, HashSet<IReadOnlyNode> executionQueue,
        RenderContext context,
        Guid virtualSession)
    {
        if (endNode == null)
        {
            endNode = FindEndNode();
            if (endNode == null)
            {
                return new Queue<IReadOnlyNode>();
            }
        }

        var connectToNextStart = endNode.Input.Connection;
        var connectPreviousTo = Output.Connections;
        var originalConnectedToIteration = new List<IInputProperty>(CurrentIteration.Connections);

        foreach (var input in CurrentIteration.Connections)
        {
            if (input.Connection != null)
            {
                input.SetVirtualNonOverridenValue(1, context.ContextVirtualSession, context);
            }
        }

        HashSet<IReadOnlyNode> lastQueue = executionQueue;
        Dictionary<Guid, Guid> originalIdMappings = new Dictionary<Guid, Guid>();
        for (int i = 0; i < iterations - 1; i++)
        {
            var mapping = new Dictionary<Guid, Node>();
            CloneNodes(lastQueue, mapping, originalIdMappings);

            connectPreviousTo =
                ReplaceConnections(connectToNextStart, connectPreviousTo, mapping, context.ContextVirtualSession,
                    context);

            connectToNextStart = mapping[connectToNextStart.Node.Id].OutputProperties
                .TryGetProperty(connectToNextStart.InternalPropertyName);

            ConnectExternalConnectionsToClonedNodes(lastQueue, mapping, context, context.ContextVirtualSession, i + 2);

            originalConnectedToIteration =
                SetIterationConstants(mapping, originalConnectedToIteration, i + 2, context.ContextVirtualSession,
                    context);

            clonedNodes.AddRange(mapping.Values);
            lastQueue = new HashSet<IReadOnlyNode>(mapping.Values);
        }


        connectToNextStart.VirtualConnectTo(endNode.Input, context.ContextVirtualSession, context);

        var finalQueue = GraphUtils.CalculateExecutionQueue(endNode, true, true,
            property => property.Connection?.Node != this);
        return finalQueue;
    }

    private void ConnectExternalConnectionsToClonedNodes(HashSet<IReadOnlyNode> originalQueue,
        Dictionary<Guid, Node> mapping, RenderContext context, Guid virtualSession, int i1)
    {
        foreach (var node in originalQueue)
        {
            if (node is not Node n) continue;
            if (!mapping.TryGetValue(node.Id, out var clonedNode))
            {
                continue;
            }

            foreach (var input in n.InputProperties)
            {
                if (input.Connection != null)
                {
                    var clonedInput =
                        clonedNode.InputProperties.FirstOrDefault(i =>
                            i.InternalPropertyName == input.InternalPropertyName);
                    if (clonedInput is { Connection: null } && input.Connection != Output &&
                        !mapping.TryGetValue(input.Connection.Node.Id, out _))
                    {
                        if (input.Connection.InternalPropertyName == CurrentIteration.InternalPropertyName)
                        {
                            if (input.Connection == CurrentIteration)
                            {
                                var iteration = i1;
                                clonedInput.SetVirtualNonOverridenValue(iteration, virtualSession, context);
                            }
                            else if (input.Connection.Node is RepeatNodeStart start)
                            {
                                //start.CurrentIteration.VirtualConnectTo(clonedInput, virtualSession, context);
                            }

                        }
                        /*if (input.Connection == CurrentIteration)
                        {
                            clonedInput.SetVirtualNonOverridenValue(i1, virtualSession, context);
                            continue;
                        }*/

                        //input.Connection.VirtualConnectTo(clonedInput, virtualSession, context);
                    }
                }
            }
        }
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
                    mappedNode.InputProperties.TryGetProperty(input.InternalPropertyName);

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
                    mappedNode.InputProperties.TryGetProperty(input.InternalPropertyName);
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

    private void CloneNodes(HashSet<IReadOnlyNode> originalQueue, Dictionary<Guid, Node> mapping,
        Dictionary<Guid, Guid> originalIdMappings)
    {
        foreach (var node in originalQueue)
        {
            if (node is not Node n) continue;
            if (node == this || node == endNode || endNode.HandledNodes.All(x =>
                    x.Id != node.Id && x.Id != originalIdMappings.GetValueOrDefault(node.Id))) continue;
            var clonedNode = n.Clone();

            mapping[node.Id] = clonedNode;
            Guid originalId = originalIdMappings.ContainsKey(node.Id)
                ? originalIdMappings[node.Id]
                : node.Id;
            originalIdMappings[clonedNode.Id] = originalId;
        }

        ConnectRelatedNodes(originalQueue, mapping);
    }

    private void ConnectRelatedNodes(HashSet<IReadOnlyNode> originalQueue, Dictionary<Guid, Node> mapping)
    {
        foreach (var node in originalQueue)
        {
            if (node is not Node n) continue;
            if (!mapping.TryGetValue(node.Id, out var clonedNode))
            {
                continue;
            }

            foreach (var input in n.InputProperties)
            {
                if (input.Connection != null &&
                    mapping.TryGetValue(input.Connection.Node.Id, out var connectedClonedNode))
                {
                    var output =
                        connectedClonedNode.OutputProperties.TryGetProperty(input.Connection.InternalPropertyName);
                    if (output != null)
                    {
                        var inputProp = clonedNode.InputProperties.TryGetProperty(input.InternalPropertyName);
                        output.ConnectTo(inputProp); // No need for virtual connection as it is a cloned node anyway
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
