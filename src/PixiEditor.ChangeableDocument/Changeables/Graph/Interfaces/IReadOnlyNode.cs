using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IReadOnlyNode
{
    public Guid Id { get; }
    public IReadOnlyList<IInputProperty> InputProperties { get; }
    public IReadOnlyList<IOutputProperty> OutputProperties { get; }
    public VecD Position { get; }
    public Surface? CachedResult { get; }
    string DisplayName { get; }

    public Surface? Execute(RenderingContext context);
    
    /// <summary>
    ///     Checks if the inputs are legal. If they are not, the node should not be executed.
    /// Note that all nodes connected to any output of this node won't be executed either.
    /// </summary>
    /// <example>Divide node has two inputs, if the second input is 0, the node should not be executed. Since division by 0 is illegal</example>
    /// <returns>True if the inputs are legal, false otherwise.</returns>
    
    /// <summary>
    ///     Traverses the graph backwards from this node. Backwards means towards the input nodes.
    /// </summary>
    /// <param name="action">The action to perform on each node.</param>
    public void TraverseBackwards(Func<IReadOnlyNode, bool> action);

    /// <summary>
    ///     Traverses the graph forwards from this node. Forwards means towards the output nodes.
    /// </summary>
    /// <param name="action">The action to perform on each node.</param>
    public void TraverseForwards(Func<IReadOnlyNode, bool> action);
    
    public IInputProperty? GetInputProperty(string internalName);
    public IOutputProperty? GetOutputProperty(string internalName);
    public void SerializeAdditionalData(Dictionary<string,object> additionalData);
    public string GetNodeTypeUniqueName();
}
