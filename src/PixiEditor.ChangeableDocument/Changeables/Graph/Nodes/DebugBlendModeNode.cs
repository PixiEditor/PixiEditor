using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using DrawingApiBlendMode = PixiEditor.DrawingApi.Core.Surfaces.BlendMode;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

// TODO: Add based on debug mode, not debug build.
[NodeInfo("DebugBlendMode")]
public class DebugBlendModeNode : Node
{
    private Paint _paint = new();
    
    public InputProperty<Texture?> Dst { get; }

    public InputProperty<Texture?> Src { get; }

    public InputProperty<DrawingApiBlendMode> BlendMode { get; }

    public OutputProperty<Texture> Result { get; }

    public override string DisplayName { get; set; } = "Debug Blend Mode";
    public DebugBlendModeNode()
    {
        Dst = CreateInput<Texture?>(nameof(Dst), "Dst", null);
        Src = CreateInput<Texture?>(nameof(Src), "Src", null);
        BlendMode = CreateInput(nameof(BlendMode), "Blend Mode", DrawingApiBlendMode.SrcOver);

        Result = CreateOutput<Texture>(nameof(Result), "Result", null);
    }

    protected override Texture? OnExecute(RenderingContext context)
    {
        if (Dst.Value is not { } dst || Src.Value is not { } src)
            return null;

        var size = new VecI(Math.Max(src.Size.X, dst.Size.X), int.Max(src.Size.Y, dst.Size.Y));
        var workingSurface = new Texture(size);

        workingSurface.DrawingSurface.Canvas.DrawSurface(dst.DrawingSurface, 0, 0, context.BlendModeOpacityPaint);

        _paint.BlendMode = BlendMode.Value;
        workingSurface.DrawingSurface.Canvas.DrawSurface(src.DrawingSurface, 0, 0, _paint);
        
        Result.Value = workingSurface;

        return workingSurface;
    }


    public override Node CreateCopy() => new DebugBlendModeNode();
}
