using Drawie.Backend.Core;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Effects;

[NodeInfo("Posterization")]
public class PosterizationNode : RenderNode, IRenderInput
{
    public RenderInputProperty Background { get; }
    public InputProperty<PosterizationMode> Mode { get; }
    public InputProperty<int> Levels { get; }
    public InputProperty<bool> UseDocumentColorSpace { get; }
    
    private Paint paint;
    private Shader shader;
    
    private Shader? lastImageShader;
    private PosterizationMode? lastMode = null;
    private VecI lastDocumentSize;
    
    private string shaderCode = """
                                 uniform shader iImage;
                                 uniform int iMode;
                                 uniform float iLevels;
                                 
                                 half posterize(half value, float levels) {
                                     return clamp(floor(value * (levels - 1) + 0.5) / (levels - 1), 0.0, 1.0);
                                 }
                                 
                                 half4 posterizeRgb(half4 color, float levels) {
                                     return half4(
                                         posterize(color.r, levels),
                                         posterize(color.g, levels),
                                         posterize(color.b, levels),
                                         color.a
                                     );
                                 }
                                 
                                 half4 posterizeLuminance(half4 color, float levels) {
                                     half lum = dot(color.rgb, half3(0.299, 0.587, 0.114));
                                     half posterizedLum = posterize(lum, levels);
                                     return half4(posterizedLum, posterizedLum, posterizedLum, color.a);
                                 }
                                 
                                 half4 main(float2 uv)
                                 {
                                    half4 color = iImage.eval(uv);
                                    half4 result;
                                    
                                    if(iMode == 0) {
                                        result = posterizeRgb(color, iLevels);
                                    } else if(iMode == 1) {
                                        result = posterizeLuminance(color, iLevels);
                                    } 
                                    
                                    return result;
                                 }
                                 """;

    protected override bool ExecuteOnlyOnCacheChange => true;

    public PosterizationNode()
    {
        Background = CreateRenderInput("Background", "BACKGROUND");
        Mode = CreateInput("Mode", "MODE", PosterizationMode.Rgb);
        Levels = CreateInput("Levels", "LEVELS", 8)
            .WithRules(v => v.Min(2).Max(256));
        UseDocumentColorSpace = CreateInput("UseDocumentColorSpace", "USE_DOCUMENT_COLOR_SPACE", false);
        
        paint = new Paint();
        Output.FirstInChain = null;
    }

    protected override void OnExecute(RenderContext context)
    {
        base.OnExecute(context);
        lastDocumentSize = context.RenderOutputSize;
        lastMode = Mode.Value;
        
        Uniforms uniforms = new Uniforms();
        uniforms.Add("iImage", new Uniform("iImage", lastImageShader));
        uniforms.Add("iMode", new Uniform("iMode", (int)Mode.Value));
        uniforms.Add("iLevels", new Uniform("iLevels", (float)Levels.Value));
        shader?.Dispose();
        shader = Shader.Create(shaderCode, uniforms, out _);
    }
    
    protected override void OnPaint(RenderContext context, DrawingSurface surface)
    {
        if (Background.Value == null)
        {
            return;
        }
        
        var colorSpace = UseDocumentColorSpace.Value
            ? context.ProcessingColorSpace
            : ColorSpace.CreateSrgb();
        
        using Texture temp = Texture.ForProcessing(surface, colorSpace);
        Background.Value.Paint(context, temp.DrawingSurface);
        var snapshot = temp.DrawingSurface.Snapshot();
        
        lastImageShader?.Dispose();
        lastImageShader = snapshot.ToShader();

        Uniforms uniforms = new Uniforms();
        uniforms.Add("iImage", new Uniform("iImage", lastImageShader));
        uniforms.Add("iMode", new Uniform("iMode", (int)Mode.Value));
        uniforms.Add("iLevels", new Uniform("iLevels", (float)Levels.Value));
        shader = shader.WithUpdatedUniforms(uniforms);
        paint.Shader = shader;
        snapshot.Dispose();
        
        var saved = temp.DrawingSurface.Canvas.Save();
        temp.DrawingSurface.Canvas.Scale((float)context.ChunkResolution.InvertedMultiplier());
        temp.DrawingSurface.Canvas.DrawRect(0, 0, context.RenderOutputSize.X, context.RenderOutputSize.Y, paint);
        temp.DrawingSurface.Canvas.RestoreToCount(saved);
        surface.Canvas.DrawSurface(temp.DrawingSurface, 0, 0);
    }
    
    public override RectD? GetPreviewBounds(int frame, string elementToRenderName = "")
    {
        return new RectD(0, 0, lastDocumentSize.X, lastDocumentSize.Y);
    }

    public override bool RenderPreview(DrawingSurface renderOn, RenderContext context, string elementToRenderName)
    {
        OnPaint(context, renderOn);
        return true;
    }

    public override Node CreateCopy()
    {
        return new PosterizationNode();
    }
}

public enum PosterizationMode
{
    Rgb = 0,
    Luminance = 1,
}
