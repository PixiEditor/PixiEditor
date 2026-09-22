using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

// ReSharper disable once CheckNamespace
namespace PixiEditor.GraphNavigation;

internal static class BackendNodeNavigationExtensions
{
    private const string ObsoleteMessage = "Use Navigate().Traverse...() with TraverseContext instead.";

    extension(IReadOnlyNode node)
    {
        /// <summary>
        /// Creates a navigator for this node that yields traversal context for each visited step.
        /// </summary>
        /// <returns>A navigator bound to the current node.</returns>
        public NodeNavigator<IReadOnlyNode, IInputProperty, IOutputProperty> Navigate() =>
            new(node);

        /// <summary>
        /// Creates a traversal engine for this node when explicit control over the expansion loop is required.
        /// </summary>
        /// <returns>A traversal engine bound to the current node.</returns>
        public NodeTraversalEngine<IReadOnlyNode, IInputProperty, IOutputProperty> NavigateViaEngine() =>
            new(node);

        [Obsolete(ObsoleteMessage)]
        public void TraverseBackwards(Func<IReadOnlyNode, bool> action)
        {
            foreach (var _ in node.Navigate().Backwards(ctx => action(ctx.Current).ToTraverseResult())) { }
        }

        [Obsolete(ObsoleteMessage)]
        public void TraverseBackwards(
            Func<IReadOnlyNode, IInputProperty?, bool> action,
            Func<IInputProperty, bool>? branchCondition = null)
        {
            foreach (var _ in node.Navigate().Backwards(ctx =>
                     {
                         if (ctx.InputProperty != null && branchCondition != null &&
                             !branchCondition(ctx.InputProperty))
                         {
                             return Traverse.SkipChildren;
                         }

                         return action(ctx.Current, ctx.InputProperty).ToTraverseResult();
                     })) { }
        }

        [Obsolete(ObsoleteMessage)]
        public void TraverseForwards(Func<IReadOnlyNode, bool> action)
        {
            foreach (var _ in node.Navigate().Forwards(ctx => action(ctx.Current).ToTraverseResult())) { }
        }

        [Obsolete(ObsoleteMessage)]
        public void TraverseForwards(Func<IReadOnlyNode, IInputProperty?, bool> action)
        {
            foreach (var _ in node.Navigate()
                         .Forwards(ctx => action(ctx.Current, ctx.InputProperty).ToTraverseResult())) { }
        }
    }

    /// <summary>
    /// Converts a legacy boolean continuation check into a traversal decision.
    /// </summary>
    /// <param name="continueTraversal">
    /// <see langword="true"/> to continue exploring the graph; otherwise the traversal stops after the current node.
    /// </param>
    /// <returns>
    /// <see cref="Traverse.Continue"/> when <paramref name="continueTraversal"/> is <see langword="true"/>;
    /// otherwise <see cref="Traverse.ExitInclusive"/>.
    /// </returns>
    private static Traverse ToTraverseResult(this bool continueTraversal) =>
        continueTraversal ? Traverse.Continue : Traverse.ExitInclusive;
}
