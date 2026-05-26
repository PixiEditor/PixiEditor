using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.ColorSpaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("ColorRampNode")]
public class ColorRampNode : Node
{
    public InputProperty<Texture> Fac { get; }
    public InputProperty<int> StopsCount { get; }
    public InputProperty<Color[]> Colors { get; }
    public InputProperty<float[]> Offsets { get; }
    public InputProperty<ColorSpaceType> ColorSpace { get; }
    public OutputProperty<Texture> Image { get; }
    public OutputProperty<Texture> Alpha { get; }

    public Dictionary<InputProperty<Color>, InputProperty<float>> ColorStops { get; } = new();

    private TextureCache textureCache = new();

    protected override bool ExecuteOnlyOnCacheChange => true;
    protected override CacheTriggerFlags CacheTrigger => CacheTriggerFlags.Inputs;

    private string shaderCode = """
                                uniform shader iImage;
                                uniform shader iGradient;
                                uniform int iMode;

                                half4 main(float2 uv) {
                                    half factor = clamp(dot(iImage.eval(uv).rgb, half3(0.299, 0.587, 0.114)), 0.0, 1.0);
                                    half4 ramp = iGradient.eval(float2(factor, 0.0));
                                    if (iMode == 1) {
                                        return half4(ramp.a, ramp.a, ramp.a, 1.0);
                                    }
                                    return ramp;
                                }
                                """;

    public ColorRampNode()
    {
        Fac = CreateInput<Texture>(nameof(Fac), "FAC", null);
        Colors = CreateInput<Color[]>("Colors", "COLORS", null);
        Offsets = CreateInput<float[]>("Offsets", "OFFSETS", null);
        StopsCount = CreateInput<int>("StopsCount", "STOPS_COUNT", 2)
            .NonOverridenChanged(_ => RegenerateStops());

        Colors.ConnectionChanged += OnColorsConnected;
        Offsets.ConnectionChanged += OnOffsetsConnected;

        ColorSpace = CreateInput("ColorSpace", "COLOR_SPACE", ColorSpaceType.Inherit);

        Image = CreateOutput<Texture>(nameof(Image), "IMAGE", null);
        Alpha = CreateOutput<Texture>(nameof(Alpha), "ALPHA", null);

        GenerateStops();
    }

    protected override void OnExecute(RenderContext context)
    {
        if (context == null || Fac.Value == null) { return; }

        var finalColorStops = new List<GradientStop>();
        if (Colors.Value != null && Offsets.Value != null && Colors.Value.Length == Offsets.Value.Length)
        {
            for (int i = 0; i < Math.Min(Colors.Value.Length, Offsets.Value.Length); i++)
            {
                finalColorStops.Add(new GradientStop(Colors.Value[i], Offsets.Value[i]));
            }
        }
        else
        {
            finalColorStops = ColorStops.Select(kvp => new GradientStop(kvp.Key.Value, kvp.Value.Value)).ToList();
        }
        if (finalColorStops.Count < 2) { return; }

        Color[] colors = finalColorStops.Select(x => x.Color).ToArray();
        float[] offsets = finalColorStops.Select(x => (float)x.Offset).ToArray();
        VecI size = Fac.Value.Size;
        var colorSpace = ColorSpace.Value == ColorSpaceType.Inherit
            ? context.ProcessingColorSpace
            : (ColorSpace.Value == ColorSpaceType.Srgb
                ? Drawie.Backend.Core.Surfaces.ImageData.ColorSpace.CreateSrgb()
                : Drawie.Backend.Core.Surfaces.ImageData.ColorSpace.CreateSrgbLinear());

        using var snapshot = Fac.Value.DrawingSurface.Snapshot();
        using Shader imageShader = snapshot.ToShader();
        using Shader gradientShader = Shader.CreateLinearGradient(new VecD(0, 0), new VecD(1, 0), colors, offsets, Matrix3X3.Identity);

        Image.Value = DrawRamp(0, size, colorSpace, imageShader, gradientShader);
        Alpha.Value = DrawRamp(1, size, colorSpace, imageShader, gradientShader);
    }

    private Texture DrawRamp(int id, VecI size, ColorSpace colorSpace, Shader imageShader, Shader gradientShader)
    {
        Texture texture = textureCache.RequestTexture(id, size, colorSpace);
        Uniforms uniforms = new Uniforms();
        uniforms.Add("iImage", new Uniform("iImage", imageShader));
        uniforms.Add("iGradient", new Uniform("iGradient", gradientShader));
        uniforms.Add("iMode", new Uniform("iMode", id));
        using Shader shader = Shader.Create(shaderCode, uniforms, out _);
        using Paint paint = new() { BlendMode = BlendMode.Src };
        paint.Shader = shader;
        texture.DrawingSurface.Canvas.DrawRect(0, 0, size.X, size.Y, paint);
        return texture;
    }

    private void OnColorsConnected()
    {
        if (Colors.Connection != null)
        {
            foreach (var kvp in ColorStops)
            {
                RemoveInputProperty(kvp.Key);
                RemoveInputProperty(kvp.Value);
            }
            RemoveInputProperty(StopsCount);
            ColorStops.Clear();
        }
        else
        {
            RegenerateStops();
        }
    }

    private void OnOffsetsConnected()
    {
        if (Offsets.Connection != null)
        {
            foreach (var kvp in ColorStops)
            {
                RemoveInputProperty(kvp.Key);
                RemoveInputProperty(kvp.Value);
            }
            RemoveInputProperty(StopsCount);
            ColorStops.Clear();
        }
        else
        {
            RegenerateStops();
        }
    }

    private void RegenerateStops()
    {
        if (StopsCount.Value < ColorStops.Count)
        {
            int diff = ColorStops.Count - StopsCount.Value;
            var keysToRemove = ColorStops.Keys.TakeLast(diff).ToList();
            foreach (var key in keysToRemove)
            {
                RemoveInputProperty(key);
                RemoveInputProperty(ColorStops[key]);
                ColorStops.Remove(key);
            }
        }
        GenerateStops();
    }

    private void GenerateStops()
    {
        if (Colors.Connection != null || Offsets.Connection != null) return;
        if (!InputProperties.Contains(StopsCount))
        {
            AddInputProperty(StopsCount);
        }
        int startIndex = ColorStops.Count;
        for (int i = startIndex; i < StopsCount.Value; i++)
        {
            var colorInput = CreateInput<Color>($"ColorStopColor_{i + 1}", "COLOR_STOP_COLOR",
                Drawie.Backend.Core.ColorsImpl.Colors.White);
            var positionInput = CreateInput<float>($"ColorStopPosition_{i + 1}", $"COLOR_STOP_POSITION",
                i / (float)(StopsCount.Value - 1));
            ColorStops[colorInput] = positionInput;
        }
    }

    public override Node CreateCopy()
    {
        return new ColorRampNode();
    }

    public override void Dispose()
    {
        base.Dispose();
        textureCache.Dispose();
    }
}
