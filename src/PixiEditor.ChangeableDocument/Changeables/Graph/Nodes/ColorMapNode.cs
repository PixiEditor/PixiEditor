using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.ColorSpaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Palettes;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("ColorMap")]
public class ColorMapNode : RenderNode, IRenderInput
{
    public RenderInputProperty Background { get; }
    public InputProperty<Palette> Palette { get; }
    public InputProperty<ColorSpaceType> ColorSpace { get; }

    protected override bool ExecuteOnlyOnCacheChange => true;
    protected override CacheTriggerFlags CacheTrigger => CacheTriggerFlags.Inputs;

    private const int MaxPaletteColors = 256;

    private Shader? shader;
    private Shader? imageShader;
    private Shader? paletteShader;
    private Paint paint;

    private string shaderCode = """
                                uniform shader iImage;
                                uniform shader iPalette;
                                uniform int paletteCount;

                                half4 main(float2 uv) {
                                    half4 src = iImage.eval(uv);
                                    half3 c = src.rgb;
                                    half3 best = iPalette.eval(float2(0.5, 0.5)).rgb;
                                    half bestD = dot(c - best, c - best);
                                    for (int i = 1; i < 256; i++) {
                                        if (i >= paletteCount) { break; }
                                        half3 cand = iPalette.eval(float2(float(i) + 0.5, 0.5)).rgb;
                                        half d = dot(c - cand, c - cand);
                                        if (d < bestD) { bestD = d; best = cand; }
                                    }
                                    return half4(best, 1.0) * src.a;
                                }
                                """;

    public ColorMapNode()
    {
        Background = CreateRenderInput("Image", "COLOR_MAP_IMAGE");
        Palette = CreateInput<Palette>("Palette", "PALETTE", null);
        ColorSpace = CreateInput("ColorSpace", "COLOR_SPACE", ColorSpaceType.Inherit);

        paint = new Paint() { BlendMode = BlendMode.Src };
        Output.FirstInChain = null;
        RendersInAbsoluteCoordinates = true;
    }

    protected override void OnPaint(RenderContext context, Canvas surface)
    {
        if (context == null || Background.Value == null)
            return;

        bool isAdjusted = context.RenderOutputSize == context.DocumentSize;
        VecI finalSize = isAdjusted
            ? context.RenderOutputSize
            : (VecI)(context.RenderOutputSize * context.ChunkResolution.InvertedMultiplier());

        if (finalSize.X <= 0 || finalSize.Y <= 0)
            return;

        var colorSpace = ColorSpace.Value == ColorSpaceType.Inherit
            ? context.ProcessingColorSpace
            : ColorSpace.Value == ColorSpaceType.Srgb
                ? Drawie.Backend.Core.Surfaces.ImageData.ColorSpace.CreateSrgb()
                : Drawie.Backend.Core.Surfaces.ImageData.ColorSpace.CreateSrgbLinear();

        using var source = Texture.ForProcessing(finalSize, colorSpace);
        var backgroundContext = context.Clone();
        backgroundContext.RenderSurface = source.DrawingSurface.Canvas;
        backgroundContext.RenderOutputSize = finalSize;
        backgroundContext.ChunkResolution = ChunkResolution.Full;
        backgroundContext.VisibleDocumentRegion = null;
        Background.Value.Paint(backgroundContext, source.DrawingSurface.Canvas);

        Palette palette = Palette.Value;
        int count = palette != null ? Math.Min(palette.Count, MaxPaletteColors) : 0;

        Texture target = RequestTexture(0, finalSize, colorSpace);

        if (count > 0)
        {
            UpdateShader(source, palette, count, colorSpace);
            target.DrawingSurface.Canvas.DrawRect(0, 0, finalSize.X, finalSize.Y, paint);
        }
        else
        {
            target.DrawingSurface.Canvas.DrawSurface(source.DrawingSurface, 0, 0);
        }

        int saved = surface.Save();
        if (context.ChunkResolution != ChunkResolution.Full)
        {
            surface.Scale((float)context.ChunkResolution.Multiplier());
        }

        surface.DrawSurface(target.DrawingSurface, 0, 0);
        surface.RestoreToCount(saved);
    }

    private void UpdateShader(Texture source, Palette palette, int count,
        Drawie.Backend.Core.Surfaces.ImageData.ColorSpace colorSpace)
    {
        imageShader?.Dispose();
        using var imageSnapshot = source.DrawingSurface.Snapshot();
        imageShader = imageSnapshot.ToShader();

        paletteShader?.Dispose();
        paletteShader = CreatePaletteShader(palette, count, colorSpace);

        Uniforms uniforms = new Uniforms();
        uniforms.Add("iImage", new Uniform("iImage", imageShader));
        uniforms.Add("iPalette", new Uniform("iPalette", paletteShader));
        uniforms.Add("paletteCount", new Uniform("paletteCount", count));

        shader = shader == null
            ? Shader.Create(shaderCode, uniforms, out _)
            : shader.WithUpdatedUniforms(uniforms);

        paint.Shader = shader;
    }

    private Shader CreatePaletteShader(Palette palette, int count,
        Drawie.Backend.Core.Surfaces.ImageData.ColorSpace colorSpace)
    {
        using var paletteTexture = Texture.ForProcessing(new VecI(count, 1), colorSpace);
        using Paint pixelPaint = new() { BlendMode = BlendMode.Src };
        for (int i = 0; i < count; i++)
        {
            pixelPaint.Color = palette[i];
            paletteTexture.DrawingSurface.Canvas.DrawRect(i, 0, 1, 1, pixelPaint);
        }

        using var snapshot = paletteTexture.DrawingSurface.Snapshot();
        return snapshot.ToShader(TileMode.Clamp, TileMode.Clamp, SamplingOptions.Default, Matrix3X3.Identity);
    }

    public override void RenderPreview(DrawingSurface renderOn, RenderContext context, string elementToRenderName)
    {
        int saved = renderOn.Canvas.Save();
        renderOn.Canvas.Scale((float)context.ChunkResolution.InvertedMultiplier());
        OnPaint(context, renderOn.Canvas);
        renderOn.Canvas.RestoreToCount(saved);
    }

    public override RectD? GetPreviewBounds(RenderContext ctx, string elementToRenderName)
    {
        return new RectD(0, 0, ctx.DocumentSize.X, ctx.DocumentSize.Y);
    }

    public override Node CreateCopy()
    {
        return new ColorMapNode();
    }

    public override void Dispose()
    {
        base.Dispose();
        shader?.Dispose();
        imageShader?.Dispose();
        paletteShader?.Dispose();
        paint?.Dispose();
    }
}
