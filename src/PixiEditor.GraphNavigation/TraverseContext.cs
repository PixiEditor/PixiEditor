namespace PixiEditor.GraphNavigation;

/// <summary>
/// Represents the state and context of a single step during graph traversal with strongly-typed property references.
/// </summary>
/// <typeparam name="TNode">The type of node being traversed.</typeparam>
/// <typeparam name="TInput">The concrete or interface type of input properties.</typeparam>
/// <typeparam name="TOutput">The concrete or interface type of output properties.</typeparam>
public readonly struct TraverseContext<TNode, TInput, TOutput>(
    TNode current,
    TNode? adjacent,
    TInput? inputProperty,
    TOutput? outputProperty)
    where TNode : class, INavigableNode<TNode, TInput, TOutput>
    where TInput : class, INavigableInputProperty<TNode, TInput, TOutput>
    where TOutput : class, INavigableOutputProperty<TNode, TInput, TOutput>
{
    /// <summary>
    /// Gets the node currently being evaluated in this traversal step.
    /// </summary>
    public TNode Current { get; } = current;

    /// <summary>
    /// Gets the node that led to <see cref="Current"/>, or <see langword="null"/> if <see cref="Current"/> is the origin node.
    /// </summary>
    public TNode? Adjacent { get; } = adjacent;

    /// <summary>
    /// Gets the input property involved in the connection between <see cref="Current"/> and <see cref="Adjacent"/>, if any.
    /// </summary>
    public TInput? InputProperty { get; } = inputProperty;

    /// <summary>
    /// Gets the output property involved in the connection between <see cref="Current"/> and <see cref="Adjacent"/>, if any.
    /// </summary>
    public TOutput? OutputProperty { get; } = outputProperty;

    /// <summary>
    /// Creates the initial context for the starting point of a graph traversal.
    /// </summary>
    /// <param name="origin">The root node from which traversal begins.</param>
    /// <returns>A new <see cref="TraverseContext{TNode, TInput, TOutput}"/> initialized for the origin node.</returns>
    internal static TraverseContext<TNode, TInput, TOutput> Origin(TNode origin) => new(origin, null, null, null);

    /// <summary>
    /// Creates a new context for stepping to a connected node, setting the current node as <see cref="Adjacent"/>.
    /// </summary>
    /// <param name="next">The target node being stepped into.</param>
    /// <param name="inputProperty">The input property forming the edge connection, if applicable.</param>
    /// <param name="outputProperty">The output property forming the edge connection, if applicable.</param>
    /// <returns>A new <see cref="TraverseContext{TNode, TInput, TOutput}"/> representing the next step in traversal.</returns>
    internal TraverseContext<TNode, TInput, TOutput> Next(
        TNode next,
        TInput? inputProperty,
        TOutput? outputProperty) => new(next, Current, inputProperty, outputProperty);
}
