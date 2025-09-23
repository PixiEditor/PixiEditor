using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Numerics;
using PixiEditor.Common;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IReadOnlyNode : ICacheable
{
    public Guid Id { get; }
    public IReadOnlyInputProperties InputProperties { get; }
    public IReadOnlyOutputProperties OutputProperties { get; }
    public IReadOnlyList<IReadOnlyKeyFrameData> KeyFrames { get; }
    public VecD Position { get; }
    string DisplayName { get; }

    public void Execute(RenderContext context);

    /// <summary>
    ///     Traverses the graph backwards from this node. Backwards means towards the input nodes.
    /// </summary>
    /// <param name="action">The action to perform on each node.</param>
    public void TraverseBackwards(Func<IReadOnlyNode, bool> action);

    /// <summary>
    ///     Traverses the graph backwards from this node. Backwards means towards the input nodes.
    /// </summary>
    /// <param name="action">The action to perform on each node. Input property is the input that was used to traverse this node.</param>
    public void TraverseBackwards(Func<IReadOnlyNode, IInputProperty, bool> action, Func<IInputProperty, bool>? branchCondition = null);

    /// <summary>
    ///     Traverses the graph forwards from this node. Forwards means towards the output nodes.
    /// </summary>
    /// <param name="action">The action to perform on each node.</param>
    public void TraverseForwards(Func<IReadOnlyNode, bool> action);
    
     /// <summary>
    ///     Traverses the graph forwards from this node. Forwards means towards the output nodes.
    /// </summary>
    /// <param name="action">The action to perform on each node. Input property is the input that was used to traverse this node.</param>
    public void TraverseForwards(Func<IReadOnlyNode, IInputProperty, bool> action);

    public IInputProperty? GetInputProperty(string internalName);
    public IOutputProperty? GetOutputProperty(string internalName);
    public void SerializeAdditionalData(Dictionary<string, object> additionalData);
    public string GetNodeTypeUniqueName();
}
