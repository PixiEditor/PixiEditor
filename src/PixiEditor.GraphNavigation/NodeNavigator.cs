using System.Diagnostics.CodeAnalysis;

namespace PixiEditor.GraphNavigation;

/// <summary>
/// Provides directional graph traversal capabilities starting from a designated origin node.
/// </summary>
/// <typeparam name="TNode">The type of node being navigated.</typeparam>
/// <typeparam name="TInput">The concrete or interface type of input properties.</typeparam>
/// <typeparam name="TOutput">The concrete or interface type of output properties.</typeparam>
/// <param name="origin">The root node from which traversal operations begin.</param>
public readonly struct NodeNavigator<TNode, TInput, TOutput>(TNode origin)
    where TNode : class, INavigableNode<TNode, TInput, TOutput>
    where TInput : class, INavigableInputProperty<TNode, TInput, TOutput>
    where TOutput : class, INavigableOutputProperty<TNode, TInput, TOutput>
{
    /// <summary>
    /// Traverses the graph backwards (upstream) from the origin node towards its inputs.
    /// </summary>
    /// <param name="func">
    /// A callback delegate invoked for each step in the traversal. Receives the current 
    /// <see cref="TraverseContext{TNode, TInput, TOutput}"/> and returns a <see cref="Traverse"/> value controlling flow continuation.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="func"/> returns an unhandled or invalid <see cref="Traverse"/> enum value.
    /// </exception>
    public void TraverseBackwards(Func<TraverseContext<TNode, TInput, TOutput>, Traverse> func)
    {
        var visited = new HashSet<TNode>();
        var queueNodes = new Queue<TraverseContext<TNode, TInput, TOutput>>();
        queueNodes.Enqueue(TraverseContext<TNode, TInput, TOutput>.Origin(origin));

        while (queueNodes.Count > 0)
        {
            var context = queueNodes.Dequeue();

            if (!visited.Add(context.Current))
            {
                continue;
            }

            var result = func(context);

            switch (result)
            {
                case Traverse.NoFurther:
                    continue;
                case Traverse.Exit:
                    return;
                case Traverse.Further:
                    break;
                default:
                    ThrowInvalidTraverseResult(result);
                    break;
            }

            foreach (var inputProperty in context.Current.Inputs)
            {
                var connectedOutput = inputProperty.ConnectedOutput;
                if (connectedOutput != null)
                {
                    queueNodes.Enqueue(context.Next(connectedOutput.Node, inputProperty, connectedOutput));
                }
            }
        }
    }

    /// <summary>
    /// Traverses the graph forwards (downstream) from the origin node towards its outputs.
    /// </summary>
    /// <param name="func">
    /// A callback delegate invoked for each step in the traversal. Receives the current 
    /// <see cref="TraverseContext{TNode, TInput, TOutput}"/> and returns a <see cref="Traverse"/> value controlling flow continuation.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="func"/> returns an unhandled or invalid <see cref="Traverse"/> enum value.
    /// </exception>
    public void TraverseForwards(Func<TraverseContext<TNode, TInput, TOutput>, Traverse> func)
    {
        var visited = new HashSet<TNode>();
        var queueNodes = new Queue<TraverseContext<TNode, TInput, TOutput>>();
        queueNodes.Enqueue(TraverseContext<TNode, TInput, TOutput>.Origin(origin));

        while (queueNodes.Count > 0)
        {
            var context = queueNodes.Dequeue();

            if (!visited.Add(context.Current))
            {
                continue;
            }

            var result = func(context);

            switch (result)
            {
                case Traverse.NoFurther:
                    continue;
                case Traverse.Exit:
                    return;
                case Traverse.Further:
                    break;
                default:
                    ThrowInvalidTraverseResult(result);
                    break;
            }

            foreach (var outputProperty in context.Current.Outputs)
            {
                foreach (var connectedInput in outputProperty.ConnectedInputs)
                {
                    queueNodes.Enqueue(context.Next(connectedInput.Node, connectedInput, outputProperty));
                }
            }
        }
    }

    [DoesNotReturn]
    private void ThrowInvalidTraverseResult(Traverse traverse) =>
        throw new IndexOutOfRangeException($"Invalid Traverse Option '{traverse}'");
}
