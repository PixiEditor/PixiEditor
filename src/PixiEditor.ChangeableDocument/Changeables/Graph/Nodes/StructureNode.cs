using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public abstract class StructureNode : Node, IReadOnlyStructureNode, IBackgroundInput
{
    public InputProperty<Surface?> Background { get; }
    public InputProperty<float> Opacity { get; } 
    public InputProperty<bool> IsVisible { get; }
    public InputProperty<bool> ClipToPreviousMember { get; }
    public InputProperty<BlendMode> BlendMode { get; } 
    public InputProperty<ChunkyImage?> Mask { get; }
    public InputProperty<bool> MaskIsVisible { get; }
    
    public OutputProperty<Surface?> Output { get; }
    
    public string MemberName { get; set; } = string.Empty;

    protected StructureNode()
    {
        Background = CreateInput<Surface?>("Background", "BACKGROUND", null);
        Opacity = CreateInput<float>("Opacity", "OPACITY", 1);
        IsVisible = CreateInput<bool>("IsVisible", "IS_VISIBLE", true);
        ClipToPreviousMember = CreateInput<bool>("ClipToMemberBelow", "CLIP_TO_MEMBER_BELOW", false);
        BlendMode = CreateInput<BlendMode>("BlendMode", "BLEND_MODE", Enums.BlendMode.Normal);
        Mask = CreateInput<ChunkyImage?>("Mask", "MASK", null);
        MaskIsVisible = CreateInput<bool>("MaskIsVisible", "MASK_IS_VISIBLE", true);
        
        Output = CreateOutput<Surface?>("Output", "OUTPUT", null);
    }

    protected abstract override Surface? OnExecute(RenderingContext context);
    public abstract override bool Validate();

    public abstract RectI? GetTightBounds(KeyFrameTime frameTime);
}
