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
            Nodes.NoiseType.Voronoi => GetVoronoiShader((float)(1d / (Scale.Value)), octaves, (float)Seed.Value, (int)VoronoiFeature.Value, (float)Randomness.Value, (float)AngleOffset.Value),
            _ => null
        };

        return shader;
    }

    private Shader GetVoronoiShader(float frequency, int octaves, float seed, int feature, float randomness, float angleOffset)
    {
        string voronoiShaderCode = """
                                   uniform float iSeed;
                                   uniform float iFrequency;
                                   uniform int iOctaves;
                                   uniform float iRandomness;
                                   uniform int iFeature;
                                   uniform float iAngleOffset;
                                   
                                   const int MAX_OCTAVES = 8;
                                   const float LARGE_NUMBER = 1e9;
                                   const float FEATURE_SEED_SCALE = 10.0;
                                   const float PI = 3.14159265;
                                   
                                   float hashPoint(float2 p, float seed) {
                                       p = fract(p * float2(0.3183099, 0.3678794) + seed);
                                       p += dot(p, p.yx + 19.19);
                                       return fract(p.x * p.y);
                                   }
                                   
                                   float2 getFeaturePoint(float2 cell, float seed, float randomness, float angleOffset) {
                                       float2 randomCellOffset = float2(
                                           hashPoint(cell, seed),
                                           hashPoint(cell, seed + 17.0)
                                       );
                                       
                                       float2 featurePoint = mix(float2(0.5, 0.5), randomCellOffset, randomness);
                                       
                                       float angle = hashPoint(cell, seed + 53.0) * PI * 2.0;
                                       angle += angleOffset;
                                       
                                       float2 dir = float2(cos(angle), sin(angle));
                                       float offsetAmount = 0.15;
                                       featurePoint += dir * offsetAmount * randomness;
                                       
                                       featurePoint = clamp(featurePoint, 0.0, 1.0);
                                       
                                       return featurePoint;
                                   }
                                   
                                   float2 getVoronoiDistances(float2 pos, float seed) {
                                       float2 cell = floor(pos);
                                       float minDist = LARGE_NUMBER;
                                       float secondMinDist = LARGE_NUMBER;
                                   
                                       for (int y = -1; y <= 1; y++) {
                                           for (int x = -1; x <= 1; x++) {
                                               float2 neighborCell = cell + float2(float(x), float(y));
                                               float2 featurePoint = getFeaturePoint(neighborCell, seed, iRandomness, iAngleOffset);
                                               float2 delta = pos - (neighborCell + featurePoint);
                                               float dist = length(delta);
                                               
                                               if (dist < minDist) {
                                                   secondMinDist = minDist;
                                                   minDist = dist;
                                               } else if (dist < secondMinDist) {
                                                   secondMinDist = dist;
                                               }
                                           }
                                       }
                                       return float2(minDist, secondMinDist);
                                   }
                                   
                                   half4 main(float2 uv) {
                                       float noiseSum = 0.0;
                                       float amplitude = 1.0;
                                       float amplitudeSum = 0.0;
                                   
                                       for (int octave = 0; octave < MAX_OCTAVES; octave++) {
                                           if (octave >= iOctaves) break;
                                   
                                           float freq = iFrequency * exp2(float(octave));
                                           float2 samplePos = uv * freq;
                                           
                                           float dist = 0.0;
                                           float2 distances = getVoronoiDistances(samplePos, iSeed + float(octave) * FEATURE_SEED_SCALE);
                                           float f1 = distances.x;
                                           float f2 = distances.y;
                                           
                                           if (iFeature == 0) {
                                               dist = f1;
                                           }
                                           else if (iFeature == 1) {
                                               dist = f2;
                                           }
                                           else if (iFeature == 2) {
                                               dist = f2 - f1;
                                           }
                                   
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
        uniforms.Add("iFeature", new Uniform("iFeature", feature));
        uniforms.Add("iAngleOffset", new Uniform("iAngleOffset", angleOffset));

        if (voronoiShader == null)
        {
            voronoiShader = Shader.Create(voronoiShaderCode, uniforms, out _);
        }
        else
        {
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
