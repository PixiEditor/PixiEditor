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
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("ColorRampNode")]
public class ColorRampNode : Node
{
    public InputProperty<Texture> Fac { get; }
    public InputProperty<Paintable> Gradient { get; }
    public InputProperty<ColorSpaceType> ColorSpace { get; }
    public OutputProperty<Texture> Image { get; }

    private TextureCache textureCache = new();

    protected override bool ExecuteOnlyOnCacheChange => true;
    protected override CacheTriggerFlags CacheTrigger => CacheTriggerFlags.Inputs;

    private string shaderCode = """
                                uniform shader iImage;
                                uniform shader iGradient;

                                half4 main(float2 uv) {
                                    half4 src = iImage.eval(uv);
                                    half factor = clamp(dot(src.rgb, half3(0.299, 0.587, 0.114)), 0.0, 1.0);
                                    half4 ramp = iGradient.eval(float2(factor, 0.0));
                                    return ramp * src.a;
                                }
                                """;

    public ColorRampNode()
    {
        Fac = CreateInput<Texture>("Fac", "FAC", null);
        Gradient = CreateInput<Paintable>("Gradient", "GRADIENT", new ColorPaintable(Colors.Transparent));
        ColorSpace = CreateInput("ColorSpace", "COLOR_SPACE", ColorSpaceType.Inherit);

        Image = CreateOutput<Texture>("Image", "IMAGE", null);
    }

    protected override void OnExecute(RenderContext context)
    {
        if (context == null || Fac.Value == null) { return; }

        var finalColorStops = new List<GradientStop>();
        if (Gradient.Value is GradientPaintable gradient)
        {
            finalColorStops = gradient.GradientStops.ToList();
        }
        else if (Gradient.Value is ColorPaintable solid)
        {
            if (solid.AnythingVisible)
            {
                finalColorStops.Add(new GradientStop(solid.Color, 0));
                finalColorStops.Add(new GradientStop(solid.Color, 1));
            }
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
        using Shader gradientShader = Shader.CreateLinearGradient(VecD.Zero, new VecD(1, 0), colors, offsets, Matrix3X3.Identity);

        Texture texture = textureCache.RequestTexture(0, size, colorSpace);
        Uniforms uniforms = new Uniforms();
        uniforms.Add("iImage", new Uniform("iImage", imageShader));
        uniforms.Add("iGradient", new Uniform("iGradient", gradientShader));
        using Shader shader = Shader.Create(shaderCode, uniforms, out _);
        using Paint paint = new() { BlendMode = BlendMode.Src };
        paint.Shader = shader;
        texture.DrawingSurface.Canvas.DrawRect(0, 0, size.X, size.Y, paint);

        Image.Value = texture;
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
