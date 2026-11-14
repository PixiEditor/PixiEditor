using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Noise")]
public class NoiseNode : RenderNode
{
    private double previousScale = double.NaN;
    private double previousSeed = double.NaN;
    private NoiseType previousNoiseType = Nodes.NoiseType.FractalPerlin;
    private int previousOctaves = -1;
    private VecD previousOffset = new VecD(0d, 0d);
    private VoronoiFeature previousVoronoiFeature = Nodes.VoronoiFeature.F1;
    private double previousRandomness = double.NaN;
    private double previousAngleOffset = double.NaN;

    private Shader voronoiShader;

    private Paint paint = new();

    private static readonly ColorFilter grayscaleFilter = ColorFilter.CreateColorMatrix(
        ColorMatrix.MapAlphaToRedGreenBlue + ColorMatrix.OpaqueAlphaOffset);

    public InputProperty<NoiseType> NoiseType { get; }

    public InputProperty<VecD> Offset { get; }

    public InputProperty<double> Scale { get; }

    public InputProperty<int> Octaves { get; }

    public InputProperty<double> Seed { get; }

    public InputProperty<VoronoiFeature> VoronoiFeature { get; }

    public InputProperty<double> Randomness { get; }

    public InputProperty<double> AngleOffset { get; }

    public NoiseNode()
    {
        NoiseType = CreateInput(nameof(NoiseType), "NOISE_TYPE", Nodes.NoiseType.FractalPerlin);

        Offset = CreateInput(nameof(Offset), "OFFSET", new VecD(0d, 0d));

        Scale = CreateInput(nameof(Scale), "SCALE", 10d).WithRules(v => v.Min(0.1));
        Octaves = CreateInput(nameof(Octaves), "OCTAVES", 1)
            .WithRules(validator => validator.Min(1));

        Seed = CreateInput(nameof(Seed), "SEED", 0d);

        VoronoiFeature = CreateInput(nameof(VoronoiFeature), "VORONOI_FEATURE", Nodes.VoronoiFeature.F1);

        Randomness = CreateInput(nameof(Randomness), "RANDOMNESS", 1d)
            .WithRules(v => v.Min(0d).Max(1d));

        AngleOffset = CreateInput(nameof(AngleOffset), "ANGLE_OFFSET", 0d);
    }

    protected override void OnPaint(RenderContext context, Canvas target)
    {
        if (Math.Abs(previousScale - Scale.Value) > 0.000001
            || previousSeed != Seed.Value
            || previousOctaves != Octaves.Value
            || previousNoiseType != NoiseType.Value
            || previousOffset != Offset.Value
            || previousVoronoiFeature != VoronoiFeature.Value
            || Math.Abs(previousRandomness - Randomness.Value) > 0.000001
            || Math.Abs(previousAngleOffset - AngleOffset.Value) > 0.000001
            || double.IsNaN(previousScale))
        {
            if (Scale.Value < 0.000001)
            {
                return;
            }

            var shader = SelectShader();
            if (shader == null)
            {
                return;
            }

            if (paint.Shader != voronoiShader)
            {
                paint?.Shader?.Dispose();
            }

            paint.Shader = shader;

            // Define a grayscale color filter to apply to the image
            paint.ColorFilter = grayscaleFilter;

            previousScale = Scale.Value;
            previousSeed = Seed.Value;
            previousOctaves = Octaves.Value;
            previousNoiseType = NoiseType.Value;
            previousVoronoiFeature = VoronoiFeature.Value;
            previousRandomness = Randomness.Value;
            previousAngleOffset = AngleOffset.Value;
        }

        RenderNoise(target);
    }

    private void RenderNoise(Canvas workingSurface)
    {
        int saved = workingSurface.Save();
        workingSurface.Translate(-(float)Offset.Value.X, -(float)Offset.Value.Y);
        workingSurface.DrawPaint(paint);
        workingSurface.RestoreToCount(saved);
    }

    public override void RenderPreview(DrawingSurface renderOn, RenderContext context, string elementToRenderName)
    {
        var shader = SelectShader();
        if (shader == null)
        {
            return;
        }

        if (paint.Shader != voronoiShader)
        {
            paint?.Shader?.Dispose();
        }

        paint.Shader = shader;
        paint.ColorFilter = grayscaleFilter;

        RenderNoise(renderOn.Canvas);
    }

    private Shader? SelectShader()
    {
        var freq = (float)(1d / Scale.Value);
        freq = Math.Max(freq, 0.000001f);

        int octaves = Math.Max(1, Octaves.Value);
        Shader shader = NoiseType.Value switch
        {
            Nodes.NoiseType.TurbulencePerlin => Shader.CreatePerlinNoiseTurbulence(
                freq, freq, octaves, (float)Seed.Value),
            Nodes.NoiseType.FractalPerlin => Shader.CreatePerlinFractalNoise(
                freq, freq,
                octaves, (float)Seed.Value),
            Nodes.NoiseType.Voronoi => GetVoronoiShader(freq, octaves, (float)Seed.Value,
                (int)VoronoiFeature.Value, (float)Randomness.Value, (float)AngleOffset.Value),
            _ => null
        };

        return shader;
    }

    private Shader GetVoronoiShader(float frequency, int octaves, float seed, int feature, float randomness,
        float angleOffset)
    {
        if (voronoiShader == null)
        {
            voronoiShader = Shader.CreateVoronoi(frequency, octaves, seed, feature, randomness, angleOffset);
        }
        else
        {
            Uniforms uniforms = new Uniforms();
            uniforms.Add("iSeed", new Uniform("iSeed", seed));
            uniforms.Add("iFrequency", new Uniform("iFrequency", frequency));
            uniforms.Add("iOctaves", new Uniform("iOctaves", octaves));
            uniforms.Add("iRandomness", new Uniform("iRandomness", randomness));
            uniforms.Add("iFeature", new Uniform("iFeature", feature));
            uniforms.Add("iAngleOffset", new Uniform("iAngleOffset", angleOffset));
            voronoiShader = voronoiShader.WithUpdatedUniforms(uniforms);
        }

        return voronoiShader;
    }

    public override Node CreateCopy() => new NoiseNode();
}

public enum NoiseType
{
    TurbulencePerlin,
    FractalPerlin,
    Voronoi
}

public enum VoronoiFeature
{
    F1 = 0, // Distance to the closest feature point
    F2 = 1, // Distance to the second-closest feature point
    F2MinusF1 = 2
}
