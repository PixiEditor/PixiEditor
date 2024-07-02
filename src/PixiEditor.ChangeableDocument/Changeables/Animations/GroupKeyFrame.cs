using System.Collections.Immutable;
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

    IReadOnlyList<IReadOnlyKeyFrame> IKeyFrameChildrenContainer.Children => Children;

    public GroupKeyFrame(IReadOnlyLayer layer, int startFrame, Document document) : base(layer, startFrame)
    {
        Id = layer.GuidValue;
        this.document = document;
    }

    public override KeyFrame Clone()
    {
        var clone = new GroupKeyFrame(TargetLayer, StartFrame, document) { Id = this.Id };
        foreach (var child in Children)
        {
            clone.Children.Add(child.Clone());
        }

        return clone;
    }
}
