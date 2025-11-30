using PixiEditor.ChangeableDocument.Changeables.Animations;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Rendering;
using BlendMode = PixiEditor.ChangeableDocument.Enums.BlendMode;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IReadOnlyStructureNode : IReadOnlyNode, ISceneObject
{
    public InputProperty<float> Opacity { get; }
    public InputProperty<bool> IsVisible { get; }
    public bool ClipToPreviousMember { get; }
    public InputProperty<BlendMode> BlendMode { get; }
    public RenderInputProperty CustomMask { get; }
    public InputProperty<bool> MaskIsVisible { get; }
    public string MemberName { get; set; }
    public RectD? GetTightBounds(KeyFrameTime frameTime);
    public ChunkyImage? EmbeddedMask { get; }
    public ShapeCorners GetTransformationCorners(KeyFrameTime frameTime);
    public void RenderForOutput(RenderContext context, Canvas renderTarget, RenderOutputProperty output);
}
