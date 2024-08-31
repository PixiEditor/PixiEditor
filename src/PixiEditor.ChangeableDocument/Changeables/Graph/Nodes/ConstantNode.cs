using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("PixiEditor.Constant")]
public class ConstantNode : Node
{
    private IReadOnlyGraphConstant _constant;

    public Guid ConstantId => _constant.Id;

    internal ConstantNode(IReadOnlyGraphConstant constant)
    {
        _constant = constant;

        Value = CreateFuncOutput("Value", "VALUE", _ => GetValue());
    }

    public FuncOutputProperty<Float1> Value { get; set; }

    protected override Texture? OnExecute(RenderingContext context) => null;

    public override Node CreateCopy() => new ConstantNode(_constant);

    private Float1 GetValue()
    {
        if (_constant.Value == null)
        {
            return 0;
        }

        return (double)_constant.Value;
    }
    
    public static object GetDefault(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}
