using System.Collections;
using System.Diagnostics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Surface.ImageData;

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

    private Queue<IReadOnlyNode> CalculateExecutionQueue(OutputNode outputNode, bool validate = true)
    {
        // backwards breadth-first search
        var visited = new HashSet<IReadOnlyNode>();
        var queueNodes = new Queue<IReadOnlyNode>();
        List<IReadOnlyNode> finalQueue = new();
        queueNodes.Enqueue(outputNode);

        while (queueNodes.Count > 0)
        {
            var node = queueNodes.Dequeue();
            if (!visited.Add(node) || (validate && !node.Validate()))
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
        
        var queue = CalculateExecutionQueue(OutputNode, false);
        
        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            action(node);
        }
        
        return true;
    }

    public Surface? Execute(RenderingContext context)
    {
        Stopwatch stopwatch = new();
        if (OutputNode == null) return null;

        var queue = CalculateExecutionQueue(OutputNode);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            
            stopwatch.Restart();
            node.Execute(context);
            Console.WriteLine($"{node.GetType().Name} took {stopwatch.ElapsedMilliseconds}ms to execute");
        }

        return OutputNode.Input.Value;
    }
}
