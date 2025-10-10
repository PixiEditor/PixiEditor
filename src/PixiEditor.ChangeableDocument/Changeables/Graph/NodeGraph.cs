using System.Collections.Immutable;
using System.Diagnostics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class NodeGraph : IReadOnlyNodeGraph
{
    private Dictionary<IReadOnlyNode, ImmutableList<IReadOnlyNode>?> cachedExecutionList;

    private readonly List<Node> _nodes = new();
    public IReadOnlyCollection<Node> Nodes => _nodes;
    public IReadOnlyDictionary<Guid, Node> NodeLookup => nodeLookup;
    public Node? OutputNode => CustomOutputNode ?? Nodes.OfType<OutputNode>().FirstOrDefault();
    public Node? CustomOutputNode { get; set; }

    public Blackboard Blackboard { get; } = new();

    private Dictionary<Guid, Node> nodeLookup = new();

    IReadOnlyCollection<IReadOnlyNode> IReadOnlyNodeGraph.AllNodes => Nodes;
    IReadOnlyNode IReadOnlyNodeGraph.OutputNode => OutputNode;
    IReadOnlyBlackboard IReadOnlyNodeGraph.Blackboard => Blackboard;

    bool isExecuting = false;

    public void AddNode(Node node)
    {
        if (Nodes.Contains(node))
        {
            return;
        }

        node.ConnectionsChanged += ResetCache;
        _nodes.Add(node);
        nodeLookup[node.Id] = node;
        ResetCache();
    }

    public void RemoveNode(Node node)
    {
        if (!Nodes.Contains(node))
        {
            return;
        }

        node.ConnectionsChanged -= ResetCache;
        _nodes.Remove(node);
        nodeLookup.Remove(node.Id);
        ResetCache();
    }

    public Node? FindNode(Guid guid)
    {
        return nodeLookup.GetValueOrDefault(guid);
    }

    public T? FindNode<T>(Guid guid) where T : Node
    {
        return nodeLookup.TryGetValue(guid, out Node? node) && node is T typedNode ? typedNode : null;
    }

    public Queue<IReadOnlyNode> CalculateExecutionQueue(IReadOnlyNode outputNode)
    {
        return new Queue<IReadOnlyNode>(CalculateExecutionQueueInternal(outputNode));
    }

    public IReadOnlyNodeGraph Clone()
    {
        var newGraph = new NodeGraph();
        var nodeMapping = new Dictionary<Node, Node>();

        // Clone nodes
        foreach (var node in Nodes)
        {
            var clonedNode = node.Clone(true);
            newGraph.AddNode(clonedNode);
            nodeMapping[node] = clonedNode;
        }

        // Re-establish connections
        foreach (var node in Nodes)
        {
            var clonedNode = nodeMapping[node];
            foreach (var input in node.InputProperties)
            {
                if (input.Connection != null)
                {
                    var connectedNode = input.Connection.Node;
                    if (nodeMapping.TryGetValue(connectedNode as Node, out var clonedConnectedNode))
                    {
                        var clonedOutput = clonedConnectedNode.OutputProperties.FirstOrDefault(o =>
                            o.InternalPropertyName == input.Connection.InternalPropertyName);
                        var clonedInput = clonedNode.InputProperties.FirstOrDefault(i =>
                            i.InternalPropertyName == input.InternalPropertyName);
                        if (clonedOutput != null && clonedInput != null)
                        {
                            clonedOutput.ConnectTo(clonedInput);
                        }
                    }
                }
            }
        }

        // Set custom output node if applicable
        if (CustomOutputNode != null && nodeMapping.TryGetValue(CustomOutputNode, out var mappedOutputNode))
        {
            newGraph.CustomOutputNode = mappedOutputNode;
        }

        // Clone blackboard variables
        foreach (var kvp in Blackboard.Variables)
        {
            object valueCopy;
            if (kvp.Value.Value is ICloneable cloneable)
            {
                valueCopy = cloneable.Clone();
            }
            else
            {
                valueCopy = kvp.Value.Value;
            }

            newGraph.Blackboard.SetVariable(kvp.Key, kvp.Value.Type, valueCopy);
        }

        return newGraph;
    }

    private ImmutableList<IReadOnlyNode> CalculateExecutionQueueInternal(IReadOnlyNode outputNode)
    {
        var cached = this.cachedExecutionList?.GetValueOrDefault(outputNode);
        if (cached != null)
        {
            return cached;
        }

        var calculated = GraphUtils.CalculateExecutionQueue(outputNode).ToImmutableList();
        cachedExecutionList ??= new Dictionary<IReadOnlyNode, ImmutableList<IReadOnlyNode>?>();
        cachedExecutionList[outputNode] = calculated;
        return calculated;
    }

    void IReadOnlyNodeGraph.AddNode(IReadOnlyNode node) => AddNode((Node)node);

    void IReadOnlyNodeGraph.RemoveNode(IReadOnlyNode node) => RemoveNode((Node)node);

    public void Dispose()
    {
        foreach (var node in Nodes)
        {
            node.Dispose();
        }
    }

    public bool TryTraverse(Action<IReadOnlyNode> action)
    {
        return TryTraverse(OutputNode, action);
    }

    public bool TryTraverse(IReadOnlyNode end, Action<IReadOnlyNode> action)
    {
        if (end == null) return false;

        var queue = CalculateExecutionQueueInternal(end);

        foreach (var node in queue)
        {
            action(node);
        }

        return true;
    }


    public void Execute(RenderContext context)
    {
        Execute(OutputNode, context);
    }

    public void Execute(IReadOnlyNode end, RenderContext context)
    {
        //if (isExecuting) return;
        isExecuting = true;
        if (end == null) return;
        if (!CanExecute()) return;

        var queue = CalculateExecutionQueueInternal(end);

        foreach (var node in queue)
        {
            lock (node)
            {
                if (node is Node typedNode)
                {
                    if (typedNode.IsDisposed) continue;

                    typedNode.ExecuteInternal(context);
                }
                else
                {
                    node.Execute(context);
                }
            }
        }

        isExecuting = false;
    }

    private bool CanExecute()
    {
        foreach (var node in Nodes)
        {
            if (node.IsDisposed)
            {
                return false;
            }
        }

        return true;
    }

    private void ResetCache()
    {
        cachedExecutionList = null;
    }

    public int GetCacheHash()
    {
        HashCode hash = new();
        var queue = CalculateExecutionQueueInternal(OutputNode);

        foreach (var node in queue)
        {
            int nodeCache = node.GetCacheHash();
            hash.Add(nodeCache);
        }

        return hash.ToHashCode();
    }
}
