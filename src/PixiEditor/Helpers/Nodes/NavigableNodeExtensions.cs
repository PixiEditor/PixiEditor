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
    }
    
    extension(INodeHandler node)
    {
        [Obsolete(ObsoleteMessage)]
        public void TraverseBackwards(Func<INodeHandler, Traverse> func) =>
            node.Navigate().TraverseBackwards(ctx => func(ctx.Current));

        [Obsolete(ObsoleteMessage)]
        public void TraverseBackwards(Func<INodeHandler, INodeHandler?, Traverse> func) =>
            node.Navigate().TraverseBackwards(ctx => func(ctx.Current, ctx.Adjacent));

        [Obsolete(ObsoleteMessage)]
        public void TraverseBackwards(Func<INodeHandler, INodeHandler?, INodePropertyHandler?, Traverse> func) =>
            node.Navigate().TraverseBackwards(ctx => func(ctx.Current, ctx.Adjacent, ctx.InputProperty));

        [Obsolete(ObsoleteMessage)]
        public void TraverseForwards(Func<INodeHandler, Traverse> func) =>
            node.Navigate().TraverseForwards(ctx => func(ctx.Current));

        [Obsolete(ObsoleteMessage)]
        public void TraverseForwards(Func<INodeHandler, INodeHandler?, Traverse> func) =>
            node.Navigate().TraverseForwards(ctx => func(ctx.Current, ctx.Adjacent));

        [Obsolete(ObsoleteMessage)]
        public void TraverseForwards(Func<INodeHandler, INodeHandler?, INodePropertyHandler?, Traverse> func) =>
            node.Navigate().TraverseForwards(ctx => func(ctx.Current, ctx.Adjacent, ctx.OutputProperty));

        [Obsolete(ObsoleteMessage)]
        public void TraverseForwards(Func<INodeHandler, INodeHandler?, INodePropertyHandler?, INodePropertyHandler?, Traverse> func) =>
            node.Navigate().TraverseForwards(ctx => func(ctx.Current, ctx.Adjacent, ctx.OutputProperty, ctx.InputProperty));
    }
}
