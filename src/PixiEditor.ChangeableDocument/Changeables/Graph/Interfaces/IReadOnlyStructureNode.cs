using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IReadOnlyStructureNode : IReadOnlyNode
{
    public InputProperty<float> Opacity { get; }
    public InputProperty<bool> IsVisible { get; }
    public InputProperty<bool> ClipToMemberBelow { get; }
    public InputProperty<BlendMode> BlendMode { get; }
    public InputProperty<ChunkyImage?> Mask { get; }
    public InputProperty<bool> MaskIsVisible { get; }
    public string MemberName { get; set; }
    public RectI? GetTightBounds(KeyFrameTime frameTime);
}
