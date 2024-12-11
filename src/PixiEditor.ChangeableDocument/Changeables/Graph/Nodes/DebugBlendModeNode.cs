using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using DrawingApiBlendMode = Drawie.Backend.Core.Surfaces.BlendMode;
using Drawie.Numerics;

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

    private Paint blendModeOpacityPaint => new() { BlendMode = DrawingApiBlendMode.SrcOver }; 
    public DebugBlendModeNode()
    {
        Dst = CreateInput<Texture?>(nameof(Dst), "Dst", null);
        Src = CreateInput<Texture?>(nameof(Src), "Src", null);
        BlendMode = CreateInput(nameof(BlendMode), "Blend Mode", DrawingApiBlendMode.SrcOver);

        Result = CreateOutput<Texture>(nameof(Result), "Result", null);
    }

    protected override void OnExecute(RenderContext context)
    {
        if (Dst.Value is not { } dst || Src.Value is not { } src)
            return;

        var size = new VecI(Math.Max(src.Size.X, dst.Size.X), int.Max(src.Size.Y, dst.Size.Y));
        var workingSurface = RequestTexture(0, size, context.ProcessingColorSpace);

        workingSurface.DrawingSurface.Canvas.DrawSurface(dst.DrawingSurface, 0, 0, blendModeOpacityPaint);

        _paint.BlendMode = BlendMode.Value;
        workingSurface.DrawingSurface.Canvas.DrawSurface(src.DrawingSurface, 0, 0, _paint);
        
        Result.Value = workingSurface;
    }


    public override Node CreateCopy() => new DebugBlendModeNode();

    public override void Dispose()
    {
        base.Dispose();
        _paint.Dispose();
    }
}
