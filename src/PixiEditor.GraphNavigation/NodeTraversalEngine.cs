namespace PixiEditor.GraphNavigation;

public readonly struct NodeTraversalEngine<TNode, TInput, TOutput> where TNode : class, INavigableNode<TNode, TInput, TOutput>
    where TInput : class, INavigableInputProperty<TNode, TInput, TOutput>
    where TOutput : class, INavigableOutputProperty<TNode, TInput, TOutput>
{
    private readonly HashSet<TNode> visited = [];
    private readonly Queue<TraverseContext<TNode, TInput, TOutput>> queue = new();

    public NodeTraversalEngine(TNode origin)
    {
        queue.Enqueue(TraverseContext<TNode, TInput, TOutput>.Origin(origin));
    }

    public bool TryGetNext(out TraverseContext<TNode, TInput, TOutput> context)
    {
        while (queue.Count > 0)
        {
            context = queue.Dequeue();
            if (visited.Add(context.Current))
            {
                return true;
            }
        }

        context = default;
        return false;
    }

    public void ExpandBackwards(in TraverseContext<TNode, TInput, TOutput> context)
    {
        foreach (var inputProperty in context.Current.Inputs)
        {
            var connectedOutput = inputProperty.ConnectedOutput;
            if (connectedOutput != null)
            {
                queue.Enqueue(context.Next(connectedOutput.Node, inputProperty, connectedOutput));
            }
        }
    }
    
    public void ExpandForwards(in TraverseContext<TNode, TInput, TOutput> context)
    {
        foreach (var outputProperty in context.Current.Outputs)
        {
            foreach (var connectedInput in outputProperty.ConnectedInputs)
            {
                queue.Enqueue(context.Next(connectedInput.Node, connectedInput, outputProperty));
            }
        }
    }
}
