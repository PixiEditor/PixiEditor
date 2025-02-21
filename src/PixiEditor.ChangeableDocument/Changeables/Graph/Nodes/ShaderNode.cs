using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Shaders.Generation;
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
    private string lastShaderCode;
    private Paint paint;

    private VecI lastDocumentSize;
    private List<Shader> lastCustomImageShaders = new();

    private Dictionary<string, (InputProperty prop, UniformValueType valueType)> uniformInputs = new();

    protected override bool ExecuteOnlyOnCacheChange => true;
    protected override CacheTriggerFlags CacheTrigger => CacheTriggerFlags.All;

    public ShaderNode()
    {
        Background = CreateRenderInput("Background", "BACKGROUND");
        ShaderCode = CreateInput("ShaderCode", "SHADER_CODE", "")
            .WithRules(validator => validator.Custom(ValidateShaderCode))
            .NonOverridenChanged(RegenerateUniformInputs);

        paint = new Paint();
        Output.FirstInChain = null;
    }

    protected override void OnExecute(RenderContext context)
    {
        base.OnExecute(context);

        lastDocumentSize = context.DocumentSize;

        if (lastShaderCode != ShaderCode.Value)
        {
            GenerateShader(context);
        }
        else if (shader != null)
        {
            Uniforms uniforms = GenerateUniforms(context);
            shader = shader.WithUpdatedUniforms(uniforms);
        }

        paint.Shader = shader;
    }

    private void GenerateShader(RenderContext context)
    {
        Uniforms uniforms = null;

        uniforms = GenerateUniforms(context);

        shader?.Dispose();

        if (uniforms != null)
        {
            shader = Shader.Create(ShaderCode.Value, uniforms, out _);
        }
        else
        {
            shader = Shader.Create(ShaderCode.Value, out _);
        }

        lastShaderCode = ShaderCode.Value;
    }

    private Uniforms GenerateUniforms(RenderContext context)
    {
        Uniforms uniforms;
        uniforms = new Uniforms();

        uniforms.Add("iResolution", new Uniform("iResolution", context.DocumentSize));
        uniforms.Add("iNormalizedTime", new Uniform("iNormalizedTime", (float)context.FrameTime.NormalizedTime));
        uniforms.Add("iFrame", new Uniform("iFrame", context.FrameTime.Frame));

        AddCustomUniforms(uniforms);

        if (Background.Value == null)
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

        surface.Canvas.DrawRect(0, 0, context.DocumentSize.X, context.DocumentSize.Y, paint);
    }

    public override RectD? GetPreviewBounds(int frame, string elementToRenderName = "")
    {
        return new RectD(0, 0, lastDocumentSize.X, lastDocumentSize.Y);
    }

    public override bool RenderPreview(DrawingSurface renderOn, RenderContext context, string elementToRenderName)
    {
        if (shader == null || paint == null)
        {
            renderOn.Canvas.DrawColor(Colors.Magenta, BlendMode.Src);
            return true;
        }

        renderOn.Canvas.DrawRect(0, 0, lastDocumentSize.X, lastDocumentSize.Y, paint);
        return true;
    }

    public override Node CreateCopy()
    {
        return new ShaderNode();
    }

    private void RegenerateUniformInputs(string newShaderCode)
    {
        UniformDeclaration[]? declarations = Shader.GetUniformDeclarations(newShaderCode);
        if(declarations == null) return;

        if(declarations.Length == 0)
        {
            foreach (var input in uniformInputs)
            {
                RemoveInputProperty(input.Value.prop);
            }

            uniformInputs.Clear();
            return;
        }

        var uniforms = declarations;

        var nonExistingUniforms = uniformInputs.Keys.Where(x => uniforms.All(y => y.Name != x)).ToList();
        foreach (var nonExistingUniform in nonExistingUniforms)
        {
            RemoveInputProperty(uniformInputs[nonExistingUniform].prop);
            uniformInputs.Remove(nonExistingUniform);
        }

        foreach (var uniform in uniforms)
        {
            if(IsBuiltInUniform(uniform.Name))
            {
                continue;
            }

            if (uniformInputs.ContainsKey(uniform.Name) && uniformInputs[uniform.Name].valueType != uniform.DataType)
            {
                RemoveInputProperty(uniformInputs[uniform.Name].prop);
                uniformInputs.Remove(uniform.Name);
            }

            if (!uniformInputs.ContainsKey(uniform.Name))
            {
                InputProperty input;
                if (uniform.DataType == UniformValueType.Float)
                {
                    input = CreateInput(uniform.Name, uniform.Name, 0f);
                }
                else if (uniform.DataType == UniformValueType.Shader)
                {
                    input = CreateInput<Texture>(uniform.Name, uniform.Name, null);
                }
                else if (uniform.DataType == UniformValueType.Color)
                {
                    input = CreateInput<Color>(uniform.Name, uniform.Name, Colors.Black);
                }
                else if (uniform.DataType == UniformValueType.Vector2)
                {
                    input = CreateInput<VecD>(uniform.Name, uniform.Name, new VecD(0, 0));
                }
                else
                {
                    continue;
                }

                uniformInputs.Add(uniform.Name, (input, uniform.DataType));
            }
        }
    }

    private void AddCustomUniforms(Uniforms uniforms)
    {
        foreach (var imgShader in lastCustomImageShaders)
        {
            imgShader.Dispose();
        }

        lastCustomImageShaders.Clear();

        foreach (var input in uniformInputs)
        {
            object value = input.Value.prop.Value;
            if (input.Value.prop.Value is ShaderExpressionVariable expressionVariable)
            {
                value = expressionVariable.GetConstant();
            }

            if (value is float floatValue)
            {
                uniforms.Add(input.Key, new Uniform(input.Key, floatValue));
            }
            else if (value is double doubleValue)
            {
                uniforms.Add(input.Key, new Uniform(input.Key, (float)doubleValue));
            }
            else if (value is int intValue)
            {
                uniforms.Add(input.Key, new Uniform(input.Key, intValue));
            }
            else if (value is VecD vector)
            {
                uniforms.Add(input.Key, new Uniform(input.Key, vector));
            }
            else if (value is Color color)
            {
                uniforms.Add(input.Key, new Uniform(input.Key, color));
            }
            else if (value is Texture texture)
            {
                var snapshot = texture.DrawingSurface.Snapshot();
                Shader snapshotShader = snapshot.ToShader();
                lastCustomImageShaders.Add(snapshotShader);
                uniforms.Add(input.Key, new Uniform(input.Key, snapshotShader));
                snapshot.Dispose();
            }
        }
    }

    private bool IsBuiltInUniform(string name)
    {
        return name is "iResolution" or "iNormalizedTime" or "iFrame" or "iImage";
    }

    private ValidatorResult ValidateShaderCode(object? value)
    {
        if (value is string code)
        {
            var result = Shader.Create(code, out string errors);
            result?.Dispose();
            return new (string.IsNullOrWhiteSpace(errors), errors);
        }

        return new (false, "Shader code must be a string");
    }
}
