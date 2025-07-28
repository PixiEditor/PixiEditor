using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using Drawie.Backend.Core.Shaders.Generation;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

internal class NodeVariableAttachments
{
    private readonly List<(INodeProperty key, ShaderExpressionVariable variable)> _variables = new();
    
    public FuncContext LastContext { get; private set; }

    /// <summary>
    /// Tries getting a shader variable associated to the property. Otherwise, it creates a new one from the variable factory
    /// </summary>
    /// <typeparam name="T">The shader variable type. i.e. Half4, Float1</typeparam>
    /// <returns>The attached or new variable</returns>
    public T GetOrAttachNew<T>(FuncContext context, INodeProperty property, Func<T> variable) where T : ShaderExpressionVariable
    {
        if (LastContext != context)
        {
            LastContext = context;
            _variables.Clear();
            
            return AttachNew(property, variable);
        }

        var existing = _variables.FirstOrDefault(x => x.key == property);

        if (existing == default)
        {
            existing.variable = AttachNew(property, variable);
        }

        return (T)existing.variable;
    }

    private T AttachNew<T>(INodeProperty property, Func<T> variable) where T : ShaderExpressionVariable
    {
        var newVariable = variable();
        
        _variables.Add((property, newVariable));

        return newVariable;
    }
}
