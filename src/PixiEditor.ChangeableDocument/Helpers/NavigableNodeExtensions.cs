using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

// ReSharper disable once CheckNamespace
namespace PixiEditor.GraphNavigation;

internal static class BackendNodeNavigationExtensions
{
    private const string ObsoleteMessage = "Use Navigate().Traverse...() with TraverseContext instead.";

    extension(IReadOnlyNode node)
    {
        public NodeNavigator<IReadOnlyNode, IInputProperty, IOutputProperty> Navigate() =>
            new(node);
        
        
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
    /// Maps legacy boolean traversal action results to the <see cref="Traverse"/> control flow enum.
    /// </summary>
    /// <param name="continueTraversal">
    /// <see langword="true"/> to continue exploring the graph (<see cref="Traverse.Continue"/>); 
    /// <see langword="false"/> to abort traversal entirely (<see cref="Traverse.Exit"/>).
    /// </param>
    /// <returns>
    /// <see cref="Traverse.Continue"/> if <paramref name="continueTraversal"/> is <see langword="true"/>; 
    /// otherwise, <see cref="Traverse.Exit"/>.
    /// </returns>
    private static Traverse ToTraverseResult(this bool continueTraversal) =>
        continueTraversal ? Traverse.Continue : Traverse.ExitInclusive;
}
