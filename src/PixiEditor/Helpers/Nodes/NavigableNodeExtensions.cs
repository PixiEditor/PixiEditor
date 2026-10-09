using PixiEditor.Models.Handlers;

// ReSharper disable once CheckNamespace
namespace PixiEditor.GraphNavigation;

internal static class NavigableNodeExtensions
{
    private const string ObsoleteMessage = "Use Navigate().Traverse...() with TraverseContext instead.";

    extension(INodeHandler node)
    {
        /// <summary>
        /// Creates a navigator for this node that yields traversal context for each visited step.
        /// </summary>
        /// <returns>A navigator bound to the current node.</returns>
        public NodeNavigator<INodeHandler, INodePropertyHandler, INodePropertyHandler> Navigate() =>
            new(node);

        /// <summary>
        /// Creates a traversal engine for this node when explicit control over the expansion loop is required.
        /// </summary>
        /// <returns>A traversal engine bound to the current node.</returns>
        public NodeTraversalEngine<INodeHandler, INodePropertyHandler, INodePropertyHandler> NavigateViaEngine() =>
            new(node);
    }

    extension(INodeHandler node)
    {
        [Obsolete(ObsoleteMessage)]
        public void TraverseBackwards(Func<INodeHandler, Traverse> func)
        {
            foreach (var _ in node.Navigate().Backwards(ctx => func(ctx.Current))) { }
        }

        [Obsolete(ObsoleteMessage)]
        public void TraverseBackwards(Func<INodeHandler, INodeHandler?, INodePropertyHandler?, Traverse> func)
        {
            foreach (var _ in node.Navigate().Backwards(ctx => func(ctx.Current, ctx.Adjacent, ctx.InputProperty))) { }
        }

        [Obsolete(ObsoleteMessage)]
        public void TraverseForwards(Func<INodeHandler, Traverse> func)
        {
            foreach (var _ in node.Navigate().Forwards(ctx => func(ctx.Current))) { }
        }

        [Obsolete(ObsoleteMessage)]
        public void TraverseForwards(
            Func<INodeHandler, INodeHandler?, INodePropertyHandler?, INodePropertyHandler?, Traverse> func)
        {
            foreach (var _ in node.Navigate()
                         .Forwards(ctx => func(ctx.Current, ctx.Adjacent, ctx.OutputProperty, ctx.InputProperty))) { }
        }
    }
}
