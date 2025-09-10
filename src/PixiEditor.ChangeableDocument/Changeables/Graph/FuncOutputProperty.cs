using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using Drawie.Backend.Core.Shaders;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class FuncOutputProperty<T, TContext> : OutputProperty<Func<TContext, T>> where TContext : FuncContext
{
    internal FuncOutputProperty(Node node, string internalName, string displayName, Func<TContext, T> defaultValue) : base(node, internalName, displayName, defaultValue)
    {
        
    }
}
