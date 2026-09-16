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
    public IEnumerable<TraverseContext<TNode, TInput, TOutput>> Backwards(bool yieldOrigin = true) =>
        Backwards(null, yieldOrigin);

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

    public IEnumerable<TraverseContext<TNode, TInput, TOutput>> Forwards() => Forwards(null);

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

    [DoesNotReturn]
    private void ThrowInvalidTraverseResult(Traverse traverse) =>
        throw new IndexOutOfRangeException($"Invalid Traverse Option '{traverse}'");
}
