using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public static class GraphUtils
{
    public static Queue<IReadOnlyNode> CalculateExecutionQueue(IReadOnlyNode outputNode,
        bool considerFlowNodes = false,
        Func<IInputProperty, bool>? branchFilter = null)
    {
        var finalQueue = new HashSet<IReadOnlyNode>();
        var queueNodes = new Queue<IReadOnlyNode>();
        var nodesToExclude = new HashSet<IReadOnlyNode>();
        queueNodes.Enqueue(outputNode);

        while (queueNodes.Count > 0)
        {
            var node = queueNodes.Dequeue();

            if (finalQueue.Contains(node))
            {
                continue;
            }

            if (considerFlowNodes && node is IExecutionFlowNode flowNode && !nodesToExclude.Contains(node))
            {
                var handledNodes = flowNode.HandledNodes;
                foreach (var handled in handledNodes)
                {
                    nodesToExclude.Add(handled);
                }
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

        finalQueue = new HashSet<IReadOnlyNode>(finalQueue.Except(nodesToExclude));

        return new Queue<IReadOnlyNode>(finalQueue);
    }

    public static Queue<IReadOnlyNode> CalculateExecutionQueue(IReadOnlyNode outputNode,
        bool considerFlowNodes,
        bool ignoreOutputFlowNode,
        Func<IInputProperty, bool>? branchFilter = null)
    {
        var finalQueue = new HashSet<IReadOnlyNode>();
        var queueNodes = new Queue<IReadOnlyNode>();
        var nodesToExclude = new HashSet<IReadOnlyNode>();
        queueNodes.Enqueue(outputNode);

        while (queueNodes.Count > 0)
        {
            var node = queueNodes.Dequeue();

            if (finalQueue.Contains(node))
            {
                continue;
            }

            if (considerFlowNodes && node is IExecutionFlowNode flowNode && !nodesToExclude.Contains(node) && !(ignoreOutputFlowNode && node == outputNode))
            {
                var handledNodes = flowNode.HandledNodes;
                foreach (var handled in handledNodes)
                {
                    nodesToExclude.Add(handled);
                }
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

        if(ignoreOutputFlowNode && outputNode is IExecutionFlowNode)
            nodesToExclude.Add(outputNode);

        finalQueue = new HashSet<IReadOnlyNode>(finalQueue.Except(nodesToExclude));

        return new Queue<IReadOnlyNode>(finalQueue);
    }
}
