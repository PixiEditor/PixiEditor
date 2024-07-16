using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IReadOnlyNode
{
    public Guid Id { get; }
    public IReadOnlyCollection<IInputProperty> InputProperties { get; }
    public IReadOnlyCollection<IOutputProperty> OutputProperties { get; }
    public VecD Position { get; }
    public Surface? CachedResult { get; }
    
    public string InternalName { get; }

    public Surface? Execute(RenderingContext context);
    public bool Validate();
    
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
}
