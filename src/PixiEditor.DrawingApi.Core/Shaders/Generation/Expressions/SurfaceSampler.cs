namespace PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

public class SurfaceSampler : ShaderExpressionVariable<Texture>
{
    public SurfaceSampler(string name) : base(name)
    {
    }

    public override string ConstantValueString { get; } = "";
    public override Expression? OverrideExpression { get; set; }
}
