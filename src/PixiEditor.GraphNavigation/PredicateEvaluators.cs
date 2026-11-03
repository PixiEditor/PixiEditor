using System.Runtime.CompilerServices;

namespace PixiEditor.GraphNavigation;

internal interface IPredicateEvaluator<TNode, TInput, TOutput>
    where TNode : class, INavigableNode<TNode, TInput, TOutput>
    where TInput : class, INavigableInputProperty<TNode, TInput, TOutput>
    where TOutput : class, INavigableOutputProperty<TNode, TInput, TOutput>
{
    public bool Evaluate(in TraverseContext<TNode, TInput, TOutput> context);
}

internal readonly struct NodePredicate<TNode, TInput, TOutput>(Func<TNode, bool> predicate)
    : IPredicateEvaluator<TNode, TInput, TOutput>
    where TNode : class, INavigableNode<TNode, TInput, TOutput>
    where TInput : class, INavigableInputProperty<TNode, TInput, TOutput>
    where TOutput : class, INavigableOutputProperty<TNode, TInput, TOutput>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Evaluate(in TraverseContext<TNode, TInput, TOutput> context) => predicate(context.Current);
}

internal readonly struct ContextPredicate<TNode, TInput, TOutput>(
    Func<TraverseContext<TNode, TInput, TOutput>, bool> predicate)
    : IPredicateEvaluator<TNode, TInput, TOutput>
    where TNode : class, INavigableNode<TNode, TInput, TOutput>
    where TInput : class, INavigableInputProperty<TNode, TInput, TOutput>
    where TOutput : class, INavigableOutputProperty<TNode, TInput, TOutput>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Evaluate(in TraverseContext<TNode, TInput, TOutput> context) => predicate(context);
}

internal readonly struct AlwaysTruePredicate<TNode, TInput, TOutput> : IPredicateEvaluator<TNode, TInput, TOutput>
    where TNode : class, INavigableNode<TNode, TInput, TOutput>
    where TInput : class, INavigableInputProperty<TNode, TInput, TOutput>
    where TOutput : class, INavigableOutputProperty<TNode, TInput, TOutput>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Evaluate(in TraverseContext<TNode, TInput, TOutput> context) => true;
}
