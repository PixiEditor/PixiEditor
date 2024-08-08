using System.Collections.Generic;
using System.Text;
using PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;
using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Shaders.Generation;

public class ShaderBuilder
{
    public Uniforms Uniforms { get; } = new Uniforms();

    private StringBuilder _bodyBuilder = new StringBuilder();
    
    private List<ShaderExpressionVariable> _variables = new List<ShaderExpressionVariable>();
    
    private Dictionary<Texture, TextureSampler> _samplers = new Dictionary<Texture, TextureSampler>();

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

    public TextureSampler AddOrGetTexture(Texture texture)
    {
        if (_samplers.TryGetValue(texture, out var sampler))
        {
            return sampler;
        }
        
        string name = $"texture_{Uniforms.Count}";
        using var snapshot = texture.DrawingSurface.Snapshot();
        Uniforms[name] = new Uniform(name, snapshot.ToShader());
        var newSampler = new TextureSampler(name);
        _samplers[texture] = newSampler;
        
        return newSampler;
    }
    
    public Half4 Sample(TextureSampler texName, Float2 pos)
    {
        string resultName = $"color_{_variables.Count}";
        Half4 result = new Half4(resultName);
        _variables.Add(result);
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

    public Float2 ConstructFloat2(Expression x, Expression y)
    {
        string name = $"vec2_{_variables.Count}";
        Float2 result = new Float2(name);
        _variables.Add(result);

        string xExpression = x.ExpressionValue; 
        string yExpression = y.ExpressionValue;
        
        _bodyBuilder.AppendLine($"float2 {name} = float2({xExpression}, {yExpression});");
        return result;
    }

    public Float1 ConstructFloat1(Expression assignment)
    {
        string name = $"float_{_variables.Count}";
        Float1 result = new Float1(name, 0);
        _variables.Add(result);
        
        _bodyBuilder.AppendLine($"float {name} = {assignment.ExpressionValue};");
        return result;
    }

    public Half4 ConstructHalf4(Expression r, Expression g, Expression b, Expression a)
    {
        string name = $"color_{_variables.Count}";
        Half4 result = new Half4(name);
        _variables.Add(result);
        
        string rExpression = r.ExpressionValue;
        string gExpression = g.ExpressionValue;
        string bExpression = b.ExpressionValue;
        string aExpression = a.ExpressionValue;
        
        _bodyBuilder.AppendLine($"half4 {name} = half4({rExpression}, {gExpression}, {bExpression}, {aExpression});");
        return result;
    }

    public void Dispose()
    {
        _bodyBuilder.Clear();
        _variables.Clear();
        _samplers.Clear();
        
        foreach (var uniform in Uniforms)
        {
            uniform.Value.Dispose();
        }
    }
}
