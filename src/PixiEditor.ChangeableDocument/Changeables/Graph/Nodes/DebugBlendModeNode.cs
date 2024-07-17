using PixiEditor.ChangeableDocument.Rendering;
using DrawingApiBlendMode = PixiEditor.DrawingApi.Core.Surface.BlendMode;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

// TODO: Add based on debug mode, not debug build.
#if DEBUG
public class DebugBlendModeNode : Node
{
    private Paint _paint = new();
    
    public InputProperty<Surface?> Dst { get; }

    public InputProperty<Surface?> Src { get; }

    public InputProperty<DrawingApiBlendMode> BlendMode { get; }

    public OutputProperty<Surface> Result { get; }

    public DebugBlendModeNode()
    {
        Dst = CreateInput<Surface?>(nameof(Dst), "Dst", null);
        Src = CreateInput<Surface?>(nameof(Src), "Src", null);
        BlendMode = CreateInput(nameof(BlendMode), "Blend Mode", DrawingApiBlendMode.SrcOver);

        Result = CreateOutput<Surface>(nameof(Result), "Result", null);
    }
    
    protected override Surface? OnExecute(RenderingContext context)
    {
        if (Dst.Value is not { } dst || Src.Value is not { } src)
            return null;

        var size = new VecI(Math.Max(src.Size.X, dst.Size.X), int.Max(src.Size.Y, dst.Size.Y));
        var workingSurface = new Surface(size);

        workingSurface.DrawingSurface.Canvas.DrawSurface(dst.DrawingSurface, 0, 0, context.BlendModeOpacityPaint);

        _paint.BlendMode = BlendMode.Value;
        workingSurface.DrawingSurface.Canvas.DrawSurface(src.DrawingSurface, 0, 0, _paint);
        
        Result.Value = workingSurface;

        return workingSurface;
    }

    public override bool Validate() => true;

    public override Node CreateCopy() => new DebugBlendModeNode();
}
#endif
