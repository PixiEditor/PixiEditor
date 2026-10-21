using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace PixiEditor.GraphNavigation;

/// <summary>
/// Provides a breadth-first traversal of a directed node graph starting from a designated origin node.
/// </summary>
/// <typeparam name="TNode">The node type being traversed.</typeparam>
/// <typeparam name="TInput">The concrete or interface type used for input properties.</typeparam>
/// <typeparam name="TOutput">The concrete or interface type used for output properties.</typeparam>
/// <param name="origin">The root node from which traversal begins.</param>
/// <remarks>
/// <para>
/// Each visited step is exposed as a <see cref="TraverseContext{TNode, TInput, TOutput}"/> that describes the
/// current node and the edge used to reach it. Traversal is breadth-first and nodes are visited at most once.
/// </para>
/// <para>
/// Use the <see cref="Backwards(bool)"/> and <see cref="Forwards(bool)"/> overloads for simple enumeration, or
/// provide a brancher delegate to stop, skip children, or continue traversal at each step.
/// </para>
/// </remarks>
public readonly struct NodeNavigator<TNode, TInput, TOutput>(TNode origin)
    where TNode : class, INavigableNode<TNode, TInput, TOutput>
    where TInput : class, INavigableInputProperty<TNode, TInput, TOutput>
    where TOutput : class, INavigableOutputProperty<TNode, TInput, TOutput>
{
    /// <summary>
    /// Traverses the graph in the output direction (right to left in UI) starting from the origin node.
    /// </summary>
    /// <param name="yieldOrigin">
    /// <see langword="true"/> to include the origin node in the enumeration. Otherwise the origin is not yielded.
    /// </param>
    /// <returns>An enumeration of traversal steps in reverse dependency order.</returns>
    [Pure]
    public IEnumerable<TraverseContext<TNode, TInput, TOutput>> Backwards(bool yieldOrigin = true) =>
        Backwards(null, yieldOrigin);

    /// <summary>
    /// Traverses the graph in the output direction (right to left in UI) and allows a callback to control flow for each visited step.
    /// </summary>
    /// <param name="brancher">
    /// A delegate that receives the current traversal context and returns the next action to take.
    /// </param>
    /// <param name="yieldOrigin">
    /// <see langword="true"/> to include the origin node in the enumeration; otherwise, the origin is evaluated
    /// but not yielded.
    /// </param>
    /// <returns>An enumeration of traversal steps whose expansion can be controlled by <paramref name="brancher"/>.</returns>
    [Pure]
    public IEnumerable<TraverseContext<TNode, TInput, TOutput>> Backwards(
        Func<TraverseContext<TNode, TInput, TOutput>, Traverse>? brancher, bool yieldOrigin = true)
    {
        var engine = new NodeTraversalEngine<TNode, TInput, TOutput>(origin);

        // Handle origin node setup when omitted from results. Still allows handling connection selection
        if (!yieldOrigin && engine.TryGetNext(out var originContext))
        {
            var originResult = brancher?.Invoke(originContext) ?? Traverse.Continue;

            switch (originResult)
            {
                case Traverse.Continue:
                    engine.ExpandBackwards(originContext);
                    break;

                case Traverse.ExitExclusive:
                case Traverse.ExitInclusive:

                case Traverse.SkipChildren:
                    yield break;

                default:
                    ThrowInvalidTraverseResult(originResult);
                    break;
            }
        }

        if (brancher == null)
        {
            while (engine.TryGetNext(out var context))
            {
                yield return context;
                engine.ExpandBackwards(context);
            }

            yield break;
        }

        while (engine.TryGetNext(out var context))
        {
            var result = brancher(context);

            switch (result)
            {
                case Traverse.ExitExclusive: // Do not yield current node (Exclusive)
                    yield break;

                case Traverse.ExitInclusive: // Yield current node (Inclusive)
                    yield return context;
                    yield break;

                case Traverse.SkipChildren:
                    yield return context;
                    break;

                case Traverse.Continue:
                    yield return context;
                    engine.ExpandBackwards(context);
                    break;

                default:
                    ThrowInvalidTraverseResult(result);
                    break;
            }
        }
    }

    /// <summary>
    /// Traverses the graph in the the input direction (left to right in UI) starting from the origin node.
    /// </summary>
    /// <param name="yieldOrigin">
    /// <see langword="true"/> to include the origin node in the enumeration. Otherwise the origin is not yielded.
    /// </param>
    /// <returns>An enumeration of traversal steps in forward dependency order.</returns>
    [Pure]
    public IEnumerable<TraverseContext<TNode, TInput, TOutput>> Forwards(bool yieldOrigin = true) =>
        Forwards(null, yieldOrigin);

    /// <summary>
    /// Traverses the graph in the input direction (left to right in UI) and allows a callback to control flow for each visited step.
    /// </summary>
    /// <param name="brancher">
    /// A delegate that receives the current traversal context and returns the next action to take.
    /// </param>
    /// <param name="yieldOrigin">
    /// <see langword="true"/> to include the origin node in the enumeration; otherwise, the origin is evaluated
    /// but not yielded.
    /// </param>
    /// <returns>An enumeration of traversal steps whose expansion can be controlled by <paramref name="brancher"/>.</returns>
    [Pure]
    public IEnumerable<TraverseContext<TNode, TInput, TOutput>> Forwards(
        Func<TraverseContext<TNode, TInput, TOutput>, Traverse>? brancher, bool yieldOrigin = true)
    {
        var engine = new NodeTraversalEngine<TNode, TInput, TOutput>(origin);

        // Handle origin node setup when omitted from results. Still allows handling connection selection
        if (!yieldOrigin && engine.TryGetNext(out var originContext))
        {
            var originResult = brancher?.Invoke(originContext) ?? Traverse.Continue;

            switch (originResult)
            {
                case Traverse.Continue:
                    engine.ExpandForwards(originContext);
                    break;

                case Traverse.ExitExclusive:
                case Traverse.ExitInclusive:

                case Traverse.SkipChildren:
                    yield break;

                default:
                    ThrowInvalidTraverseResult(originResult);
                    break;
            }
        }

        if (brancher == null)
        {
            while (engine.TryGetNext(out var context))
            {
                yield return context;
                engine.ExpandForwards(context);
            }

            yield break;
        }

        while (engine.TryGetNext(out var context))
        {
            var result = brancher(context);

            switch (result)
            {
                case Traverse.ExitExclusive: // Do not yield current node (Exclusive)
                    yield break;

                case Traverse.ExitInclusive: // Yield current node (Inclusive)
                    yield return context;
                    yield break;

                case Traverse.SkipChildren:
                    yield return context;
                    break;

                case Traverse.Continue:
                    yield return context;
                    engine.ExpandForwards(context);
                    break;

                default:
                    ThrowInvalidTraverseResult(result);
                    break;
            }
        }
    }

    public IEnumerable<TTarget> NodesOfType<TTarget>(
        NavigationDirection direction)
        where TTarget : class, TNode
    {
        var engine = new NodeTraversalEngine<TNode, TInput, TOutput>(origin);

        while (engine.TryGetNext(out var context))
        {
            if (context.Current is TTarget target)
            {
                yield return target;
            }

            engine.Expand(context, direction);
        }
    }

    public IEnumerable<TTarget> NodesOfTypeWhere<TTarget>(
        NavigationDirection direction,
        Func<TraverseContext<TNode, TInput, TOutput>, bool> predicate)
        where TTarget : class, TNode
    {
        var engine = new NodeTraversalEngine<TNode, TInput, TOutput>(origin);

        while (engine.TryGetNext(out var context))
        {
            if (context.Current is TTarget target && predicate(context))
            {
                yield return target;
            }

            engine.Expand(context, direction);
        }
    }

    [return: NotNullIfNotNull(nameof(defaultValue))]
    private TNode? FirstOrDefaultCore<TEvaluator>(
        NavigationDirection direction,
        TEvaluator evaluator,
        TNode? defaultValue,
        bool yieldOrigin)
        where TEvaluator : struct, IPredicateEvaluator<TNode, TInput, TOutput>
    {
        var engine = new NodeTraversalEngine<TNode, TInput, TOutput>(origin);

        if (!engine.TryGetNext(out var context))
            return defaultValue;

        if (yieldOrigin && evaluator.Evaluate(context))
            return context.Current;

        engine.Expand(context, direction);

        while (engine.TryGetNext(out context))
        {
            if (evaluator.Evaluate(context))
                return context.Current;

            engine.Expand(context, direction);
        }

        return defaultValue;
    }

    public bool Any(NavigationDirection direction, Func<TNode, bool> predicate, bool yieldOrigin = true)
        => FirstOrDefault(direction, predicate, defaultValue: null, yieldOrigin) != null;

    public bool Any(NavigationDirection direction, Func<TraverseContext<TNode, TInput, TOutput>, bool> predicate, bool yieldOrigin = true)
        => FirstOrDefault(direction, predicate, defaultValue: null, yieldOrigin) != null;

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public TNode? FirstOrDefault(NavigationDirection direction, Func<TNode, bool> predicate, TNode? defaultValue = null, bool yieldOrigin = true)
        => FirstOrDefaultCore(direction, new NodePredicate<TNode, TInput, TOutput>(predicate), defaultValue, yieldOrigin);

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public TNode? FirstOrDefault(NavigationDirection direction, Func<TraverseContext<TNode, TInput, TOutput>, bool> predicate, TNode? defaultValue = null, bool yieldOrigin = true)
        => FirstOrDefaultCore(direction, new ContextPredicate<TNode, TInput, TOutput>(predicate), defaultValue, yieldOrigin);

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public TNode? FirstOrDefault(NavigationDirection direction, TNode? defaultValue = null, bool yieldOrigin = true)
        => FirstOrDefaultCore(direction, default(AlwaysTruePredicate<TNode, TInput, TOutput>), defaultValue, yieldOrigin);

    [DoesNotReturn]
    private void ThrowInvalidTraverseResult(Traverse traverse) =>
        throw new IndexOutOfRangeException($"Invalid Traverse Option '{traverse}'");
}
