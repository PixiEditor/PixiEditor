using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Noise", "NOISE_NODE")]
public class NoiseNode : Node
{
    private double previousScale = double.NaN;
    private double previousSeed = double.NaN;
    private NoiseType previousNoiseType = Nodes.NoiseType.TurbulencePerlin;
    private int previousOctaves = -1;
    
    private Paint paint = new();
    
    private static readonly ColorFilter grayscaleFilter = ColorFilter.CreateColorMatrix(
        ColorMatrix.MapAlphaToRedGreenBlue + ColorMatrix.OpaqueAlphaOffset);
    
    public OutputProperty<Surface> Noise { get; }
    
    public InputProperty<NoiseType> NoiseType { get; }
    public InputProperty<VecI> Size { get; }
    
    public InputProperty<double> Scale { get; }
    
    public InputProperty<int> Octaves { get; }
    
    public InputProperty<double> Seed { get; }

    public NoiseNode()
    {
        Noise = CreateOutput<Surface>(nameof(Noise), "NOISE", null);
        NoiseType = CreateInput(nameof(NoiseType), "NOISE_TYPE", Nodes.NoiseType.TurbulencePerlin);
        Size = CreateInput(nameof(Size), "SIZE", new VecI(64, 64));
        Scale = CreateInput(nameof(Scale), "SCALE", 10d);
        Octaves = CreateInput(nameof(Octaves), "OCTAVES", 1);
        Seed = CreateInput(nameof(Seed), "SEED", 0d);
    }

    protected override Surface OnExecute(RenderingContext context)
    {
        if (Math.Abs(previousScale - Scale.Value) > 0.000001
            || previousSeed != Seed.Value
            || previousOctaves != Octaves.Value
            || previousNoiseType != NoiseType.Value
            || double.IsNaN(previousScale))
        {
            if(Scale.Value < 0.000001)
            {
                Noise.Value = null;
                return null;
            }
            
            var shader = SelectShader();
            if (shader == null)
            {
                Noise.Value = null;
                return null;
            }
            
            paint.Shader = shader;
            
            // Define a grayscale color filter to apply to the image
            paint.ColorFilter = grayscaleFilter; 
            
            previousScale = Scale.Value;
            previousSeed = Seed.Value;
            previousOctaves = Octaves.Value;
            previousNoiseType = NoiseType.Value;
        }
        
        var size = Size.Value;
        
        if (size.X < 1 || size.Y < 1)
        {
            Noise.Value = null;
            return null;
        }
        
        var workingSurface = new Surface(size);
       
        workingSurface.DrawingSurface.Canvas.DrawPaint(paint);

        Noise.Value = workingSurface;
        
        return Noise.Value;
    }

    private Shader SelectShader()
    {
        Shader shader = NoiseType.Value switch
        {
            Nodes.NoiseType.TurbulencePerlin => Shader.CreatePerlinNoiseTurbulence(
                (float)(1d / Scale.Value),
                (float)(1d / Scale.Value), Octaves.Value, (float)Seed.Value),
            Nodes.NoiseType.FractalPerlin => Shader.CreatePerlinFractalNoise(
                (float)(1d / Scale.Value),
                (float)(1d / Scale.Value), Octaves.Value, (float)Seed.Value),
            _ => null
        };

        return shader;
    }

    public override Node CreateCopy() => new NoiseNode();
}

public enum NoiseType
{
    TurbulencePerlin,
    FractalPerlin
}
