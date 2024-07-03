using SkiaSharp;

namespace Nodes.Nodes;

public class MergeNode : Node
{
    public InputProperty<SKSurface?> Top { get; }
    public InputProperty<SKSurface?> Bottom { get; }
    public OutputProperty<SKSurface> Output { get; }
    
    public MergeNode(string name) : base(name)
    {
        Top = CreateInput<SKSurface>("Top", null);
        Bottom = CreateInput<SKSurface>("Bottom", null);
        Output = CreateOutput<SKSurface>("Output", null);
    }
    
    public override bool Validate()
    {
        return Top.Value != null || Bottom.Value != null;
    }

    public override void OnExecute(int frame)
    {
        SKImage topSnapshot = Top.Value?.Snapshot();
        SKImage bottomSnapshot = Bottom.Value?.Snapshot();
        using SKPaint paint = new SKPaint() { BlendMode = SKBlendMode.SrcOver };
        
        SKSizeI size = new SKSizeI(topSnapshot?.Width ?? bottomSnapshot.Width, topSnapshot?.Height ?? bottomSnapshot.Height);
        
        Output.Value = SKSurface.Create(new SKImageInfo(size.Width, size.Height));
        using SKCanvas canvas = Output.Value.Canvas;
        
        if (bottomSnapshot != null)
        {
            canvas.DrawImage(bottomSnapshot, 0, 0, paint);
        }
        
        if (topSnapshot != null)
        {
            canvas.DrawImage(topSnapshot, 0, 0, paint);
        }
    }
}
