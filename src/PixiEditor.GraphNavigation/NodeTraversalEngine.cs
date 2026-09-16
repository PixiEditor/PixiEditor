namespace PixiEditor.GraphNavigation;

/// <summary>
/// Represents the queue-based engine used to traverse a node graph with explicit control over when expansion occurs.
/// </summary>
/// <typeparam name="TNode">The node type being traversed.</typeparam>
/// <typeparam name="TInput">The concrete or interface type used for input properties.</typeparam>
/// <typeparam name="TOutput">The concrete or interface type used for output properties.</typeparam>
/// <remarks>
/// <para>
/// The engine maintains a breadth-first queue of <see cref="TraverseContext{TNode, TInput, TOutput}"/> values and
/// a <see cref="HashSet{TNode}"/> to prevent revisiting nodes once they have been yielded. This makes the engine
/// suitable for high-throughput traversal loops where the caller needs to decide expansion timing explicitly.
/// </para>
/// </remarks>
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

    /// <summary>
    /// Tries to retrieve the next unvisited traversal step from the queue.
    /// </summary>
    /// <param name="context">
    /// When this method returns <see langword="true"/>, contains the next step to process; otherwise the value is <c>default</c>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if another unvisited node is available; otherwise, <see langword="false"/>.
    /// </returns>
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

    /// <summary>
    /// Expands the current traversal step by enqueuing all connected predecessor nodes.
    /// </summary>
    /// <param name="context">The current traversal context whose incoming connections should be explored.</param>
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

    /// <summary>
    /// Expands the current traversal step by enqueuing all connected successor nodes.
    /// </summary>
    /// <param name="context">The current traversal context whose outgoing connections should be explored.</param>
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
