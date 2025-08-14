using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
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

    private Paint paint = new();

    private static readonly ColorFilter grayscaleFilter = ColorFilter.CreateColorMatrix(
        ColorMatrix.MapAlphaToRedGreenBlue + ColorMatrix.OpaqueAlphaOffset);

    public InputProperty<NoiseType> NoiseType { get; }

    public InputProperty<VecD> Offset { get; }
    
    public InputProperty<double> Scale { get; }

    public InputProperty<int> Octaves { get; }

    public InputProperty<double> Seed { get; }
    public InputProperty<double> Randomness { get; }

    public NoiseNode()
    {
        NoiseType = CreateInput(nameof(NoiseType), "NOISE_TYPE", Nodes.NoiseType.FractalPerlin);

        Offset = CreateInput(nameof(Offset), "OFFSET", new VecD(0d, 0d));
        
        Scale = CreateInput(nameof(Scale), "SCALE", 10d).WithRules(v => v.Min(0.1));
        Octaves = CreateInput(nameof(Octaves), "OCTAVES", 1)
            .WithRules(validator => validator.Min(1));

        Seed = CreateInput(nameof(Seed), "SEED", 0d);
        
        Randomness = CreateInput(nameof(Randomness), "RANDOMNESS", 0d)
            .WithRules(v => v.Min(0d).Max(1d));
    }

    protected override void OnPaint(RenderContext context, DrawingSurface target)
    {
        if (Math.Abs(previousScale - Scale.Value) > 0.000001
            || previousSeed != Seed.Value
            || previousOctaves != Octaves.Value
            || previousNoiseType != NoiseType.Value
            || previousOffset != Offset.Value
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

            paint.Shader = shader;

            // Define a grayscale color filter to apply to the image
            paint.ColorFilter = grayscaleFilter;
            
            previousScale = Scale.Value;
            previousSeed = Seed.Value;
            previousOctaves = Octaves.Value;
            previousNoiseType = NoiseType.Value;
        }

        RenderNoise(target);
    }

    private void RenderNoise(DrawingSurface workingSurface)
    {
        int saved = workingSurface.Canvas.Save();
        workingSurface.Canvas.Translate(-(float)Offset.Value.X, -(float)Offset.Value.Y);
        workingSurface.Canvas.DrawPaint(paint);
        workingSurface.Canvas.RestoreToCount(saved);
    }

    public override RectD? GetPreviewBounds(int frame, string elementToRenderName = "")
    {
        return new RectD(0, 0, 128, 128); 
    }

    public override bool RenderPreview(DrawingSurface renderOn, RenderContext context, string elementToRenderName)
    {
        var shader = SelectShader();
        if (shader == null)
        {
            return false;
        }
        
        paint.Shader = shader;
        paint.ColorFilter = grayscaleFilter;
        
        RenderNoise(renderOn);

        return true;
    }

    private Shader SelectShader()
    {
        int octaves = Math.Max(1, Octaves.Value);
        Shader shader = NoiseType.Value switch
        {
            Nodes.NoiseType.TurbulencePerlin => Shader.CreatePerlinNoiseTurbulence(
                (float)(1d / Scale.Value),
                (float)(1d / Scale.Value), octaves, (float)Seed.Value),
            Nodes.NoiseType.FractalPerlin => Shader.CreatePerlinFractalNoise(
                (float)(1d / Scale.Value),
                (float)(1d / Scale.Value),
                octaves, (float)Seed.Value),
            Nodes.NoiseType.Voronoi => CreateVoronoiShader((float)Seed.Value, (float)(1d / (Scale.Value)), octaves, (float)Randomness.Value),
            _ => null
        };

        return shader;
    }

    private Shader CreateVoronoiShader(float seed, float frequency, int octaves, float randomness)
    {
        string voronoiShaderCode = """
                                   uniform float iSeed;
                                   uniform float iFrequency;
                                   uniform int iOctaves;
                                   uniform float iRandomness;
                                   
                                   const int MAX_OCTAVES = 8;
                                   const float LARGE_NUMBER = 1e9;
                                   const float FEATURE_SEED_SCALE = 10.0;
                                   
                                   float hashPoint(float2 p, float seed) {
                                       p = fract(p * float2(0.3183099, 0.3678794) + seed);
                                       p += dot(p, p.yx + 19.19);
                                       return fract(p.x * p.y);
                                   }
                                   
                                   float2 getFeaturePoint(float2 cell, float seed, float randomness) {
                                       float2 randomFeaturePoint = float2(
                                           hashPoint(cell, seed),
                                           hashPoint(cell, seed + 17.0)
                                       );
                                       return mix(float2(0.5, 0.5), randomFeaturePoint, randomness);
                                   }
                                   
                                   float getNearestVoronoiDistance(float2 pos, float seed) {
                                       float2 cell = floor(pos);
                                       float minDist = LARGE_NUMBER;
                                   
                                       for (int y = -1; y <= 1; y++) {
                                           for (int x = -1; x <= 1; x++) {
                                               float2 neighborCell = cell + float2(float(x), float(y));
                                               float2 featurePoint = getFeaturePoint(neighborCell, seed, iRandomness);
                                               float2 delta = pos - (neighborCell + featurePoint);
                                               float dist = length(delta);
                                               minDist = min(minDist, dist);
                                           }
                                       }
                                       return minDist;
                                   }
                                   
                                   half4 main(float2 uv) {
                                       float noiseSum = 0.0;
                                       float amplitude = 1.0;
                                       float amplitudeSum = 0.0;
                                   
                                       for (int octave = 0; octave < MAX_OCTAVES; octave++) {
                                           if (octave >= iOctaves) break;
                                   
                                           float freq = iFrequency * exp2(float(octave));
                                           float2 samplePos = uv * freq;
                                           float dist = getNearestVoronoiDistance(
                                               samplePos,
                                               iSeed + float(octave) * FEATURE_SEED_SCALE
                                           );
                                   
                                           noiseSum += dist * amplitude;
                                           amplitudeSum += amplitude;
                                           amplitude *= 0.5;
                                       }
                                   
                                       return half4(noiseSum / amplitudeSum);
                                   }
                                   """;
        
        Uniforms uniforms = new Uniforms();
        uniforms.Add("iSeed", new Uniform("iSeed", seed));
        uniforms.Add("iFrequency", new Uniform("iFrequency", frequency));
        uniforms.Add("iOctaves", new Uniform("iOctaves", octaves));
        uniforms.Add("iRandomness", new Uniform("iRandomness", randomness));
        
        return Shader.Create(voronoiShaderCode, uniforms, out _);
    }

    public override Node CreateCopy() => new NoiseNode();
}

public enum NoiseType
{
    TurbulencePerlin,
    FractalPerlin,
    Voronoi
}
