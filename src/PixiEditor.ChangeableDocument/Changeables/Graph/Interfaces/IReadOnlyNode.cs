using Drawie.Backend.Core;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Numerics;
using PixiEditor.GraphNavigation;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IReadOnlyNode : INavigableNode<IReadOnlyNode, IInputProperty, IOutputProperty>, ICacheable
{
    public Guid Id { get; }
    public IReadOnlyList<IInputProperty> InputProperties { get; }
    public IReadOnlyList<IOutputProperty> OutputProperties { get; }
    public IReadOnlyList<IReadOnlyKeyFrameData> KeyFrames { get; }
    public VecD Position { get; }
    string DisplayName { get; }

    public void Execute(RenderContext context);

    public IInputProperty? GetInputProperty(string internalName);
    public IOutputProperty? GetOutputProperty(string internalName);
    public void SerializeAdditionalData(IReadOnlyDocument target, Dictionary<string, object> additionalData);
    public string GetNodeTypeUniqueName();

    IEnumerable<IInputProperty> INavigableNode<IReadOnlyNode, IInputProperty, IOutputProperty>.Inputs => InputProperties;
    
    IEnumerable<IOutputProperty> INavigableNode<IReadOnlyNode, IInputProperty, IOutputProperty>.Outputs => OutputProperties;
}
