
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

        [Obsolete(ObsoleteMessage)]
        public void TraverseBackwards(Func<IReadOnlyNode, bool> action) =>
            node.Navigate().TraverseBackwards(ctx => action(ctx.Current).ToTraverseResult());

        [Obsolete(ObsoleteMessage)]
        public void TraverseBackwards(
            Func<IReadOnlyNode, IInputProperty?, bool> action,
            Func<IInputProperty, bool>? branchCondition = null) =>
            node.Navigate().TraverseBackwards(ctx =>
            {
                if (ctx.InputProperty != null && branchCondition != null && !branchCondition(ctx.InputProperty))
                {
                    return Traverse.NoFurther;
                }

                return action(ctx.Current, ctx.InputProperty).ToTraverseResult();
            });

        [Obsolete(ObsoleteMessage)]
        public void TraverseBackwards(Func<IReadOnlyNode, IReadOnlyNode?, IInputProperty?, bool> action) =>
            node.Navigate().TraverseBackwards(ctx => action(ctx.Current, ctx.Adjacent, ctx.InputProperty).ToTraverseResult());

        [Obsolete(ObsoleteMessage)]
        public void TraverseForwards(Func<IReadOnlyNode, bool> action) =>
            node.Navigate().TraverseForwards(ctx => action(ctx.Current).ToTraverseResult());

        [Obsolete(ObsoleteMessage)]
        public void TraverseForwards(Func<IReadOnlyNode, IInputProperty?, bool> action) =>
            node.Navigate().TraverseForwards(ctx => action(ctx.Current, ctx.InputProperty).ToTraverseResult());

        [Obsolete(ObsoleteMessage)]
        public void TraverseForwards(Func<IReadOnlyNode, IInputProperty?, IOutputProperty?, bool> action) =>
            node.Navigate().TraverseForwards(ctx => action(ctx.Current, ctx.InputProperty, ctx.OutputProperty).ToTraverseResult());
    }

    /// <summary>
    /// Maps legacy boolean traversal action results to the <see cref="Traverse"/> control flow enum.
    /// </summary>
    /// <param name="continueTraversal">
    /// <see langword="true"/> to continue exploring the graph (<see cref="Traverse.Further"/>); 
    /// <see langword="false"/> to abort traversal entirely (<see cref="Traverse.Exit"/>).
    /// </param>
    /// <returns>
    /// <see cref="Traverse.Further"/> if <paramref name="continueTraversal"/> is <see langword="true"/>; 
    /// otherwise, <see cref="Traverse.Exit"/>.
    /// </returns>
    private static Traverse ToTraverseResult(this bool continueTraversal) =>
        continueTraversal ? Traverse.Further : Traverse.Exit;
}
