using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Shader")]
public class ShaderNode : RenderNode, IRenderInput, ICustomShaderNode
{
    public RenderInputProperty Background { get; }
    public InputProperty<string> ShaderCode { get; }

    private Shader? shader;
    private Shader? lastImageShader;
    private string lastErrors;
    private string lastShaderCode;
    private Paint paint;

    protected override bool ExecuteOnlyOnCacheChange => true;
    protected override CacheTriggerFlags CacheTrigger => CacheTriggerFlags.All;

    public ShaderNode()
    {
        Background = CreateRenderInput("Background", "BACKGROUND");
        ShaderCode = CreateInput("ShaderCode", "SHADER_CODE", "");
        paint = new Paint();
        Output.FirstInChain = null;
    }

    protected override void OnExecute(RenderContext context)
    {
        base.OnExecute(context);

        if (lastShaderCode != ShaderCode.Value)
        {
            Uniforms uniforms = null;

            uniforms = GenerateUniforms(context);

            shader?.Dispose();

            if (uniforms != null)
            {
                shader = Shader.CreateFromString(ShaderCode.Value, uniforms, out lastErrors);
            }
            else
            {
                shader = Shader.CreateFromString(ShaderCode.Value, out lastErrors);
            }

            lastShaderCode = ShaderCode.Value;

            if (shader == null)
            {
                return;
            }
        }
        else if(shader != null)
        {
            Uniforms uniforms = GenerateUniforms(context);
            shader = shader.WithUpdatedUniforms(uniforms);
        }

        paint.Shader = shader;
    }

    private Uniforms GenerateUniforms(RenderContext context)
    {
        Uniforms uniforms;
        uniforms = new Uniforms();

        uniforms.Add("iResolution", new Uniform("iResolution", context.DocumentSize));
        uniforms.Add("iNormalizedTime", new Uniform("iNormalizedTime", (float)context.FrameTime.NormalizedTime));
        uniforms.Add("iFrame", new Uniform("iFrame", context.FrameTime.Frame));

        if(Background.Value == null)
        {
            lastImageShader?.Dispose();
            lastImageShader = null;
            return uniforms;
        }

        Texture texture = RequestTexture(50, context.DocumentSize, context.ProcessingColorSpace);
        Background.Value.Paint(context, texture.DrawingSurface);

        var snapshot = texture.DrawingSurface.Snapshot();
        lastImageShader?.Dispose();
        lastImageShader = snapshot.ToShader();

        uniforms.Add("iImage", new Uniform("iImage", lastImageShader));

        snapshot.Dispose();
        return uniforms;
    }

    protected override void OnPaint(RenderContext context, DrawingSurface surface)
    {
        if (shader == null)
        {
            surface.Canvas.DrawColor(Colors.Magenta, BlendMode.Src);
            return;
        }

        if (Background.Value != null)
        {
            int saved = surface.Canvas.SaveLayer(paint);
            Background.Value.Paint(context, surface);
            surface.Canvas.RestoreToCount(saved);
        }

        surface.Canvas.DrawRect(0, 0, context.DocumentSize.X, context.DocumentSize.Y, paint);
    }

    public override RectD? GetPreviewBounds(int frame, string elementToRenderName = "")
    {
        return null;
    }

    public override bool RenderPreview(DrawingSurface renderOn, RenderContext context, string elementToRenderName)
    {
        return false;
    }

    public override Node CreateCopy()
    {
        return new ShaderNode();
    }
}
