namespace PixiEditor.GraphNavigation;

public interface INavigableNodeReference<TNode, TInput, TOutput>
    where TNode : class, INavigableNode<TNode, TInput, TOutput>
    where TInput : class, INavigableInputProperty<TNode, TInput, TOutput>
    where TOutput : class, INavigableOutputProperty<TNode, TInput, TOutput>
{
    TNode Node { get; }
}

public interface INavigableInputProperty<TNode, TInput, TOutput> : INavigableNodeReference<TNode, TInput, TOutput>
    where TNode : class, INavigableNode<TNode, TInput, TOutput>
    where TInput : class, INavigableInputProperty<TNode, TInput, TOutput>
    where TOutput : class, INavigableOutputProperty<TNode, TInput, TOutput>
{
    TOutput? ConnectedOutput { get; }
}

public interface INavigableOutputProperty<TNode, TInput, TOutput> : INavigableNodeReference<TNode, TInput, TOutput>
    where TNode : class, INavigableNode<TNode, TInput, TOutput>
    where TInput : class, INavigableInputProperty<TNode, TInput, TOutput>
    where TOutput : class, INavigableOutputProperty<TNode, TInput, TOutput>
{
    IEnumerable<TInput> ConnectedInputs { get; }
}
