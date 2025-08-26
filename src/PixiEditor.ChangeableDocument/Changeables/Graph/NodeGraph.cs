using System.Collections.Immutable;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class NodeGraph : IReadOnlyNodeGraph
{
    private ImmutableList<IReadOnlyNode>? cachedExecutionList;
    
    private readonly List<Node> _nodes = new();
    public IReadOnlyCollection<Node> Nodes => _nodes;
    public IReadOnlyDictionary<Guid, Node> NodeLookup => nodeLookup;
    public Node? OutputNode => CustomOutputNode ?? Nodes.OfType<OutputNode>().FirstOrDefault();
    public Node? CustomOutputNode { get; set; }

    private Dictionary<Guid, Node> nodeLookup = new();

    IReadOnlyCollection<IReadOnlyNode> IReadOnlyNodeGraph.AllNodes => Nodes;
    IReadOnlyNode IReadOnlyNodeGraph.OutputNode => OutputNode;


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
                        var clonedOutput = clonedConnectedNode.OutputProperties.FirstOrDefault(o => o.InternalPropertyName == input.Connection.InternalPropertyName);
                        var clonedInput = clonedNode.InputProperties.FirstOrDefault(i => i.InternalPropertyName == input.InternalPropertyName);
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

        return newGraph;
    }

    private ImmutableList<IReadOnlyNode> CalculateExecutionQueueInternal(IReadOnlyNode outputNode)
    {
        return cachedExecutionList ??= GraphUtils.CalculateExecutionQueue(outputNode).ToImmutableList();
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
        if(OutputNode == null) return false;
        
        var queue = CalculateExecutionQueueInternal(OutputNode);
        
        foreach (var node in queue)
        {
            action(node);
        }
        
        return true;
    }

    bool isexecuting = false;
    public void Execute(RenderContext context)
    {
        if (isexecuting) return;
        isexecuting = true;
        if (OutputNode == null) return;
        if(!CanExecute()) return;

        var queue = CalculateExecutionQueueInternal(OutputNode);
        
        foreach (var node in queue)
        {
            if (node is Node typedNode)
            {
                if(typedNode.IsDisposed) continue;
                
                typedNode.ExecuteInternal(context);
            }
            else
            {
                node.Execute(context);
            }
        }
        isexecuting = false;
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
        foreach (var node in Nodes)
        {
            hash.Add(node.GetCacheHash());
        }

        return hash.ToHashCode();
    }
}
