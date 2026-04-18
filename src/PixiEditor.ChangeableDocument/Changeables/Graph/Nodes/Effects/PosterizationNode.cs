using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.ColorSpaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Effects;

[NodeInfo("Posterization")]
public class PosterizationNode : RenderNode, IRenderInput
{
    public RenderInputProperty Background { get; }
    public InputProperty<PosterizationMode> Mode { get; }
    public InputProperty<int> Levels { get; }
    public InputProperty<ColorSpaceType> PosterizationColorSpace { get; }
    
    private Paint paint;
    private Shader shader;
    
    private Shader? lastImageShader;
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
        PosterizationColorSpace = CreateInput("PosterizationColorSpace", "COLOR_SPACE", ColorSpaceType.Srgb);
        
        paint = new Paint();
        paint.BlendMode = BlendMode.Src;
        Output.FirstInChain = null;
    }

    protected override void OnExecute(RenderContext context)
    {
        base.OnExecute(context);
        lastDocumentSize = context.RenderOutputSize;
        
        Uniforms uniforms = new Uniforms();
        uniforms.Add("iImage", new Uniform("iImage", lastImageShader));
        uniforms.Add("iMode", new Uniform("iMode", (int)Mode.Value));
        uniforms.Add("iLevels", new Uniform("iLevels", (float)Levels.Value));
        shader?.Dispose();
        shader = Shader.Create(shaderCode, uniforms, out _);
    }
    
    protected override void OnPaint(RenderContext context, Canvas surface)
    {
        if (Background.Value == null)
        {
            return;
        }

        ColorSpace colorSpace;
        switch (PosterizationColorSpace.Value)
        {
            case ColorSpaceType.Srgb:
                colorSpace = ColorSpace.CreateSrgb();
                break;
            case ColorSpaceType.LinearSrgb:
                colorSpace = ColorSpace.CreateSrgbLinear();
                break;
            case ColorSpaceType.Inherit:
                colorSpace = context.ProcessingColorSpace;
                break;
            default:
                colorSpace = ColorSpace.CreateSrgb();
                break;
        }
        
        using Texture temp = Texture.ForProcessing(surface, colorSpace);
        Background.Value.Paint(context, temp.DrawingSurface.Canvas);
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
        
        var savedTemp = temp.DrawingSurface.Canvas.Save();
        temp.DrawingSurface.Canvas.SetMatrix(Matrix3X3.Identity);
        temp.DrawingSurface.Canvas.DrawRect(0, 0, context.RenderOutputSize.X, context.RenderOutputSize.Y, paint);
        temp.DrawingSurface.Canvas.RestoreToCount(savedTemp);
        
        var saved = surface.Save();
        surface.SetMatrix(Matrix3X3.Identity);
        surface.DrawSurface(temp.DrawingSurface, 0, 0);
        surface.RestoreToCount(saved);
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
