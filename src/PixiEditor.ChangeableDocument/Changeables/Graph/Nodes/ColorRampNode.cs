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
public class ColorRampNode : RenderNode, IRenderInput
{
    public RenderInputProperty Background { get; }
    public InputProperty<Paintable> Gradient { get; }
    public InputProperty<ColorSpaceType> ColorSpace { get; }

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
        Background = CreateRenderInput("Fac", "COLOR_RAMP_FACTOR");
        Gradient = CreateInput<Paintable>("Gradient", "GRADIENT", new LinearGradientPaintable(VecD.Zero, new VecD(1, 0),
            [new GradientStop(Colors.White, 0), new GradientStop(Colors.Black, 1)]));
        ColorSpace = CreateInput("ColorSpace", "COLOR_SPACE", ColorSpaceType.Inherit);

        Output.FirstInChain = null;
    }

    protected override void OnPaint(RenderContext context, Canvas surface)
    {
        if (context == null || Background.Value == null) { return; }

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

        VecI size = context.RenderOutputSize;
        if (size.X <= 0 || size.Y <= 0) { return; }

        var colorSpace = ColorSpace.Value == ColorSpaceType.Inherit
            ? context.ProcessingColorSpace
            : (ColorSpace.Value == ColorSpaceType.Srgb
                ? Drawie.Backend.Core.Surfaces.ImageData.ColorSpace.CreateSrgb()
                : Drawie.Backend.Core.Surfaces.ImageData.ColorSpace.CreateSrgbLinear());

        using var source = Texture.ForProcessing(size, colorSpace);
        Background.Value.Paint(context, source.DrawingSurface.Canvas);

        using var snapshot = source.DrawingSurface.Snapshot();
        using Shader imageShader = snapshot.ToShader();
        using Shader gradientShader = Shader.CreateLinearGradient(VecD.Zero, new VecD(1, 0), colors, offsets, Matrix3X3.Identity);

        Uniforms uniforms = new Uniforms();
        uniforms.Add("iImage", new Uniform("iImage", imageShader));
        uniforms.Add("iGradient", new Uniform("iGradient", gradientShader));
        using Shader shader = Shader.Create(shaderCode, uniforms, out _);
        using Paint paint = new() { BlendMode = BlendMode.Src };
        paint.Shader = shader;

        Texture texture = RequestTexture(0, size, colorSpace);
        int savedTexture = texture.DrawingSurface.Canvas.Save();
        texture.DrawingSurface.Canvas.SetMatrix(Matrix3X3.Identity);
        texture.DrawingSurface.Canvas.DrawRect(0, 0, size.X, size.Y, paint);
        texture.DrawingSurface.Canvas.RestoreToCount(savedTexture);

        int saved = surface.Save();
        surface.SetMatrix(Matrix3X3.Identity);
        surface.DrawSurface(texture.DrawingSurface, 0, 0);
        surface.RestoreToCount(saved);
    }

    public override Node CreateCopy()
    {
        return new ColorRampNode();
    }
}
