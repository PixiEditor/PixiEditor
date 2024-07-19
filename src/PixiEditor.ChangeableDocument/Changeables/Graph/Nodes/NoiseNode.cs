using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class NoiseNode : Node
{
    private double previousScale = double.NaN;
    private Paint paint = new();
    
    public OutputProperty<Surface> Noise { get; }

    public InputProperty<VecI> Size { get; }
    
    public InputProperty<double> Scale { get; }
    
    public InputProperty<double> Seed { get; }

    public NoiseNode()
    {
        Noise = CreateOutput<Surface>(nameof(Noise), "NOISE", null);
        Size = CreateInput(nameof(Size), "SIZE", new VecI());
        Scale = CreateInput(nameof(Scale), "SCALE", 0d);
        Seed = CreateInput(nameof(Seed), "SEED", 0d);
    }

    protected override string NodeUniqueName => "Noise";

    protected override Surface OnExecute(RenderingContext context)
    {
        if (Math.Abs(previousScale - Scale.Value) > 0.000001 || double.IsNaN(previousScale))
        {
            var shader = Shader.CreatePerlinNoiseTurbulence((float)(1d / Scale.Value), (float)(1d / Scale.Value), 4, (float)Seed.Value);
            paint.Shader = shader;

            previousScale = Scale.Value;
        }
        
        var size = Size.Value;
        
        var workingSurface = new Surface(size);
        
        workingSurface.DrawingSurface.Canvas.DrawPaint(paint);

        Noise.Value = workingSurface;
        
        return Noise.Value;
    }

    public override string DisplayName { get; set; } = "NOISE_NODE";
    public override bool AreInputsLegal() => Size.Value is { X: > 0, Y: > 0 }; 

    public override Node CreateCopy() => new NoiseNode();
}
