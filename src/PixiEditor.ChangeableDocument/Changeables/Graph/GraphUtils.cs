using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

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
}
