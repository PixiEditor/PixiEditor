namespace PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

public class TextureSampler : ShaderExpressionVariable<Texture>
{
    public TextureSampler(string name) : base(name)
    {
    }

    public override string ConstantValueString { get; } = "";
}
