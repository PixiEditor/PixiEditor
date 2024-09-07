using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IReadOnlyStructureNode : IReadOnlyNode
{
    public InputProperty<float> Opacity { get; }
    public InputProperty<bool> IsVisible { get; }
    public bool ClipToPreviousMember { get; }
    public InputProperty<BlendMode> BlendMode { get; }
    public InputProperty<Texture?> CustomMask { get; }
    public InputProperty<bool> MaskIsVisible { get; }
    public string MemberName { get; set; }
    public RectD? GetTightBounds(KeyFrameTime frameTime);
    public ChunkyImage? EmbeddedMask { get; }
    public ShapeCorners GetTransformationCorners(KeyFrameTime frameTime);
}
