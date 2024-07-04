using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public abstract class StructureNode : Node, IReadOnlyStructureNode
{
    public InputProperty<ChunkyImage?> Background { get; }
    public InputProperty<float> Opacity { get; } 
    public InputProperty<bool> IsVisible { get; }
    public InputProperty<bool> ClipToMemberBelow { get; }
    public InputProperty<BlendMode> BlendMode { get; } 
    public InputProperty<ChunkyImage?> Mask { get; }
    public InputProperty<bool> MaskIsVisible { get; }
    
    public OutputProperty<ChunkyImage?> Output { get; }
    
    public string LayerName { get; set; } = string.Empty;

    protected StructureNode(Guid? id = null) : base(id)
    {
        Background = CreateInput<ChunkyImage?>("Background", null);
        Opacity = CreateInput<float>("Opacity", 1);
        IsVisible = CreateInput<bool>("IsVisible", true);
        ClipToMemberBelow = CreateInput<bool>("ClipToMemberBelow", false);
        BlendMode = CreateInput<BlendMode>("BlendMode", Enums.BlendMode.Normal);
        Mask = CreateInput<ChunkyImage?>("Mask", null);
        MaskIsVisible = CreateInput<bool>("MaskIsVisible", true);
        
        Output = CreateOutput<ChunkyImage?>("Output", null);
    }
    
    public abstract override ChunkyImage? OnExecute(KeyFrameTime frameTime);
    public abstract override bool Validate();

    public abstract RectI? GetTightBounds(KeyFrameTime frameTime);
}
