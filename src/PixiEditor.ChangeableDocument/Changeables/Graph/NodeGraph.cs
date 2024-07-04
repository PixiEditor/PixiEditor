using System.Collections;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class NodeGraph : IReadOnlyNodeGraph, IDisposable
{
    private readonly List<Node> _nodes = new();
    public IReadOnlyCollection<Node> Nodes => _nodes;
    public OutputNode? OutputNode => Nodes.OfType<OutputNode>().FirstOrDefault();

    IReadOnlyCollection<IReadOnlyNode> IReadOnlyNodeGraph.AllNodes => Nodes;
    IReadOnlyNode IReadOnlyNodeGraph.OutputNode => OutputNode;

    public void AddNode(Node node)
    {
        if (Nodes.Contains(node))
        {
            return;
        }

        _nodes.Add(node);
    }

    public void RemoveNode(Node node)
    {
        if (!Nodes.Contains(node))
        {
            return;
        }

        _nodes.Remove(node);
    }

    public ChunkyImage Execute()
    {
        if (OutputNode == null) return null;

        var queue = CalculateExecutionQueue(OutputNode);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();

            node.Execute(0);
        }

        return OutputNode.Input.Value;
    }

    private Queue<IReadOnlyNode> CalculateExecutionQueue(OutputNode outputNode)
    {
        // backwards breadth-first search
        var visited = new HashSet<IReadOnlyNode>();
        var queueNodes = new Queue<IReadOnlyNode>();
        List<IReadOnlyNode> finalQueue = new();
        queueNodes.Enqueue(outputNode);

        while (queueNodes.Count > 0)
        {
            var node = queueNodes.Dequeue();
            if (!visited.Add(node))
            {
                continue;
            }

            finalQueue.Add(node);

            foreach (var input in node.InputProperties)
            {
                if (input.Connection == null)
                {
                    continue;
                }

                queueNodes.Enqueue(input.Connection.Node);
            }
        }

        finalQueue.Reverse();
        return new Queue<IReadOnlyNode>(finalQueue);
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
        
        var queue = CalculateExecutionQueue(OutputNode);
        
        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            action(node);
        }
        
        return true;
    }
}
