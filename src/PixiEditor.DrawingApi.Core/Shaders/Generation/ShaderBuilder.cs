using System.Text;
using PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;
using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Shaders.Generation;

public class ShaderBuilder
{
    public Uniforms Uniforms { get; } = new Uniforms();

    private StringBuilder _bodyBuilder = new StringBuilder();

    public Shader BuildShader()
    {
        string generatedSksl = ToSkSl();
        return new Shader(generatedSksl, Uniforms);
    }

    public string ToSkSl()
    {
        StringBuilder sb = new StringBuilder();
        AppendUniforms(sb);
        sb.AppendLine("half4 main(float2 coords)");
        sb.AppendLine("{");
        sb.Append(_bodyBuilder);
        sb.AppendLine("}");

        return sb.ToString();
    }

    private void AppendUniforms(StringBuilder sb)
    {
        foreach (var uniform in Uniforms)
        {
            sb.AppendLine($"uniform {uniform.Value.UniformName} {uniform.Value.Name};");
        }
    }

    public TextureSampler AddTexture(Texture texture)
    {
        string name = $"texture_{Uniforms.Count}";
        Uniforms[name] = new Uniform(name, texture.DrawingSurface.Snapshot().ToShader());
        return new TextureSampler(name);
    }
    
    public Half4 Sample(TextureSampler texName, Float2 pos)
    {
        string resultName = $"color_{Uniforms.Count}";
        Half4 result = new Half4(resultName);
        _bodyBuilder.AppendLine($"half4 {resultName} = sample({texName.UniformName}, {pos.UniformName});"); 
        return result;
    }

    public void ReturnVar(Half4 colorValue)
    {
        _bodyBuilder.AppendLine($"return {colorValue.UniformName};");
    }
    
    public void ReturnConst(Half4 colorValue)
    {
        _bodyBuilder.AppendLine($"return {colorValue.ConstantValueString};");
    }

    public void Set<T>(T contextPosition, T coordinateValue) where T : ShaderExpressionVariable
    {
        _bodyBuilder.AppendLine($"{contextPosition.UniformName} = {coordinateValue.UniformName};");
    }

    public void SetConstant<T>(T contextPosition, T constantValueVar) where T : ShaderExpressionVariable
    {
        _bodyBuilder.AppendLine($"{contextPosition.UniformName} = {constantValueVar.ConstantValueString};"); 
    }
}
