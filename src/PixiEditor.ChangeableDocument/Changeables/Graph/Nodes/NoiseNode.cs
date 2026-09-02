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
    private double previousPeriod = double.NaN;
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
    private int previousDimensions = -1;
    private bool previousTurbulence = false;
    private bool previousTiling = false;

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
    public InputProperty<bool> Turbulence { get; }
    public InputProperty<bool> Tiling { get; }

    public InputProperty<double> Seed { get; }

    public InputProperty<VoronoiFeature> VoronoiFeature { get; }

    public InputProperty<double> Randomness { get; }

    public InputProperty<double> AngleOffset { get; }
    
    public InputProperty<double> Lacunarity { get; }
    public InputProperty<double> Persistence { get; }
    public InputProperty<double> Z { get; }
    public InputProperty<double> Period { get; }

    public NoiseNode()
    {
        NoiseType = CreateInput(nameof(NoiseType), "NOISE_TYPE", Nodes.NoiseType.FractalPerlin);

        Offset = CreateInput(nameof(Offset), "OFFSET", new VecD(0d, 0d));
        Z = CreateInput(nameof(Z), "Z", 0d);

        Scale = CreateInput(nameof(Scale), "SCALE", 10d).WithRules(v => v.Min(0.1));
        Period = CreateInput(nameof(Period), "PERIOD", 10d).WithRules(v => v.Min(0.1));
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
        Tiling = CreateInput(nameof(Tiling), "TILING", false);
        Turbulence = CreateInput(nameof(Turbulence), "TURBULENCE", false);
    }

    protected override void OnPaint(RenderContext context, Canvas target)
    {
        if (Math.Abs(previousScale - Scale.Value) > 0.000001
            || Math.Abs(previousPeriod - Period.Value) > 0.000001
            || previousSeed != Seed.Value
            || previousOctaves != Octaves.Value
            || previousNoiseType != NoiseType.Value
            || previousOffset != Offset.Value
            || previousDimensions != Dimensions.Value
            || previousTiling != Tiling.Value
            || previousTurbulence != Turbulence.Value
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
            previousTiling = Tiling.Value;
            previousTurbulence = Turbulence.Value;
            previousZ = Z.Value;
        }

        RenderNoise(target);
    }

    private void RenderNoise(Canvas workingSurface)
    {
        int saved = workingSurface.Save();
        workingSurface.Translate(-(float)Offset.Value.X, -(float)Offset.Value.Y);
        workingSurface.Translate(-(float)Offset.Value.X, -(float)Offset.Value.Y);
        // workingSurface.DrawPaint(paint);
        workingSurface.DrawRect(workingSurface.LocalClipBounds, paint);
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
        float period = (float)Math.Max(Period.Value, 0.000001f);
        Shader shader = NoiseType.Value switch
        {
            Nodes.NoiseType.TurbulencePerlin => Shader.CreatePerlinNoiseTurbulence(
                freq, freq, octaves, (float)Seed.Value),
            Nodes.NoiseType.FractalPerlin => Shader.CreatePerlinFractalNoise(
                freq, freq,
                octaves, (float)Seed.Value),
            Nodes.NoiseType.Voronoi => GetVoronoiShader(freq, octaves, (float)Seed.Value,
                (int)VoronoiFeature.Value, (float)Randomness.Value, (float)AngleOffset.Value),
            Nodes.NoiseType.FractalValue => GetValueShader(dims, Turbulence.Value, Tiling.Value, period, freq, octaves, (float)Seed.Value, lacunarity, persistence, (float)Z.Value),
            Nodes.NoiseType.FractalPerlin2 => GetPerlinShader(dims, Turbulence.Value, Tiling.Value, period, freq, octaves, (float)Seed.Value, lacunarity, persistence, (float)Z.Value),
            Nodes.NoiseType.FractalVoronoi => GetFractalVoronoiShader(dims, Turbulence.Value, Tiling.Value, freq, octaves, (float)Seed.Value,
                (int)VoronoiFeature.Value, (float)Randomness.Value, (float)AngleOffset.Value, lacunarity, persistence),
            Nodes.NoiseType.FractalSimplexValue => GetSimplexValueShader(dims, Turbulence.Value, Tiling.Value, freq, octaves, (float)Seed.Value, lacunarity, persistence, (float)Z.Value),
            Nodes.NoiseType.FractalSimplexGradient => GetSimplexGradientShader(dims, Turbulence.Value, Tiling.Value, freq, octaves, (float)Seed.Value, lacunarity, persistence, (float)Z.Value),
            
            _ => null
        };

        return shader;
    }

    private const string MainShaderCode =
        """
        NoiseSample noiseSelectorD(float3 p, float freq, int d, float seed, bool tiling, bool turbulence) {
            NoiseSample sample = NoiseSample(0, float3(0));
            if (d == 1) 
                sample = noise1d(p.x, freq, seed, tiling);
            if (d == 2)
                sample = noise2d(p.xy, freq, seed, tiling);
            if (d == 3)
                sample = noise3d(p, freq, seed, tiling);
            if (turbulence) {
                sample.value = abs(sample.value);
            }
            return sample;
        }
        NoiseSample sum(float3 p, float freq, int d, int oct, float lac, float per, float seed, bool tiling, bool turbulence) {
            NoiseSample sum = noiseSelectorD(p, freq, d, seed, tiling, turbulence);
            float amplitude = 1;
            float range = 1;
            for (int o = 1; o<8; o++) {
                if (o>=oct) break;
                freq *= lac;
                amplitude *= per;
                range += amplitude;
                sum = fma(noiseSelectorD(p, freq, d, seed+float(o), tiling, turbulence), amplitude, sum);
            }
            return mul(sum, 1./range);
        }
        
        half4 main(float2 uv)
        {
            NoiseSample s = sum(float3(uv, iZ), iFrequency, iDimensions, iOctaves, iLacunarity, iPersistence, iSeed, iTiling == 1, iTurbulence == 1);
            return half4(s.value,s.derivative/6.+0.5);
        }
        """;

    private const string BaseShaderCode = 
        """
        #version 300
        const float sqr2 = sqrt(2);

        float mod289(const in float x) { return x - floor(x * (1. / 289.)) * 289.; }
        float2 mod289(const in float2 x) { return x - floor(x * (1. / 289.)) * 289.; }
        float3 mod289(const in float3 x) { return x - floor(x * (1. / 289.)) * 289.; }

        float permute(const in float v) { return mod289(((v * 34.0) + 1.0) * v); }
        float2 permute(const in float2 v) { return mod289(((v * 34.0) + 1.0) * v); }
        float3 permute(const in float3 v) { return mod289(((v * 34.0) + 1.0) * v); }
        
        float permute(const in float v, const in float seed) { return permute(v+seed); }
        float2 permute(const in float2 v, const in float seed) { return permute(v+seed); }
        float3 permute(const in float3 v, const in float seed) { return permute(v+seed); }
        
        float permute2(in float2 x) {
          return permute(permute(x.x)+x.y);
        }
        float permute2(in float3 x) {
          return permute(permute2(x.xy)+x.z);
        }
        
        float permute2(in float2 x, const in float seed) {
          return permute(permute(x.x, seed)+x.y);
        }
        float permute2(in float3 x, const in float seed) {
          return permute(permute2(x.xy, seed)+x.z);
        }
        
        float quintic(const in float v) { return v*v*v*(v*(v*6.0-15.0)+10.0); }
        float2  quintic(const in float2 v)  { return v*v*v*(v*(v*6.0-15.0)+10.0); }
        float3  quintic(const in float3 v)  { return v*v*v*(v*(v*6.0-15.0)+10.0); }
        
        float quinticDerivative(const in float v) { return 30.0*v*v*(v*(v-2.0)+1.0); }
        float2  quinticDerivative(const in float2 v)  { return 30.0*v*v*(v*(v-2.0)+1.0); }
        float3  quinticDerivative(const in float3 v)  { return 30.0*v*v*(v*(v-2.0)+1.0); }
        
        //float select(const in float a, const in float b, const in bool t) { return t?b:a; }
        //float2 select(const in float2 a, const in float2 b, const in bool t) { return t?b:a; }
        //float3 select(const in float3 a, const in float3 b, const in bool t) { return t?b:a; }
        //float4 select(const in float4 a, const in float4 b, const in bool t) { return t?b:a; }
        
        //float2 select(const in float2 a, const in float2 b, const in bool2 t) { return float2(select(a.x,b.x,t.x), select(a.y,b.y,t.y)); }
        //float3 select(const in float3 a, const in float3 b, const in bool3 t) { return float3(select(a.xy,b.xy,t.xy), select(a.z,b.z,t.z)); }
        //float4 select(const in float4 a, const in float4 b, const in bool4 t) { return float4(select(a.xy,b.xy,t.xy), select(a.zw,b.zw,t.zw)); }
        
        
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
        
        
        float random(in float x, in float seed) {
          x = permute(x,seed);
          x = fract(x * scale.x);
          return fract(2*pow(x, 2)* pow(33.33 + x, 2));
        }
        float random(in float2 x, in float seed) {
          return random(random(x.x, seed)+x.y);
        }
        float random(in float3 x, in float seed) {
          return random(random(x.xy, seed)+x.z);
        }
        struct NoiseSample {
          float value;
          float3 derivative;
        };
        NoiseSample add(NoiseSample a, NoiseSample b) {
          a.value += b.value;
          a.derivative += b.derivative;
          return a;
        }
        NoiseSample mul(NoiseSample a, float b) {
          a.value *= b;
          a.derivative *= b;
          return a;
        }
        
        NoiseSample fma(NoiseSample a, float b, float c) {
          a.value *= b;
          a.value += c;
          a.derivative *= b;
          return a;
        }
        NoiseSample fma(NoiseSample a, float b, NoiseSample c) {
          a.value *= b;
          a.derivative *= b;
          a.value += c.value;
          a.derivative += c.derivative;
          return a;
        }
        
        uniform float iSeed;
        uniform float iFrequency;
        uniform int iOctaves;
        uniform float iLacunarity;
        uniform float iPersistence;
        uniform int iDimensions;
        uniform float iZ;
        uniform int iTurbulence;
        uniform int iTiling;
        """;

    private const string GradientsCode = 
        """
        const float[2] gradients1D = float[2](1,-1);
        const int gradientsMask1D = 1;
        const float2[8] gradients2D = float2[8](
            float2( 1., 0.),
            float2(-1., 0.),
            float2( 0., 1.),
            float2( 0.,-1.),
            normalize(float2( 1., 1.)),
            normalize(float2(-1., 1.)),
            normalize(float2( 1.,-1.)),
            normalize(float2(-1.,-1.))
        );
        const int gradientsMask2D = 7;

        const float3[16] gradients3D = float3[16](
        	float3( 1., 1., 0.),
        	float3(-1., 1., 0.),
        	float3( 1.,-1., 0.),
        	float3(-1.,-1., 0.),
        	float3( 1., 0., 1.),
        	float3(-1., 0., 1.),
        	float3( 1., 0.,-1.),
        	float3(-1., 0.,-1.),
        	float3( 0., 1., 1.),
        	float3( 0.,-1., 1.),
        	float3( 0., 1.,-1.),
        	float3( 0.,-1.,-1.),
        	
        	float3( 1., 1., 0.),
        	float3(-1., 1., 0.),
        	float3( 0.,-1., 1.),
        	float3( 0.,-1.,-1.)
        );
        const int gradientsMask3D = 15;
        inline float gradients1d(int p) { return gradients1D[p&gradientsMask1D]; }
        inline float gradients1d(float p) { return gradients1d(int(p)); }
        inline float2 gradients2d(int p) { return gradients2D[p&gradientsMask2D]; }
        inline float2 gradients2d(float p) { return gradients2d(int(p)); }
        inline float3 gradients3d(int p) { return gradients3D[p&gradientsMask3D]; }
        inline float3 gradients3d(float p) { return gradients3d(int(p)); }
        """;

    private Shader GetValueShader(int dimensions, bool turbulence, bool tiling, float period, float frequency, int octaves, float seed, float lacunarity, float persistence, float z)
    {
        const string valueShaderCode = BaseShaderCode+ 
                                       """
                                       NoiseSample noise1d(float p, float freq, float seed, bool tiling) {
                                           p *= freq;
                                           float i0 = floor(p);
                                           float t = fract(p);
                                           float i1 = i0+1;
                                           
                                           if(tiling) {
                                               i0 = mod(i0,freq);
                                               i0 = mix(i0, i0 + freq, i0 < 0.);
                                               i1 = mod(i0+1, freq);
                                           }
                                           float h0 = random(i0, seed);
                                           float h1 = random(i1, seed);
                                           float dt = quinticDerivative(t);
                                           t = quintic(t);

                                           float a = h0;
                                           float b = h1-h0;

                                           NoiseSample samp;
                                           samp.value = mix(h0,h1,t);
                                           samp.derivative = (b*dt).x00;
                                           samp.derivative *= freq;
                                           return fma(samp,2,-1);
                                       }
                                       NoiseSample noise2d(float2 p, float freq, float seed, bool tiling) {
                                           p *= freq;
                                           float2 i0 = floor(p);
                                           float2 t = fract(p);
                                           float2 i1 = i0+1;
                                       
                                           if(tiling) {
                                               i0 = mod(i0,freq);
                                               i0 = mix(i0, i0 + freq, lessThan(i0, float2(0.)));
                                               i1 = mod(i0+1, freq);
                                           }
                                           float h00 = random(i0, seed);
                                           float h10 = random(i0.0y+i1.x0, seed);
                                           float h01 = random(i0.x0+i1.0y, seed);
                                           float h11 = random(i1, seed);
                                           float2 dt = quinticDerivative(t);
                                           t = quintic(t);

                                           NoiseSample samp;
                                           samp.value = mix(mix(h00,h10,t.x), mix(h01,h11,t.x),t.y);
                                           samp.derivative = mix(float2(h10,h01)-h00,h11-float2(h01,h10), dt).xy0;
                                           samp.derivative *= freq;
                                           
                                           return fma(samp,2,-1);
                                       }
                                       NoiseSample noise3d(float3 p, float freq, float seed, bool tiling) {
                                           p *= freq;
                                           float3 i0 = floor(p);
                                           float3 t = fract(p);
                                           float3 i1 = i0+1;
                                       
                                           if(tiling) {
                                               i0 = mod(i0,freq);
                                               i0 = mix(i0, i0 + freq, lessThan(i0, float3(0.)));
                                               i1 = mod(i0+1, freq);
                                           }
                                           float h000 = random(i0, seed);
                                           float h100 = random(i0.0yz+i1.x00, seed);
                                           float h010 = random(i0.x0z+i1.0y0, seed);
                                           float h110 = random(i0.00z+i1.xy0, seed);
                                           float h001 = random(i0.xy0+i1.00z, seed);
                                           float h101 = random(i0.0y0+i1.x0z, seed);
                                           float h011 = random(i0.x00+i1.0yz, seed);
                                           float h111 = random(i1, seed);
                                           float3 dt = quinticDerivative(t);
                                           t = quintic(t);

                                           NoiseSample samp;
                                           samp.value = mix(mix(mix(h000,h100, t.x),mix(h010,h110,t.x),t.y),mix(mix(h001,h101,t.x),mix(h011,h111,t.x),t.y),t.z);
                                           samp.derivative.x = mix(mix(h100-h000,h110-h010,t.y), mix(h101-h001,h111-h011,t.y), t.z) * dt.x;
                                           samp.derivative.y = mix(mix(h010-h000,h110-h100,t.x), mix(h011-h001,h111-h101,t.x), t.z) * dt.y;
                                           samp.derivative.z = mix(mix(h001-h000,h101-h100,t.x), mix(h011-h010,h111-h110,t.x), t.y) * dt.z;
                                           
                                           samp.derivative *= freq;
                                           return fma(samp,2,-1);
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
        uniforms.Add("iTiling", new Uniform("iTiling", tiling?1:0));
        uniforms.Add("iPeriod", new Uniform("iPeriod", period));
        uniforms.Add("iTurbulence", new Uniform("iTurbulence", turbulence?1:0));
        uniforms.Add("iZ", new Uniform("iZ", z));

        if (valueShader == null)
        {
            valueShader = Shader.Create(valueShaderCode, uniforms, out var errors);
            if (!string.IsNullOrEmpty(errors))
                Console.WriteLine(errors);
        }
        else
        {
            valueShader = valueShader.WithUpdatedUniforms(uniforms);
        }

        return valueShader;
    }

    private Shader GetPerlinShader(int dimensions, bool turbulence, bool tiling, float period, float frequency, int octaves, float seed, float lacunarity, float persistence, float z)
    {
        const string perlinShaderCode = BaseShaderCode+GradientsCode+
                                        """

                                        NoiseSample noise1d(float p, float freq, float seed, bool tiling) {
                                            p *= freq;
                                            float i0 = floor(p.x);
                                            float t0 = fract(p.x);
                                            float t1 = t0 -1;
                                            float i1 = i0+1;
                                        
                                            if(tiling) {
                                                i0 = mod(i0,freq);
                                                i0 = mix(i0, i0 + freq, i0 < 0.);
                                                i1 = mod(i0+1, freq);
                                            }

                                            float g0 = gradients1d(permute(i0, seed));
                                            float g1 = gradients1d(permute(i1, seed));

                                            float v0 = g0*t0;
                                            float v1 = g1*t1;
                                            float dt = quinticDerivative(t0);
                                            float t = quintic(t0);
                                            
                                            NoiseSample samp;
                                            samp.value =  mix(v0,v1,t);
                                            samp.derivative = mix(g0,g1,t).x00;
                                            samp.derivative.x += (v1-v0)*dt;
                                            return mul(samp, 2);
                                        }

                                        NoiseSample noise2d(float2 p, float freq, float seed, bool tiling) {
                                            p *= freq;
                                            float2 i0 = floor(p);
                                            float2 t0 = fract(p);
                                            float2 t1 = t0-1;
                                            float2 i1 = i0+1;
                                        
                                            if(tiling) {
                                                i0 = mod(i0,freq);
                                                i0 = mix(i0, i0 + freq, lessThan(i0, float2(0.)));
                                                i1 = mod(i0+1, freq);
                                            }

                                            float2 g00 = gradients2d(permute2(i0, seed));
                                            float2 g10 = gradients2d(permute2(i0.0y+i1.x0, seed));
                                            float2 g01 = gradients2d(permute2(i0.x0+i1.0y, seed));
                                            float2 g11 = gradients2d(permute2(i1, seed));

                                            float v00 = dot(g00, t0);
                                            float v10 = dot(g10, t0.0y+t1.x0);
                                            float v01 = dot(g01, t0.x0+t1.0y);
                                            float v11 = dot(g11, t1);
                                            float2 dt = quinticDerivative(t0);
                                            float2 t = quintic(t0);
                                            NoiseSample samp;
                                            samp.value = mix(mix(v00,v10,t.x),mix(v01,v11,t.x),t.y);
                                            samp.derivative = mix(mix(g00,g10,t.x),mix(g01,g11,t.x),t.y).xy0;
                                            samp.derivative.xy += mix(float2(v10,v01)-v00,v11-float2(v01,v10), dt);
                                            
                                            samp.derivative *= freq;
                                            return mul(samp, sqr2);
                                        }
                                        NoiseSample noise3d(float3 p, float freq, float seed, bool tiling) {
                                            p *= freq;
                                            float3 i0 = floor(p);
                                            float3 t0 = fract(p);
                                            float3 i1 = i0+1;
                                            float3 t1 = t0 -1;
                                        
                                            if(tiling) {
                                                i0 = mod(i0,freq);
                                                i0 = mix(i0, i0 + freq, lessThan(i0, float3(0.)));
                                                i1 = mod(i0+1, freq);
                                            }
                                            
                                            float3 g000 = gradients3d(random(i0)*255);
                                            float3 g100 = gradients3d(random(i0.0yz+i1.x00)*255);
                                            float3 g010 = gradients3d(random(i0.x0z+i1.0y0)*255);
                                            float3 g110 = gradients3d(random(i0.00z+i1.xy0)*255);
                                            float3 g001 = gradients3d(random(i0.xy0+i1.00z)*255);
                                            float3 g101 = gradients3d(random(i0.0y0+i1.x0z)*255);
                                            float3 g011 = gradients3d(random(i0.x00+i1.0yz)*255);
                                            float3 g111 = gradients3d(random(i1)*255);

                                            float v000 = dot(g000, t0);
                                            float v100 = dot(g100, t0.0yz+t1.x00);
                                            float v010 = dot(g010, t0.x0z+t1.0y0);
                                            float v110 = dot(g110, t0.00z+t1.xy0);
                                            float v001 = dot(g001, t0.xy0+t1.00z);
                                            float v101 = dot(g101, t0.0y0+t1.x0z);
                                            float v011 = dot(g011, t0.x00+t1.0yz);
                                            float v111 = dot(g111, t1);
                                            float3 dt = quinticDerivative(t0);
                                            float3 t = quintic(t0);
                                            
                                            NoiseSample samp;
                                            samp.value = mix(mix(mix(v000,v100, t.x),mix(v010,v110,t.x),t.y),mix(mix(v001,v101,t.x),mix(v011,v111,t.x),t.y),t.z);
                                            samp.derivative = mix(mix(mix(g000,g100, t.x), mix(g010,g110,t.x), t.y), mix(mix(g001,g101,t.x),mix(g011,g111,t.x),t.y),t.z);
                                            samp.derivative.x += mix(mix(v100-v000,v110-v010,t.y), mix(v101-v001,v111-v011,t.y), t.z) * dt.x;
                                            samp.derivative.y += mix(mix(v010-v000,v110-v100,t.x), mix(v011-v001,v111-v101,t.x), t.z) * dt.y;
                                            samp.derivative.z += mix(mix(v001-v000,v101-v100,t.x), mix(v011-v010,v111-v110,t.x), t.y) * dt.z;
                                        
                                            
                                            samp.derivative *= freq;
                                            return samp;
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
        
        uniforms.Add("iTiling", new Uniform("iTiling", tiling?1:0));
        uniforms.Add("iPeriod", new Uniform("iPeriod", period));
        uniforms.Add("iTurbulence", new Uniform("iTurbulence", turbulence?1:0));
        uniforms.Add("iZ", new Uniform("iZ", z));
        if (perlinShader == null)
        {
            perlinShader = Shader.Create(perlinShaderCode, uniforms, out var errors);
            if (!string.IsNullOrEmpty(errors))
                Console.WriteLine(errors);
        }
        else
        {
            perlinShader = perlinShader.WithUpdatedUniforms(uniforms);
        }

        return perlinShader;
    }

     private Shader GetSimplexGradientShader(int dimensions, bool turbulence, bool tiling, float frequency, int octaves, float seed, float lacunarity, float persistence, float z)
    {
        const string simplexGradientShaderCode = BaseShaderCode+GradientsCode+ 
                                       """
                                       
                                       const float2 squaresToTriangles = float2((3-sqrt(3))/6.);
                                       const float2 trianglesToSquares = float2((sqrt(3)-1)/2.);
                                       
                                       const float3[32] simplexGradients3D = float3[32](
                                       	normalize(float3( 1., 1., 0.)),
                                       	normalize(float3(-1., 1., 0.)),
                                       	normalize(float3( 1.,-1., 0.)),
                                       	normalize(float3(-1.,-1., 0.)),
                                       	normalize(float3( 1., 0., 1.)),
                                       	normalize(float3(-1., 0., 1.)),
                                       	normalize(float3( 1., 0.,-1.)),
                                       	normalize(float3(-1., 0.,-1.)),
                                       	normalize(float3( 0., 1., 1.)),
                                       	normalize(float3( 0.,-1., 1.)),
                                       	normalize(float3( 0., 1.,-1.)),
                                       	normalize(float3( 0.,-1.,-1.)),
                                       	
                                       	normalize(float3( 1., 1., 0.)),
                                       	normalize(float3(-1., 1., 0.)),
                                       	normalize(float3( 1.,-1., 0.)),
                                       	normalize(float3(-1.,-1., 0.)),
                                       	normalize(float3( 1., 0., 1.)),
                                       	normalize(float3(-1., 0., 1.)),
                                       	normalize(float3( 1., 0.,-1.)),
                                       	normalize(float3(-1., 0.,-1.)),
                                       	normalize(float3( 0., 1., 1.)),
                                       	normalize(float3( 0.,-1., 1.)),
                                       	normalize(float3( 0., 1.,-1.)),
                                       	normalize(float3( 0.,-1.,-1.)),
                                       	
                                       	normalize(float3( 1., 1., 1.)),
                                       	normalize(float3(-1., 1., 1.)),
                                       	normalize(float3( 1.,-1., 1.)),
                                       	normalize(float3(-1.,-1., 1.)),
                                       	normalize(float3( 1., 1.,-1.)),
                                       	normalize(float3(-1., 1.,-1.)),
                                       	normalize(float3( 1.,-1.,-1.)),
                                       	normalize(float3(-1.,-1.,-1.))
                                       );
                                       const int simplexGradientsMask3D = 31;
                                       inline float3 simplexGradients3d(int p) { return simplexGradients3D[p&simplexGradientsMask3D]; }
                                       inline float3 simplexGradients3d(float p) { return simplexGradients3d(int(p)); }
                                       
                                       NoiseSample simplexGradient1dPart(float p, float i, float seed) {
                                           float x = p-i;
                                           
                                           float f = 1-x*x;
                                           float f2 = f*f;
                                           float f3 = f*f2;
                                           float g = gradients1d(int(permute(i, seed)));
                                           float v = g*x;
                                           NoiseSample samp;
                                           samp.value = v*f3;
                                           samp.derivative.x = g*f3-6.*v*x*f2;
                                           return samp;
                                       }
                                       NoiseSample noise1d(float p, float freq, float seed, bool tiling) {
                                           p *= freq;
                                           float i = floor(p);
                                           NoiseSample samp = simplexGradient1dPart(p,i, seed);
                                           samp = add(samp, simplexGradient1dPart(p,i+1, seed));
                                           samp.derivative *= freq;
                                           return mul(samp,64./27.);
                                       }
                                       const float simplexScale2D = 2916.* sqr2 / 125.;
                                       
                                       NoiseSample simplexGradient2dPart(float2 p, float2 i, float seed) {
                                           float unskew = dot(i,squaresToTriangles);
                                           float2 x = p-i+unskew;
                                           float f = 0.5-dot(-x,x);
                                       
                                           NoiseSample samp = NoiseSample(0,float3(0));
                                           if(f>0) {
                                               float f2 = f*f;
                                               float f3 = f*f2;
                                               float2 g = gradients2d(int(permute2(i, seed)));
                                               float v = dot(g,x);
                                               float v6f2 = -6. * v * f2;
                                               samp.value = v*f3;
                                               samp.derivative.xy = g*f3+v6f2*x;
                                           }
                                           return samp;
                                       }
                                       
                                       NoiseSample noise2d(float2 p, float freq, float seed, bool tiling) {
                                           p *= freq;
                                           float skew = dot(p,trianglesToSquares);
                                           float2 s = p+skew;
                                           float2 i = floor(s);
                                           NoiseSample samp = simplexGradient2dPart(p,i,seed);
                                           samp = add(samp, simplexGradient2dPart(p,i+1,seed));
                                           if(s.x - i.x >= s.y - i.y) {
                                               samp = add(samp,simplexGradient2dPart(p,float2(i.x+1,i.y),seed));
                                           } else {
                                               samp = add(samp,simplexGradient2dPart(p,float2(i.x,i.y+1), seed));
                                           }
                                           samp.derivative *= freq;
                                           return mul(samp,simplexScale2D);
                                       }
                                       
                                       
                                       NoiseSample simplexGradient3dPart(float3 p, float3 i, float seed) {
                                           float unskew = dot(i,float3(1./6.));
                                           float3 x = p-i+unskew;
                                           float f = 0.5+dot(-x,x);
                                       
                                           NoiseSample samp = NoiseSample(0,float3(0));
                                           if(f>0) {
                                               float f2 = f*f;
                                               float f3 = f*f2;
                                               float3 g = simplexGradients3d(int(permute2(i, seed)));
                                               float v = dot(g,x);
                                               float v6f2 = -6. * v * f2;
                                               samp.value = v*f3;
                                               samp.derivative = g*f3+v6f2*x;
                                           }
                                           return samp;
                                       }
                                       const float simplexScale3D = 8192. * sqrt(3) / 375.;
                                       NoiseSample noise3d(float3 p, float freq, float seed, bool tiling) {
                                           p *= freq;
                                           float skew = dot(p,float3(1./3.));
                                           float3 s = p+skew;
                                           float3 i = floor(s);
                                           float3 x = s-i;
                                           NoiseSample samp = simplexGradient3dPart(p,i, seed);
                                           samp = add(samp, simplexGradient3dPart(p,i+1, seed));
                                           if(x.x >= x.y) {
                                               if(x.x>=x.z) {
                                                   samp = add(samp,simplexGradient3dPart(p,i+float3(1,0,0), seed));
                                                   if(x.y>=x.z) {
                                                       samp = add(samp, simplexGradient3dPart(p,i+float3(1,1,0), seed));
                                                   }
                                                   else {
                                                       samp = add(samp, simplexGradient3dPart(p,i+float3(1,0,1), seed));
                                                   }
                                               }
                                               else {
                                                   samp = add(samp,simplexGradient3dPart(p,i+float3(0,0,1), seed));
                                                   samp = add(samp,simplexGradient3dPart(p,i+float3(1,0,1), seed));
                                               }
                                           } else {
                                               if(x.y>=x.z) {
                                                   samp = add(samp,simplexGradient3dPart(p,i+float3(0,1,0), seed));
                                                   if(x.x>=x.z) {
                                                       samp = add(samp, simplexGradient3dPart(p,i+float3(1,1,0), seed));
                                                   }
                                                   else {
                                                       samp = add(samp, simplexGradient3dPart(p,i+float3(0,1,1), seed));
                                                   }
                                               }
                                               else {
                                                   samp = add(samp,simplexGradient3dPart(p,i+float3(0,0,1), seed));
                                                   samp = add(samp,simplexGradient3dPart(p,i+float3(0,1,1), seed));
                                               }
                                           }
                                           samp.derivative *= freq;
                                           return mul(samp,simplexScale3D);
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
        
        uniforms.Add("iTiling", new Uniform("iTiling", tiling?1:0));
        uniforms.Add("iTurbulence", new Uniform("iTurbulence", turbulence?1:0));
        uniforms.Add("iZ", new Uniform("iZ", z));

        if (simplexGradientShader == null)
        {
            simplexGradientShader = Shader.Create(simplexGradientShaderCode, uniforms, out var errors);
            if (!string.IsNullOrEmpty(errors))
                Console.WriteLine(errors);
        }
        else
        {
            simplexGradientShader = simplexGradientShader.WithUpdatedUniforms(uniforms);
        }

        return simplexGradientShader;
    }

     private Shader GetSimplexValueShader(int dimensions, bool turbulence, bool tiling, float frequency, int octaves, float seed, float lacunarity, float persistence, float z)
    {
        string simplexValueShaderCode = BaseShaderCode+ 
                                              """
                                              
                                              const float2 squaresToTriangles = float2((3-sqrt(3))/6.);
                                              const float2 trianglesToSquares = float2((sqrt(3)-1)/2.);
                                              
                                              NoiseSample simplexValue1dPart(float p, float i, float seed) {
                                                  float x = p-i;
                                                  float f = 1-x*x;
                                                  float f2 = f*f;
                                                  float f3 = f*f2;
                                                  float h = random(i, seed);
                                                  NoiseSample samp;
                                                  samp.value = h*f3;
                                                  samp.derivative.x = -6.*h*x*f2;
                                                  return samp;
                                              }

                                              NoiseSample noise1d(float p, float freq, float seed, bool tiling) {
                                                  p *= freq;
                                                  float i = floor(p);
                                                  NoiseSample samp = simplexValue1dPart(p,i, seed);
                                                  samp = add(samp, simplexValue1dPart(p,i+1, seed));
                                                  samp.derivative *= freq;
                                                  return fma(samp,2,-1);
                                              }
                                              
                                              NoiseSample simplexValue2dPart(float2 p, float2 i, float seed) {
                                                  float unskew = dot(i,squaresToTriangles);
                                                  float2 x = p-i+unskew;
                                                  float f = 0.5+dot(-x,x);
                                              
                                                  NoiseSample samp = NoiseSample(0,float3(0));
                                                  if(f>0) {
                                                      float f2 = f*f;
                                                      float f3 = f*f2;
                                                      float h = random(i, seed);
                                                      float h6f2 = -6. * h * f2;
                                                      samp.value = h*f3;
                                                      samp.derivative.xy = h6f2*x;
                                                  }
                                                  return samp;
                                              }
                                              
                                              NoiseSample noise2d(float2 p, float freq, float seed, bool tiling) {
                                                  p *= freq;
                                                  float skew = dot(p,trianglesToSquares);
                                                  float2 s = p+skew;
                                                  float2 i = floor(s);
                                                  NoiseSample samp = simplexValue2dPart(p,i,seed);
                                                  samp = add(samp, simplexValue2dPart(p,i+1,seed));
                                                  if(s.x - i.x >= s.y - i.y)
                                                      samp = add(samp,simplexValue2dPart(p,i+float2(1,0),seed));
                                                  else 
                                                      samp = add(samp,simplexValue2dPart(p,i+float2(0,1),seed));
                                                  
                                                  samp.derivative *= freq;
                                                  return fma(samp,8*2,-1);
                                              }
                                              NoiseSample simplexValue3dPart(float3 p, float3 i, float seed) {
                                                  float unskew = dot(i,float3(1./6.));
                                                  float3 x = p-i+unskew;
                                                  float f = 0.5+dot(-x,x);
                                              
                                                  NoiseSample samp = NoiseSample(0,float3(0));
                                                  if(f>0) {
                                                      float f2 = f*f;
                                                      float f3 = f*f2;
                                                      float h = random(i, seed);
                                                      float h6f2 = -6. * h * f2;
                                                      samp.value = h*f3;
                                                      samp.derivative = h6f2*x;
                                                  }
                                                  return samp;
                                              }
                                              
                                              NoiseSample noise3d(float3 p, float freq, float seed, bool tiling) {
                                                  p *= freq;
                                                  float skew = dot(p,float3(1./3.));
                                                  float3 s = p+skew;
                                                  float3 i = floor(s);
                                                  float3 x = s-i;
                                                  NoiseSample samp = simplexValue3dPart(p,i, seed);
                                                  samp = add(samp, simplexValue3dPart(p,i+1, seed));
                                                  if(x.x >= x.y) {
                                                      if(x.x>=x.z) {
                                                          samp = add(samp,simplexValue3dPart(p,i+float3(1,0,0), seed));
                                                          if(x.y>=x.z)
                                                          samp = add(samp, simplexValue3dPart(p,i+float3(1,1,0), seed));
                                                          else
                                                          samp = add(samp, simplexValue3dPart(p,i+float3(1,0,1), seed));
                                                      }
                                                      else {
                                                          samp = add(samp,simplexValue3dPart(p,i+float3(0,0,1), seed));
                                                          samp = add(samp,simplexValue3dPart(p,i+float3(1,0,1), seed));
                                                      }
                                                  } else {
                                                      if(x.y>=x.z) {
                                                          samp = add(samp,simplexValue3dPart(p,i+float3(0,1,0), seed));
                                                          if(x.x>=x.z)
                                                          samp = add(samp, simplexValue3dPart(p,i+float3(1,1,0), seed));
                                                          else
                                                          samp = add(samp, simplexValue3dPart(p,i+float3(0,1,1), seed));
                                                      }
                                                      else {
                                                          samp = add(samp,simplexValue3dPart(p,i+float3(0,0,1), seed));
                                                          samp = add(samp,simplexValue3dPart(p,i+float3(0,1,1), seed));
                                                      }
                                                  }
                                                  samp.derivative *= freq;
                                                  return fma(samp,8*2,-1);
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
        
        uniforms.Add("iTiling", new Uniform("iTiling", tiling?1:0));
        uniforms.Add("iTurbulence", new Uniform("iTurbulence", turbulence?1:0));
        uniforms.Add("iZ", new Uniform("iZ", z));

        if (simplexValueShader == null)
        {
            simplexValueShader = Shader.Create(simplexValueShaderCode, uniforms, out var errors);
            if (!string.IsNullOrEmpty(errors))
                Console.WriteLine(errors);
        }
        else
        {
            simplexValueShader = simplexValueShader.WithUpdatedUniforms(uniforms);
        }

        return simplexValueShader;
    }
     
    private Shader GetFractalVoronoiShader(int dimensions, bool turbulence, bool tiling, float frequency, int octaves, float seed, int feature, float randomness,
        float angleOffset, float lacunarity, float persistence)
    {
        string voronoiShaderCode = BaseShaderCode+
                                   """
                                   uniform float iRandomness;
                                   uniform int iFeature;
                                   uniform float iAngleOffset;
                                   const float2 squaresToTriangles = float2((3-sqrt(3))/6.);
                                   const float2 trianglesToSquares = float2((sqrt(3)-1)/2.);
                                   
                                   NoiseSample simplexValue1dPart(float p, float i, float seed) {
                                       float x = p-i;
                                       float f = 1-x*x;
                                       float f2 = f*f;
                                       float f3 = f*f2;
                                       float h = random(i, seed);
                                       NoiseSample samp;
                                       samp.value = h*f3;
                                       samp.derivative.x = -6.*h*x*f2;
                                       return samp;
                                   }
                                   
                                   NoiseSample noise1d(float p, float freq, float seed, bool tiling) {
                                       p *= freq;
                                       float i = floor(p);
                                       NoiseSample samp = simplexValue1dPart(p,i, seed);
                                       samp = add(samp, simplexValue1dPart(p,i+1, seed));
                                       samp.derivative *= freq;
                                       return fma(samp,2,-1);
                                   }
                                   
                                   NoiseSample simplexValue2dPart(float2 p, float2 i, float seed) {
                                       float unskew = dot(i,squaresToTriangles);
                                       float2 x = p-i+unskew;
                                       float f = 0.5+dot(-x,x);
                                   
                                       NoiseSample samp = NoiseSample(0,float3(0));
                                       if(f>0) {
                                           float f2 = f*f;
                                           float f3 = f*f2;
                                           float h = random(i, seed);
                                           float h6f2 = -6. * h * f2;
                                           samp.value = h*f3;
                                           samp.derivative.xy = h6f2*x;
                                       }
                                       return samp;
                                   }
                                   
                                   NoiseSample noise2d(float2 p, float freq, float seed, bool tiling) {
                                       p *= freq;
                                       float skew = dot(p,trianglesToSquares);
                                       float2 s = p+skew;
                                       float2 i = floor(s);
                                       NoiseSample samp = simplexValue2dPart(p,i,seed);
                                       samp = add(samp, simplexValue2dPart(p,i+1,seed));
                                       if(s.x - i.x >= s.y - i.y)
                                           samp = add(samp,simplexValue2dPart(p,i+float2(1,0),seed));
                                       else 
                                           samp = add(samp,simplexValue2dPart(p,i+float2(0,1),seed));
                                       
                                       samp.derivative *= freq;
                                       return fma(samp,8*2,-1);
                                   }
                                   NoiseSample simplexValue3dPart(float3 p, float3 i, float seed) {
                                       float unskew = dot(i,float3(1./6.));
                                       float3 x = p-i+unskew;
                                       float f = 0.5+dot(-x,x);
                                   
                                       NoiseSample samp = NoiseSample(0,float3(0));
                                       if(f>0) {
                                           float f2 = f*f;
                                           float f3 = f*f2;
                                           float h = random(i, seed);
                                           float h6f2 = -6. * h * f2;
                                           samp.value = h*f3;
                                           samp.derivative = h6f2*x;
                                       }
                                       return samp;
                                   }
                                   
                                   NoiseSample noise3d(float3 p, float freq, float seed, bool tiling) {
                                       p *= freq;
                                       float skew = dot(p,float3(1./3.));
                                       float3 s = p+skew;
                                       float3 i = floor(s);
                                       float3 x = s-i;
                                       NoiseSample samp = simplexValue3dPart(p,i, seed);
                                       samp = add(samp, simplexValue3dPart(p,i+1, seed));
                                       if(x.x >= x.y) {
                                           if(x.x>=x.z) {
                                               samp = add(samp,simplexValue3dPart(p,i+float3(1,0,0), seed));
                                               if(x.y>=x.z)
                                               samp = add(samp, simplexValue3dPart(p,i+float3(1,1,0), seed));
                                               else
                                               samp = add(samp, simplexValue3dPart(p,i+float3(1,0,1), seed));
                                           }
                                           else {
                                               samp = add(samp,simplexValue3dPart(p,i+float3(0,0,1), seed));
                                               samp = add(samp,simplexValue3dPart(p,i+float3(1,0,1), seed));
                                           }
                                       } else {
                                           if(x.y>=x.z) {
                                               samp = add(samp,simplexValue3dPart(p,i+float3(0,1,0), seed));
                                               if(x.x>=x.z)
                                               samp = add(samp, simplexValue3dPart(p,i+float3(1,1,0), seed));
                                               else
                                               samp = add(samp, simplexValue3dPart(p,i+float3(0,1,1), seed));
                                           }
                                           else {
                                               samp = add(samp,simplexValue3dPart(p,i+float3(0,0,1), seed));
                                               samp = add(samp,simplexValue3dPart(p,i+float3(0,1,1), seed));
                                           }
                                       }
                                       samp.derivative *= freq;
                                       return fma(samp,8*2,-1);
                                   }
                                   """
                                   + MainShaderCode;

        Uniforms uniforms = new Uniforms();
        uniforms.Add("iSeed", new Uniform("iSeed", seed));
        uniforms.Add("iFrequency", new Uniform("iFrequency", frequency));
        uniforms.Add("iOctaves", new Uniform("iOctaves", octaves));
        uniforms.Add("iRandomness", new Uniform("iRandomness", randomness));
        uniforms.Add("iFeature", new Uniform("iFeature", feature));
        uniforms.Add("iAngleOffset", new Uniform("iAngleOffset", angleOffset));
        uniforms.Add("iLacunarity", new Uniform("iLacunarity", lacunarity));
        uniforms.Add("iPersistence", new Uniform("iPersistence", persistence));
        uniforms.Add("iDimensions", new Uniform("iDimensions", dimensions));
        
        uniforms.Add("iTiling", new Uniform("iTiling", tiling?1:0));
        uniforms.Add("iTurbulence", new Uniform("iTurbulence", turbulence?1:0));

        if (voronoiShader == null)
        {
            voronoiShader = Shader.Create(voronoiShaderCode, uniforms, out var errors);
            if (!string.IsNullOrEmpty(errors))
                Console.WriteLine(errors);
        }
        else
        {
            voronoiShader = voronoiShader.WithUpdatedUniforms(uniforms);
        }

        return voronoiShader;
    }
    private Shader GetVoronoiShader(float frequency, int octaves, float seed, int feature, float randomness,
        float angleOffset)
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

        if (voronoi2Shader == null)
        {
            voronoi2Shader = Shader.Create(voronoiShaderCode, uniforms, out var errors);
            if (!string.IsNullOrEmpty(errors))
                Console.WriteLine(errors);
        }
        else
        {
            voronoi2Shader = voronoi2Shader.WithUpdatedUniforms(uniforms);
        }

        return voronoi2Shader;
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
