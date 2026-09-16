using PixiEditor.Models.Handlers;

// ReSharper disable once CheckNamespace
namespace PixiEditor.GraphNavigation;

internal static class NavigableNodeExtensions
{
    private const string ObsoleteMessage = "Use Navigate().Traverse...() with TraverseContext instead.";

    extension(INodeHandler node)
    {
        public NodeNavigator<INodeHandler, INodePropertyHandler, INodePropertyHandler> Navigate() =>
            new(node);
        
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
