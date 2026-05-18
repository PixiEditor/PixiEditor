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
    private double previousLacunarity = double.NaN;
    private double previousPersistence = double.NaN;
    private double previousZ = double.NaN;
    private double previousDimensions = -1;

    private Shader? voronoiShader;
    private Shader? valueShader;
    private Shader? perlinShader;
    private Shader? simplexValueShader;
    private Shader? simplexGradientShader;
    private Shader? voronoi2Shader;

    private Paint paint = new();

    private static readonly ColorFilter grayscaleFilter = ColorFilter.CreateColorMatrix(
        ColorMatrix.MapAlphaToRedGreenBlue + ColorMatrix.OpaqueAlphaOffset);

    public InputProperty<NoiseType> NoiseType { get; }

    public InputProperty<VecD> Offset { get; }

    public InputProperty<double> Scale { get; }

    public InputProperty<int> Octaves { get; }
    public InputProperty<int> Dimensions { get; }

    public InputProperty<double> Seed { get; }

    public InputProperty<VoronoiFeature> VoronoiFeature { get; }

    public InputProperty<double> Randomness { get; }

    public InputProperty<double> AngleOffset { get; }
    
    public InputProperty<double> Lacunarity { get; }
    public InputProperty<double> Persistence { get; }
    public InputProperty<double> Z { get; }

    public NoiseNode()
    {
        NoiseType = CreateInput(nameof(NoiseType), "NOISE_TYPE", Nodes.NoiseType.FractalPerlin);

        Offset = CreateInput(nameof(Offset), "OFFSET", new VecD(0d, 0d));
        Z = CreateInput(nameof(Z), "Z", 0d);

        Scale = CreateInput(nameof(Scale), "SCALE", 10d).WithRules(v => v.Min(0.1));
        Octaves = CreateInput(nameof(Octaves), "OCTAVES", 1)
            .WithRules(validator => validator.Min(1));

        Seed = CreateInput(nameof(Seed), "SEED", 0d);

        VoronoiFeature = CreateInput(nameof(VoronoiFeature), "VORONOI_FEATURE", Nodes.VoronoiFeature.F1);

        Randomness = CreateInput(nameof(Randomness), "RANDOMNESS", 1d)
            .WithRules(v => v.Min(0d).Max(1d));

        AngleOffset = CreateInput(nameof(AngleOffset), "ANGLE_OFFSET", 0d);
        Lacunarity = CreateInput(nameof(Lacunarity), "LACUNARITY", 2d).WithRules(v => v.Min(1d) );
        Persistence = CreateInput(nameof(Persistence), "PERSISTENCE", 0.5d).WithRules(v => v.Min(0d).Max(1d) );
        Dimensions = CreateInput(nameof(Dimensions), "DIMENSIONS", 2).WithRules(v => v.Min(1).Max(3) );
    }

    protected override void OnPaint(RenderContext context, Canvas target)
    {
        if (Math.Abs(previousScale - Scale.Value) > 0.000001
            || previousSeed != Seed.Value
            || previousOctaves != Octaves.Value
            || previousNoiseType != NoiseType.Value
            || previousOffset != Offset.Value
            || previousDimensions != Dimensions.Value
            || previousVoronoiFeature != VoronoiFeature.Value
            || Math.Abs(previousRandomness - Randomness.Value) > 0.000001
            || Math.Abs(previousAngleOffset - AngleOffset.Value) > 0.000001
            || Math.Abs(previousLacunarity - Lacunarity.Value) > 0.000001
            || Math.Abs(previousPersistence - Persistence.Value) > 0.000001
            || Math.Abs(previousZ - Z.Value) > 0.000001
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

            if ((NoiseType.Value == Nodes.NoiseType.Voronoi && paint.Shader != voronoiShader) 
                || (NoiseType.Value == Nodes.NoiseType.FractalValue && paint.Shader != valueShader)
                || (NoiseType.Value == Nodes.NoiseType.FractalPerlin2 && paint.Shader != perlinShader)
                || (NoiseType.Value == Nodes.NoiseType.FractalVoronoi && paint.Shader != voronoi2Shader)
                || (NoiseType.Value == Nodes.NoiseType.FractalSimplexValue && paint.Shader != simplexValueShader)
                || (NoiseType.Value == Nodes.NoiseType.FractalSimplexGradient && paint.Shader != simplexGradientShader)
               )
            {
                //paint?.Shader?.Dispose();
            }

            paint.Shader = shader;

            // Define a grayscale color filter to apply to the image
            //paint.ColorFilter = grayscaleFilter;

            previousScale = Scale.Value;
            previousSeed = Seed.Value;
            previousOctaves = Octaves.Value;
            previousNoiseType = NoiseType.Value;
            previousVoronoiFeature = VoronoiFeature.Value;
            previousRandomness = Randomness.Value;
            previousAngleOffset = AngleOffset.Value;
            previousLacunarity = Lacunarity.Value;
            previousPersistence = Persistence.Value;
            previousDimensions = Dimensions.Value;
            previousZ = Z.Value;
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

        if ((NoiseType.Value == Nodes.NoiseType.Voronoi && paint.Shader != voronoiShader) 
            || (NoiseType.Value == Nodes.NoiseType.FractalValue && paint.Shader != valueShader)
            || (NoiseType.Value == Nodes.NoiseType.FractalPerlin2 && paint.Shader != perlinShader)
            || (NoiseType.Value == Nodes.NoiseType.FractalVoronoi && paint.Shader != voronoi2Shader)
            || (NoiseType.Value == Nodes.NoiseType.FractalSimplexValue && paint.Shader != simplexValueShader)
            || (NoiseType.Value == Nodes.NoiseType.FractalSimplexGradient && paint.Shader != simplexGradientShader)
            )
        {
            //paint?.Shader?.Dispose();
        }

        paint.Shader = shader;
        // paint.ColorFilter = grayscaleFilter;

        RenderNoise(renderOn.Canvas);
    }


    private Shader SelectShader()
    {
        var freq = (float)(1d / Scale.Value);
        freq = Math.Max(freq, 0.000001f);

        int octaves = Math.Max(1, Octaves.Value);
        var lacunarity = (float)Math.Max(1, Lacunarity.Value);
        var persistence = (float)Math.Clamp(Persistence.Value, 0, 1);
        int dims = Math.Clamp(Dimensions.Value, 1, 3);
        Shader shader = NoiseType.Value switch
        {
            Nodes.NoiseType.TurbulencePerlin => Shader.CreatePerlinNoiseTurbulence(
                freq, freq, octaves, (float)Seed.Value),
            Nodes.NoiseType.FractalPerlin => Shader.CreatePerlinFractalNoise(
                freq, freq,
                octaves, (float)Seed.Value),
            Nodes.NoiseType.Voronoi => GetVoronoiShader(freq, octaves, (float)Seed.Value,
                (int)VoronoiFeature.Value, (float)Randomness.Value, (float)AngleOffset.Value, lacunarity, persistence),
            Nodes.NoiseType.FractalValue => GetValueShader(dims, freq, octaves, (float)Seed.Value, lacunarity, persistence, (float)Z.Value),
            Nodes.NoiseType.FractalPerlin2 => GetPerlinShader(dims, freq, octaves, (float)Seed.Value, lacunarity, persistence, (float)Z.Value),
            Nodes.NoiseType.FractalVoronoi => GetPerlinShader(dims, freq, octaves, (float)Seed.Value, lacunarity, persistence, (float)Z.Value),
            Nodes.NoiseType.FractalSimplexValue => GetSimplexValueShader(dims, freq, octaves, (float)Seed.Value, lacunarity, persistence, (float)Z.Value),
            Nodes.NoiseType.FractalSimplexGradient => GetSimplexGradientShader(dims, freq, octaves, (float)Seed.Value, lacunarity, persistence, (float)Z.Value),
            
            _ => null
        };

        return shader;
    }

    private const string MainShaderCode =
        """
        NoiseSample sum(float3 p, float freq, int d, int oct, float lac, float per) {
            NoiseSample sum = noiseSelectorD(p, freq, d);
            float amplitude = 1;
            float range = 1;
            for (int o = 1; o<8; o++) {
                if (o>=oct) break;
                freq *= lac;
                amplitude *= per;
                range += amplitude;
                sum = add(sum, mul(noiseSelectorD(p, freq, d), amplitude));
            }
            return mul(sum, 1./range);
        }
        
        half4 main(float2 uv)
        {
            NoiseSample s = sum(float3(uv, iZ), iFrequency/iResolution.x, iDimensions, iOctaves, iLacunarity, iPersistence);
            return half4(s.value,s.derivative/6.+0.5);
        }
        """;
    private const string BaseShaderCode = 
        """
        const float sqr2 = sqrt(2);

        float mod289(const in float x) { return x - floor(x * (1. / 289.)) * 289.; }
        float2 mod289(const in float2 x) { return x - floor(x * (1. / 289.)) * 289.; }
        float3 mod289(const in float3 x) { return x - floor(x * (1. / 289.)) * 289.; }

        float permute(const in float v) { return mod289(((v * 34.0) + 1.0) * v); }
        float2 permute(const in float2 v) { return mod289(((v * 34.0) + 1.0) * v); }
        float3 permute(const in float3 v) { return mod289(((v * 34.0) + 1.0) * v); }

        float quintic(const in float v) { return v*v*v*(v*(v*6.0-15.0)+10.0); }
        float2  quintic(const in float2 v)  { return v*v*v*(v*(v*6.0-15.0)+10.0); }
        float3  quintic(const in float3 v)  { return v*v*v*(v*(v*6.0-15.0)+10.0); }

        float quinticDerivative(const in float v) { return 30.0*v*v*(v*(v-2.0)+1.0); }
        float2  quinticDerivative(const in float2 v)  { return 30.0*v*v*(v*(v-2.0)+1.0); }
        float3  quinticDerivative(const in float3 v)  { return 30.0*v*v*(v*(v-2.0)+1.0); }
        const float4 scale = vec4(443.897, 441.423, .0973, .1099);
        float random(in float x) {
          x = permute(x);
          x = fract(x * scale.x);
          return fract(2*pow(x, 2)* pow(33.33 + x, 2));
        }
        float random(in float2 x) {
          return random(random(x.x)+x.y);
        }
        float random(in float3 x) {
          return random(random(x.xy)+x.z);
        }
        struct NoiseSample {
          float value;
          float3 derivative;
        };
        NoiseSample add(NoiseSample a, float b) {
          a.value += b;
          return a;
        }
        NoiseSample add(float a, NoiseSample b) {
          b.value += a;
          return b;
        }
        NoiseSample add(NoiseSample a, NoiseSample b) {
          a.value += b.value;
          a.derivative += b.derivative;
          return a;
        }
        NoiseSample sub(NoiseSample a, float b) {
          a.value -= b;
          return a;
        }
        NoiseSample sub(float a, NoiseSample b) {
          b.value = a - b.value;
          b.derivative = -b.derivative;
          return b;
        }
        NoiseSample sub(NoiseSample a, NoiseSample b) {
          a.value -= b.value;
          a.derivative -= b.derivative;
          return a;
        }
        NoiseSample mul(NoiseSample a, float b) {
          a.value *= b;
          a.derivative *= b;
          return a;
        }
        NoiseSample mul(float a, NoiseSample b) {
          b.value *= a;
          b.derivative *= a;
          return b;
        }
        NoiseSample mul(NoiseSample a, NoiseSample b) {
          a.derivative = a.derivative * b.value + b.derivative * a.value;
          a.value *= b.value;
          return a;
        }
        """;

    private Shader GetValueShader(int dimensions, float frequency, int octaves, float seed, float lacunarity, float persistence, float z)
    {
        const string valueShaderCode = BaseShaderCode+ 
                                       """
                                       uniform float iSeed;
                                       uniform float iFrequency;
                                       uniform int iOctaves;
                                       uniform float iLacunarity;
                                       uniform float iPersistence;
                                       uniform int iDimensions;
                                       uniform float iZ;
                                       uniform float2 iResolution;

                                       NoiseSample value1d(float p, float freq) {
                                           p *= freq;
                                           float i0 = floor(p);
                                           float t = fract(p);
                                           float i1 = i0+1;
                                           float h0 = random(i0);
                                           float h1 = random(i1);
                                           float dt = quinticDerivative(t);
                                           t = quintic(t);

                                           float a = h0;
                                           float b = h1-h0;

                                           NoiseSample samp;
                                           samp.value = a+b*t;
                                           samp.derivative = float3(0);
                                           samp.derivative.x = b*dt;
                                           samp.derivative *= freq;
                                           return sub(mul(samp,2),1);
                                       }
                                       NoiseSample value2d(float2 p, float freq) {
                                           p *= freq;
                                           float2 i0 = floor(p);
                                           float2 t = fract(p);
                                           float2 i1 = i0+1;
                                           float h0 = random(i0.x);
                                           float h1 = random(i1.x);
                                           float h00 = random(h0+i0.y);
                                           float h10 = random(h1+i0.y);
                                           float h01 = random(h0+i1.y);
                                           float h11 = random(h1+i1.y);
                                           float2 dt = quinticDerivative(t);
                                           t = quintic(t);

                                           float a = h00;
                                           float b = h10 - h00;
                                           float c = h01 - h00;
                                           float d = h11 - h01 - h10 + h00;

                                           NoiseSample samp;
                                           samp.value = a + b * t.x + (c + d * t.x) * t.y;
                                           samp.derivative.x = (b + d * t.y) * dt.x;
                                           samp.derivative.y = (c + d * t.x) * dt.y;
                                           samp.derivative.z = 0;
                                           samp.derivative *= freq;
                                           return sub(mul(samp,2),1);
                                       }
                                       NoiseSample value3d(float3 p, float freq) {
                                           p *= freq;
                                           float3 i0 = floor(p);
                                           float3 t = fract(p);
                                           float3 i1 = i0+1;
                                           float h0 = random(i0.x);
                                           float h1 = random(i1.x);
                                           float h00 = random(h0+i0.y);
                                           float h10 = random(h1+i0.y);
                                           float h01 = random(h0+i1.y);
                                           float h11 = random(h1+i1.y);

                                           float h000 = random(h00+i0.z);
                                           float h100 = random(h10+i0.z);
                                           float h010 = random(h01+i0.z);
                                           float h110 = random(h11+i0.z);
                                           float h001 = random(h00+i1.z);
                                           float h101 = random(h10+i1.z);
                                           float h011 = random(h01+i1.z);
                                           float h111 = random(h11+i1.z);
                                           float3 dt = quinticDerivative(t);
                                           t = quintic(t);


                                           float a = h000;
                                           float b = h100 - h000;
                                           float c = h010 - h000;
                                           float d = h001 - h000;
                                           float e = h110 - h010 - h100 + h000;
                                           float f = h101 - h001 - h100 + h000;
                                           float g = h011 - h001 - h010 + h000;
                                           float h = h111 - h011 - h101 + h001 - h110 + h010 + h100 - h000;

                                           NoiseSample samp;

                                           samp.value =  a + b * t.x + (c + e * t.x) * t.y + (d + f * t.x + (g + h * t.x) * t.y) * t.z;
                                           samp.derivative.x = (b + e * t.y + (f + h * t.y) * t.z) * dt.x;
                                           samp.derivative.y = (c + e * t.x + (g + h * t.x) * t.z) * dt.y;
                                           samp.derivative.z = (d + f * t.x + (g + h * t.x) * t.y) * dt.z;
                                           samp.derivative *= freq;
                                           return sub(mul(samp,2),1);
                                       }
                                       NoiseSample noiseSelectorD(float3 p, float freq, int d) {
                                           if (d == 1)
                                           return value1d(p.x, freq);
                                           if (d == 2)
                                           return value2d(p.xy, freq);
                                           if (d == 3)
                                           return value3d(p, freq);
                                           return NoiseSample(0, float3(0));;
                                       }
                                       """ + MainShaderCode;
        // valueShader = null;
        Uniforms uniforms = new Uniforms();
        uniforms.Add("iSeed", new Uniform("iSeed", seed));
        uniforms.Add("iFrequency", new Uniform("iFrequency", frequency));
        uniforms.Add("iOctaves", new Uniform("iOctaves", octaves));
        uniforms.Add("iLacunarity", new Uniform("iLacunarity", lacunarity));
        uniforms.Add("iPersistence", new Uniform("iPersistence", persistence));
        uniforms.Add("iDimensions", new Uniform("iDimensions", dimensions));
        uniforms.Add("iZ", new Uniform("iZ", z));

        if (valueShader == null)
        {
            valueShader = Shader.Create(valueShaderCode, uniforms, out _);
        }
        else
        {
            valueShader = valueShader.WithUpdatedUniforms(uniforms);
        }

        return valueShader;
    }

    private Shader GetPerlinShader(int dimensions, float frequency, int octaves, float seed, float lacunarity, float persistence, float z)
    {
        const string perlinShaderCode = BaseShaderCode+ 
                                        """
                                        uniform float iSeed;
                                        uniform float iFrequency;
                                        uniform int iOctaves;
                                        uniform float iLacunarity;
                                        uniform float iPersistence;
                                        uniform int iDimensions;
                                        uniform float iZ;
                                        uniform float2 iResolution;

                                        float gradients1d(float p) {
                                            return mix(1, -1, mod(floor(p), 2));
                                        }
                                        float2 gradients2d(float p) {
                                            float2 a = float2(normalize(gradients1d(p)),
                                            normalize(gradients1d(p/2)));
                                            float c = gradients1d(p);
                                            float2 b = float2(mix(c.x0, c.0x, mod(floor(p/2), 2)));
                                            return float2(mix(b, a, mod(floor(p/4), 2)));
                                        }
                                        float3 gradients3d(float p) {
                                            float3 a = float3((gradients1d(p)), (gradients1d(p/2)), 1);
                                            if (int(mod(p/4, 4)) == 0)
                                                return a.xy0;
                                            if (int(mod(p/4, 4)) == 1)
                                                return a.x0y;
                                            if (int(mod(p/4, 4)) == 2)
                                                return a.0xy;
                                            if (int(mod(p/2, 2)) == 0)
                                                return a.xy0;
                                            return a.0xy;
                                        }

                                        NoiseSample perlin1d(float p, float freq) {
                                            p *= freq;
                                            float i0 = floor(p.x);
                                            float t0 = fract(p.x);
                                            float t1 = t0 -1;
                                            float i1 = i0+1;

                                            float g0 = gradients1d(permute(i0));
                                            float g1 = gradients1d(permute(i1));

                                            float v0 = g0*t0;
                                            float v1 = g1*t1;
                                            float dt = quinticDerivative(t0);
                                            float t = quintic(t0);

                                            float da = g0;
                                            float db = g1 - g0;

                                            float a = v0;
                                            float b = v1-v0;
                                            NoiseSample samp;
                                            samp.value =  a+b*t;
                                            samp.derivative = float3(0);
                                            samp.derivative.x = da+db*t+b*dt;
                                            return mul(samp, 2);
                                        }

                                        NoiseSample perlin2d(float2 p, float freq) {
                                            p *= freq;
                                            float2 i0 = floor(p);
                                            float2 t0 = fract(p);
                                            float2 t1 = t0-1;
                                            float2 i1 = i0+1;
                                            float h0 = random(i0.x);
                                            float h1 = random(i1.x);

                                            float2 g00 = gradients2d(permute(h0+i0.y));
                                            float2 g10 = gradients2d(permute(h1+i0.y));
                                            float2 g01 = gradients2d(permute(h0+i1.y));
                                            float2 g11 = gradients2d(permute(h1+i1.y));

                                            float v00 = dot(g00, t0);
                                            float v10 = dot(g10, float2(t1.x, t0.y));
                                            float v01 = dot(g01, float2(t0.x, t1.y));
                                            float v11 = dot(g11, t1);
                                            float2 dt = quinticDerivative(t0);
                                            float2 t = quintic(t0);

                                            float a = v00;
                                            float b = v10 - v00;
                                            float c = v01 - v00;
                                            float d = v11 - v01 - v10 + v00;

                                            float2 da = g00;
                                            float2 db = g10 - g00;
                                            float2 dc = g01 - g00;
                                            float2 dd = g11 - g01 - g10 + g00;

                                            NoiseSample samp;
                                            samp.value = a + b * t.x + (c + d * t.x) * t.y;
                                            samp.derivative = float3(da + db * t.x + (dc + dd * t.x) * t.y, 0);
                                            samp.derivative.x += (b + d * t.y) * dt.x;
                                            samp.derivative.y += (c + d * t.x) * dt.y;
                                            samp.derivative *= freq;
                                            return mul(samp, sqr2);
                                        }
                                        NoiseSample perlin3d(float3 p, float freq) {
                                            p *= freq;
                                            float3 i0 = floor(p);
                                            float3 t0 = fract(p);
                                            float3 i1 = i0+1;
                                            float3 t1 = t0 -1;
                                            float h0 = permute(i0.x);
                                            float h1 = permute(i1.x);
                                            float h00 = permute(h0+i0.y);
                                            float h10 = permute(h1+i0.y);
                                            float h01 = permute(h0+i1.y);
                                            float h11 = permute(h1+i1.y);

                                            float3 g000 = gradients3d(random(h00+i0.z)*255);
                                            float3 g100 = gradients3d(random(h10+i0.z)*255);
                                            float3 g010 = gradients3d(random(h01+i0.z)*255);
                                            float3 g110 = gradients3d(random(h11+i0.z)*255);
                                            float3 g001 = gradients3d(random(h00+i1.z)*255);
                                            float3 g101 = gradients3d(random(h10+i1.z)*255);
                                            float3 g011 = gradients3d(random(h01+i1.z)*255);
                                            float3 g111 = gradients3d(random(h11+i1.z)*255);

                                            float v000 = dot(g000, t0);
                                            float v100 = dot(g100, float3(t1.x, t0.y, t0.z));
                                            float v010 = dot(g010, float3(t0.x, t1.y, t0.z));
                                            float v110 = dot(g110, float3(t1.xy, t0.z));
                                            float v001 = dot(g001, float3(t0.xy, t1.z));
                                            float v101 = dot(g101, float3(t1.x, t0.y, t1.z));
                                            float v011 = dot(g011, float3(t0.x, t1.y, t1.z));
                                            float v111 = dot(g111, t1);
                                            float3 dt = quinticDerivative(t0);
                                            float3 t = quintic(t0);

                                            float a = v000;
                                            float b = v100 - v000;
                                            float c = v010 - v000;
                                            float d = v001 - v000;
                                            float e = v110 - v010 - v100 + v000;
                                            float f = v101 - v001 - v100 + v000;
                                            float g = v011 - v001 - v010 + v000;
                                            float h = v111 - v011 - v101 + v001 - v110 + v010 + v100 - v000;

                                            float3 da = g000;
                                            float3 db = g100 - g000;
                                            float3 dc = g010 - g000;
                                            float3 dd = g001 - g000;
                                            float3 de = g110 - g010 - g100 + g000;
                                            float3 df = g101 - g001 - g100 + g000;
                                            float3 dg = g011 - g001 - g010 + g000;
                                            float3 dh = g111 - g011 - g101 + g001 - g110 + g010 + g100 - g000;

                                            NoiseSample samp;
                                            samp.value = a + b * t.x + (c + e * t.x) * t.y + (d + f * t.x + (g + h * t.x) * t.y) * t.z;
                                            samp.derivative = da + db * t.x + (dc + de * t.x) * t.y + (dd + df * t.x + (dg + dh * t.x) * t.y) * t.z;
                                            samp.derivative.x += (b + e * t.y + (f + h * t.y) * t.z) * dt.x;
                                            samp.derivative.y += (c + e * t.x + (g + h * t.x) * t.z) * dt.y;
                                            samp.derivative.z += (d + f * t.x + (g + h * t.x) * t.y) * dt.z;
                                            samp.derivative *= freq;
                                            return samp;
                                        }
                                        NoiseSample noiseSelectorD(float3 p, float freq, int d) {
                                            if (d == 1)
                                            return perlin1d(p.x, freq);
                                            if (d == 2)
                                            return perlin2d(p.xy, freq);
                                            if (d == 3)
                                            return perlin3d(p, freq);
                                            return NoiseSample(0, float3(0));;
                                        }
                                        """ + MainShaderCode;
        // valueShader = null;
        Uniforms uniforms = new Uniforms();
        uniforms.Add("iSeed", new Uniform("iSeed", seed));
        uniforms.Add("iFrequency", new Uniform("iFrequency", frequency));
        uniforms.Add("iOctaves", new Uniform("iOctaves", octaves));
        uniforms.Add("iLacunarity", new Uniform("iLacunarity", lacunarity));
        uniforms.Add("iPersistence", new Uniform("iPersistence", persistence));
        uniforms.Add("iDimensions", new Uniform("iDimensions", dimensions));
        uniforms.Add("iZ", new Uniform("iZ", z));

        if (perlinShader == null)
        {
            perlinShader = Shader.Create(perlinShaderCode, uniforms, out _);
        }
        else
        {
            perlinShader = perlinShader.WithUpdatedUniforms(uniforms);
        }

        return perlinShader;
    }

     private Shader GetSimplexGradientShader(int dimensions, float frequency, int octaves, float seed, float lacunarity, float persistence, float z)
    {
        const string simplexGradientShaderCode = BaseShaderCode+ 
                                       """
                                       uniform float iSeed;
                                       uniform float iFrequency;
                                       uniform int iOctaves;
                                       uniform float iLacunarity;
                                       uniform float iPersistence;
                                       uniform int iDimensions;
                                       uniform float iZ;
                                       uniform float2 iResolution;
                                       
                                       const float squaresToTriangles = (3-sqrt(3))/6.;
                                       const float trianglesToSquares = (sqrt(3)-1)/2.;
                                       
                                       float gradients1d(float p) {
                                           return mix(1, -1, mod(floor(p), 2));
                                       }
                                       float2 gradients2d(float p) {
                                           float2 a = float2(normalize(gradients1d(p)),
                                           normalize(gradients1d(p/2)));
                                           float c = gradients1d(p);
                                           float2 b = float2(mix(c.x0, c.0x, mod(floor(p/2), 2)));
                                           return float2(mix(b, a, mod(floor(p/4), 2)));
                                       }
                                       float3 gradients3d(float p) {
                                           float3 a = float3((gradients1d(p)), (gradients1d(p/2)), 1);
                                           if (int(mod(p/4, 4)) == 0)
                                               return a.xy0;
                                           if (int(mod(p/4, 4)) == 1)
                                               return a.x0y;
                                           if (int(mod(p/4, 4)) == 2)
                                               return a.0xy;
                                           if (int(mod(p/2, 2)) == 0)
                                               return a.xy0;
                                           return a.0xy;
                                       }
                                       float3 simplexGradients3d(float p)
                                       {
                                           float3 a = float3(gradients1d(p),gradients1d(p/2),gradients1d(p/4));
                                       	float a4 = mod(floor(p / 4), 3);
                                       	if (floor(p / 24) > 0)
                                       		return a;
                                       	if (a4 == 0)
                                       		return a.xy0;
                                       	if (a4 == 1)
                                       		return a.x0y;
                                       	return a.0xy;
                                       }

                                       
                                       NoiseSample simplexGradient1dPart(float p, float i) {
                                           float x = p-i;
                                           float f = 1-x*x;
                                           float f2 = f*f;
                                           float f3 = f*f2;
                                           float g = gradients1d(random(i));
                                           float v = g*x;
                                           NoiseSample samp;
                                           samp.value = v*f3;
                                           samp.derivative.x = g*f3-6.*v*x*f2;
                                           return samp;
                                       }
                                       NoiseSample simplexGradient1d(float p, float freq) {
                                           p *= freq;
                                           float i = floor(p);
                                           NoiseSample samp = simplexGradient1dPart(p,i);
                                           samp = add(samp, simplexGradient1dPart(p,i+1));
                                           samp.derivative *= freq;
                                           return sub(mul(samp,2),1);
                                       }
                                       const float simplexScale2D = 2916.* sqr2 / 125.;
                                       
                                       NoiseSample simplexGradient2dPart(float2 p, float2 i) {
                                           float unskew = (i.x+i.y)*squaresToTriangles;
                                           float2 x = p-i+unskew;
                                           float f = 0.5-x.x*x.x-x.y*x.y;
                                       
                                           NoiseSample samp = NoiseSample(0,float3(0));
                                           if(f>0) {
                                               float f2 = f*f;
                                               float f3 = f*f2;
                                               float2 g = gradients2d(random(i));
                                               float v = dot(g,x.xy);
                                               float v6f2 = -6. * v * f2;
                                               samp.value = v*f3;
                                               samp.derivative.x = g.x*f3+v6f2*x.x;
                                               samp.derivative.y = g.y*f3+v6f2*x.y;
                                           }
                                           return samp;
                                       }
                                       NoiseSample simplexGradient2dPart(float2 p, float ix, float iy) {
                                           return simplexGradient2dPart(p,float2(ix,iy));
                                       }
                                       
                                       NoiseSample simplexGradient2d(float2 p, float freq) {
                                           p *= freq;
                                           float skew = (p.x+p.y)*trianglesToSquares;
                                           float2 s = p+skew;
                                           float2 i = floor(s);
                                           NoiseSample samp = simplexGradient2dPart(p,i);
                                           samp = add(samp, simplexGradient2dPart(p,i+1));
                                           if(s.x - i.x >= s.y - i.y) {
                                               samp = add(samp,simplexGradient2dPart(p,i.x+1,i.y));
                                           } else {
                                               samp = add(samp,simplexGradient2dPart(p,i.x,i.y+1));
                                           }
                                           samp.derivative *= freq;
                                           return mul(samp,simplexScale2D);
                                       }
                                       
                                       
                                       NoiseSample simplexGradient3dPart(float3 p, float3 i) {
                                           float unskew = (i.x+i.y+i.z)*(1./6.);
                                           float3 x = p-i+unskew;
                                           float f = 0.5-x.x*x.x-x.y*x.y-x.z*x.z;
                                       
                                           NoiseSample samp = NoiseSample(0,float3(0));
                                           if(f>0) {
                                               float f2 = f*f;
                                               float f3 = f*f2;
                                               float3 g = simplexGradients3d(random(i));
                                               float v = dot(g,x);
                                               float v6f2 = -6. * v * f2;
                                               samp.value = v*f3;
                                               samp.derivative.x =g.x*f3+v6f2*x.x;
                                               samp.derivative.y = g.y*f3+v6f2*x.y;
                                               samp.derivative.z = g.z*f3+v6f2*x.z;
                                           }
                                           return samp;
                                       }
                                       NoiseSample simplexGradient3dPart(float3 p, float ix, float iy, float iz) {
                                           return simplexGradient3dPart(p,float3(ix,iy,iz));
                                       }
                                       const float simplexScale3D = 8192. * sqrt(3) / 375.;
                                       NoiseSample simplexGradient3d(float3 p, float freq) {
                                           p *= freq;
                                           float skew = (p.x+p.y+p.z)*(1./3.);
                                           float3 s = p+skew;
                                           float3 i = floor(s);
                                           float3 x = s-i;
                                           NoiseSample samp = simplexGradient3dPart(p,i);
                                           samp = add(samp, simplexGradient3dPart(p,i+1));
                                           if(x.x >= x.y) {
                                               if(x.x>=x.z) {
                                                   samp = add(samp,simplexGradient3dPart(p,i.x+1,i.y,i.z));
                                                   if(x.y>=x.z) {
                                                       samp = add(samp, simplexGradient3dPart(p,i.x+1,i.y+1,i.z));
                                                   }
                                                   else {
                                                       samp = add(samp, simplexGradient3dPart(p,i.x+1,i.y,i.z+1));
                                                   }
                                               }
                                               else {
                                                   samp = add(samp,simplexGradient3dPart(p,i.x,i.y,i.z+1));
                                                   samp = add(samp,simplexGradient3dPart(p,i.x+1,i.y,i.z+1));
                                               }
                                           } else {
                                               if(x.y>=x.z) {
                                                   samp = add(samp,simplexGradient3dPart(p,i.x,i.y+1,i.z));
                                                   if(x.x>=x.z) {
                                                       samp = add(samp, simplexGradient3dPart(p,i.x+1,i.y+1,i.z));
                                                   }
                                                   else {
                                                       samp = add(samp, simplexGradient3dPart(p,i.x,i.y+1,i.z+1));
                                                   }
                                               }
                                               else {
                                                   samp = add(samp,simplexGradient3dPart(p,i.x,i.y,i.z+1));
                                                   samp = add(samp,simplexGradient3dPart(p,i.x,i.y+1,i.z+1));
                                               }
                                           }
                                           samp.derivative *= freq;
                                           return mul(samp,simplexScale3D);
                                       }
                                       
                                       NoiseSample noiseSelectorD(float3 p, float freq, int d) {
                                           if (d == 1)
                                           return simplexGradient1d(p.x, freq);
                                           if (d == 2)
                                           return simplexGradient2d(p.xy, freq);
                                           if (d == 3)
                                           return simplexGradient3d(p, freq);
                                           return NoiseSample(0, float3(0));;
                                       }
                                       """ + MainShaderCode;
        // valueShader = null;
        Uniforms uniforms = new Uniforms();
        uniforms.Add("iSeed", new Uniform("iSeed", seed));
        uniforms.Add("iFrequency", new Uniform("iFrequency", frequency));
        uniforms.Add("iOctaves", new Uniform("iOctaves", octaves));
        uniforms.Add("iLacunarity", new Uniform("iLacunarity", lacunarity));
        uniforms.Add("iPersistence", new Uniform("iPersistence", persistence));
        uniforms.Add("iDimensions", new Uniform("iDimensions", dimensions));
        uniforms.Add("iZ", new Uniform("iZ", z));

        if (simplexGradientShader == null)
        {
            simplexGradientShader = Shader.Create(simplexGradientShaderCode, uniforms, out _);
        }
        else
        {
            simplexGradientShader = simplexGradientShader.WithUpdatedUniforms(uniforms);
        }

        return simplexGradientShader;
    }

     private Shader GetSimplexValueShader(int dimensions, float frequency, int octaves, float seed, float lacunarity, float persistence, float z)
    {
        string simplexValueShaderCode = BaseShaderCode+ 
                                              """
                                              uniform float iSeed;
                                              uniform float iFrequency;
                                              uniform int iOctaves;
                                              uniform float iLacunarity;
                                              uniform float iPersistence;
                                              uniform int iDimensions;
                                              uniform float iZ;
                                              
                                              const float squaresToTriangles = (3-sqrt(3))/6.;
                                              const float trianglesToSquares = (sqrt(3)-1)/2.;
                                              
                                              NoiseSample simplexValue1dPart(float p, float i) {
                                                  float x = p-i;
                                                  float f = 1-x*x;
                                                  float f2 = f*f;
                                                  float f3 = f*f2;
                                                  float h = random(i);
                                                  NoiseSample samp;
                                                  samp.value = h*f3;
                                                  samp.derivative.x = -6.*h*x*f2;
                                                  return samp;
                                              }

                                              NoiseSample simplexValue1d(float p, float freq) {
                                                  p *= freq;
                                                  float i = floor(p);
                                                  NoiseSample samp = simplexValue1dPart(p,i);
                                                  samp = add(samp, simplexValue1dPart(p,i+1));
                                                  samp.derivative *= freq;
                                                  return sub(mul(samp,2),1);
                                              }
                                              
                                              NoiseSample simplexValue2dPart(float2 p, float2 i) {
                                                  float unskew = (i.x+i.y)*squaresToTriangles;
                                                  float2 x = p-i+unskew;
                                                  float f = 0.5-x.x*x.x-x.y*x.y;
                                              
                                                  NoiseSample samp = NoiseSample(0,float3(0));
                                                  if(f>0) {
                                                      float f2 = f*f;
                                                      float f3 = f*f2;
                                                      float h = random(i);
                                                      float h6f2 = -6. * h * f2;
                                                      samp.value = h*f3;
                                                      samp.derivative.x = h6f2*x.x;
                                                      samp.derivative.y = h6f2*x.y;
                                                  }
                                                  return samp;
                                              }
                                              NoiseSample simplexValue2dPart(float2 p, float ix, float iy) {
                                                  return simplexValue2dPart(p,float2(ix,iy));
                                              }
                                              
                                              NoiseSample simplexValue2d(float2 p, float freq) {
                                                  p *= freq;
                                                  float skew = (p.x+p.y)*trianglesToSquares;
                                                  float2 s = p+skew;
                                                  float2 i = floor(s);
                                                  NoiseSample samp = simplexValue2dPart(p,i);
                                                  samp = add(samp, simplexValue2dPart(p,i+1));
                                                  if(s.x - i.x >= s.y - i.y)
                                                      samp = add(samp,simplexValue2dPart(p,i.x+1,i.y));
                                                  else 
                                                      samp = add(samp,simplexValue2dPart(p,i.x,i.y+1));
                                                  
                                                  samp.derivative *= freq;
                                                  return sub(mul(samp,8*2),1);
                                              }
                                              NoiseSample simplexValue3dPart(float3 p, float3 i) {
                                                  float unskew = (i.x+i.y+i.z)*(1./6.);
                                                  float3 x = p-i+unskew;
                                                  float f = 0.5-x.x*x.x-x.y*x.y-x.z*x.z;
                                              
                                                  NoiseSample samp = NoiseSample(0,float3(0));
                                                  if(f>0) {
                                                      float f2 = f*f;
                                                      float f3 = f*f2;
                                                      float h = random(i);
                                                      float h6f2 = -6. * h * f2;
                                                      samp.value = h*f3;
                                                      samp.derivative.x = h6f2*x.x;
                                                      samp.derivative.y = h6f2*x.y;
                                                      samp.derivative.z = h6f2*x.z;
                                                  }
                                                  return samp;
                                              }
                                              
                                              NoiseSample simplexValue3d(float3 p, float freq) {
                                                  p *= freq;
                                                  float skew = (p.x+p.y+p.z)*(1./3.);
                                                  float3 s = p+skew;
                                                  float3 i = floor(s);
                                                  float3 x = s-i;
                                                  NoiseSample samp = simplexValue3dPart(p,i);
                                                  samp = add(samp, simplexValue3dPart(p,i+1));
                                                  if(x.x >= x.y) {
                                                      if(x.x>=x.z) {
                                                          samp = add(samp,simplexValue3dPart(p,float3(i.x+1,i.y,i.z)));
                                                          if(x.y>=x.z)
                                                          samp = add(samp, simplexValue3dPart(p,float3(i.x+1,i.y+1,i.z)));
                                                          else
                                                          samp = add(samp, simplexValue3dPart(p,float3(i.x+1,i.y,i.z+1)));
                                                      }
                                                      else {
                                                          samp = add(samp,simplexValue3dPart(p,float3(i.x,i.y,i.z+1)));
                                                          samp = add(samp,simplexValue3dPart(p,float3(i.x+1,i.y,i.z+1)));
                                                      }
                                                  } else {
                                                      if(x.y>=x.z) {
                                                          samp = add(samp,simplexValue3dPart(p,float3(i.x,i.y+1,i.z)));
                                                          if(x.x>=x.z)
                                                          samp = add(samp, simplexValue3dPart(p,float3(i.x+1,i.y+1,i.z)));
                                                          else
                                                          samp = add(samp, simplexValue3dPart(p,float3(i.x,i.y+1,i.z+1)));
                                                      }
                                                      else {
                                                          samp = add(samp,simplexValue3dPart(p,float3(i.x,i.y,i.z+1)));
                                                          samp = add(samp,simplexValue3dPart(p,float3(i.x,i.y+1,i.z+1)));
                                                      }
                                                  }
                                                  samp.derivative *= freq;
                                                  return sub(mul(samp,8*2),1);
                                              }
                                              
                                              
                                              NoiseSample noiseSelectorD(float3 p, float freq, int d) {
                                                  if (d == 1)
                                                  return simplexValue1d(p.x, freq);
                                                  if (d == 2)
                                                  return simplexValue2d(p.xy, freq);
                                                  if (d == 3)
                                                  return simplexValue3d(p, freq);
                                                  return NoiseSample(0, float3(0));;
                                              }
                                              """
                                              + MainShaderCode;
        // valueShader = null;
        Uniforms uniforms = new Uniforms();
        uniforms.Add("iSeed", new Uniform("iSeed", seed));
        uniforms.Add("iFrequency", new Uniform("iFrequency", frequency));
        uniforms.Add("iOctaves", new Uniform("iOctaves", octaves));
        uniforms.Add("iLacunarity", new Uniform("iLacunarity", lacunarity));
        uniforms.Add("iPersistence", new Uniform("iPersistence", persistence));
        uniforms.Add("iDimensions", new Uniform("iDimensions", dimensions));
        uniforms.Add("iZ", new Uniform("iZ", z));

        if (simplexValueShader == null)
        {
            simplexValueShader = Shader.Create(simplexValueShaderCode, uniforms, out _);
        }
        else
        {
            simplexValueShader = simplexValueShader.WithUpdatedUniforms(uniforms);
        }

        return simplexValueShader;
    }

    
    
    private Shader GetVoronoiShader(float frequency, int octaves, float seed, int feature, float randomness,
        float angleOffset, float lacunarity, float persistence)
    {
        string voronoiShaderCode = """
                                   uniform float iSeed;
                                   uniform float iFrequency;
                                   uniform int iOctaves;
                                   uniform float iRandomness;
                                   uniform int iFeature;
                                   uniform float iAngleOffset;
                                   uniform float iLacunarity;
                                   uniform float iPersistence;

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
                                       float frequency = iResolution.x/iFrequency;

                                       for (int octave = 0; octave < MAX_OCTAVES; octave++) {
                                           if (octave >= iOctaves) break;

                                           //float freq = iFrequency * exp2(float(octave));
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
                                           frequency *= iLacunarity;
                                           noiseSum += dist * amplitude;
                                           amplitudeSum += amplitude;
                                           amplitude *= iPersistence;
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
        uniforms.Add("iLacunarity", new Uniform("iLacunarity", lacunarity));
        uniforms.Add("iPersistence", new Uniform("iPersistence", persistence));

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
    Voronoi,
    FractalValue,
    FractalPerlin2,
    FractalVoronoi,
    FractalSimplexValue,
    FractalSimplexGradient
}

public enum VoronoiFeature
{
    F1 = 0, // Distance to the closest feature point
    F2 = 1, // Distance to the second-closest feature point
    F2MinusF1 = 2
}
