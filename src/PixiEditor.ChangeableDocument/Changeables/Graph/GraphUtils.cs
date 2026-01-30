using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public static class GraphUtils
{
    public static Queue<IReadOnlyNode> CalculateExecutionQueue(IReadOnlyNode outputNode,
        Func<IInputProperty, bool>? branchFilter = null)
    {
        var finalQueue = new HashSet<IReadOnlyNode>();
        var queueNodes = new Queue<IReadOnlyNode>();
        queueNodes.Enqueue(outputNode);

        while (queueNodes.Count > 0)
        {
            var node = queueNodes.Dequeue();

            if (finalQueue.Contains(node))
            {
                continue;
            }

            bool canAdd = true;

            foreach (var input in node.InputProperties)
            {
                if (input.Connection == null)
                {
                    continue;
                }

                if (finalQueue.Contains(input.Connection.Node))
                {
                    continue;
                }

                if (branchFilter != null && !branchFilter(input))
                {
                    continue;
                }

                canAdd = false;

                if (finalQueue.Contains(input.Connection.Node))
                {
                    finalQueue.Remove(input.Connection.Node);
                    finalQueue.Add(input.Connection.Node);
                }

                if (!queueNodes.Contains(input.Connection.Node))
                {
                    queueNodes.Enqueue(input.Connection.Node);
                }
            }

            if (canAdd)
            {
                finalQueue.Add(node);
            }
            else
            {
                queueNodes.Enqueue(node);
            }
        }

        return new Queue<IReadOnlyNode>(finalQueue);
    }

    public static int CalculateInputsHash(Node node)
    {
        HashCode hash = new();
        foreach (var input in node.InputProperties)
        {
            hash.Add(input.InternalPropertyName);
            hash.Add(input.ValueType);
        }

        return hash.ToHashCode();
    }

    public static int CalculateOutputsHash(Node node)
    {
        HashCode hash = new();
        foreach (var output in node.OutputProperties)
        {
            hash.Add(output.InternalPropertyName);
            hash.Add(output.ValueType);
        }

        return hash.ToHashCode();
    }
}
