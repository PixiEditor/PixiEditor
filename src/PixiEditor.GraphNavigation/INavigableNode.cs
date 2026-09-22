namespace PixiEditor.GraphNavigation;

public interface INavigableNode<TNode, TInput, TOutput>
    where TNode : class, INavigableNode<TNode, TInput, TOutput>
    where TInput : class, INavigableInputProperty<TNode, TInput, TOutput>
    where TOutput : class, INavigableOutputProperty<TNode, TInput, TOutput>
{
    IEnumerable<TInput> Inputs { get; }
    IEnumerable<TOutput> Outputs { get; }
}
