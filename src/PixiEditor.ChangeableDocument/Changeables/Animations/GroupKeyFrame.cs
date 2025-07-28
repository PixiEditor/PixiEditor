using System.Collections.Immutable;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Animations;

internal class GroupKeyFrame : KeyFrame, IKeyFrameChildrenContainer
{
    private ChunkyImage originalLayerImage;
    private Document document;
    private bool isVisible = true;
    public List<KeyFrame> Children { get; } = new List<KeyFrame>();
    public override int Duration => Children.Count > 0 ? Children.Max(x => x.StartFrame + x.Duration) - StartFrame : 0;
    public override int StartFrame => Children.Count > 0 ? Children.Min(x => x.StartFrame) : 0;

    public override bool IsVisible
    {
        get
        {
            return isVisible;
        }
        set
        {
            isVisible = value;
            foreach (var child in Children)
            {
                child.IsVisible = value;
            }
        }
    }

    IReadOnlyList<IReadOnlyKeyFrame> IKeyFrameChildrenContainer.Children => Children;

    public GroupKeyFrame(Guid targetNodeId, int startFrame, Document document) : base(targetNodeId, startFrame)
    {
        Id = targetNodeId; 
        this.document = document;
    }

    public override KeyFrame Clone()
    {
        var clone = new GroupKeyFrame(NodeId, StartFrame, document) { Id = this.Id, IsVisible = IsVisible };
        foreach (var child in Children)
        {
            clone.Children.Add(child.Clone());
        }

        return clone;
    }
}
