using Drawie.Backend.Core;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Effects;

[NodeInfo("Posterization")]
public class PosterizationNode : RenderNode, IRenderInput
{
    public RenderInputProperty Background { get; }
    public InputProperty<int> Levels { get; }
    
    private Paint paint;
    private Shader shader;
    
    private Shader? lastImageShader;
    private VecI lastDocumentSize;
    
    private string shaderCode = """
                                 uniform shader iImage;
                                 uniform float iLevels;

                                 half4 main(float2 uv)
                                 {
                                    half4 color = iImage.eval(uv);
                                    half3 posterized = floor(color.rgb * iLevels) / iLevels;
                                    return half4(posterized, color.a);
                                 }
                                 """;

    protected override bool ExecuteOnlyOnCacheChange => true;

    public PosterizationNode()
    {
        Background = CreateRenderInput("Background", "BACKGROUND");
        Levels = CreateInput("Levels", "LEVELS", 8)
            .WithRules(v => v.Min(2).Max(256));

        paint = new Paint();
        Output.FirstInChain = null;
    }

    protected override void OnExecute(RenderContext context)
    {
        base.OnExecute(context);
        lastDocumentSize = context.RenderOutputSize;
        
        Uniforms uniforms = new Uniforms();
        uniforms.Add("iImage", new Uniform("iImage", lastImageShader));
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
        using Texture temp = Texture.ForProcessing(surface, context.ProcessingColorSpace);
        Background.Value.Paint(context, temp.DrawingSurface);
        var snapshot = temp.DrawingSurface.Snapshot();
        
        lastImageShader?.Dispose();
        lastImageShader = snapshot.ToShader();

        Uniforms uniforms = new Uniforms();
        uniforms.Add("iImage", new Uniform("iImage", lastImageShader));
        uniforms.Add("iLevels", new Uniform("iLevels", (float)Levels.Value));
        shader = shader.WithUpdatedUniforms(uniforms);
        paint.Shader = shader;
        snapshot.Dispose();
        
        var saved = surface.Canvas.Save();
        surface.Canvas.Scale((float)context.ChunkResolution.InvertedMultiplier());
        surface.Canvas.DrawRect(0, 0, context.RenderOutputSize.X, context.RenderOutputSize.Y, paint);
        surface.Canvas.RestoreToCount(saved);
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
