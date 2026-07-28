using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo(UniqueName)]
public class LerpColorNode : Node // TODO: ILerpable as inputs? 
{
    public const string UniqueName = "Lerp";
    public const string FromPropertyName = "From";
    public const string ToPropertyName = "To";

    public FuncOutputProperty<Half4> Result { get; }
    public FuncInputProperty<Half4> From { get; }
    public FuncInputProperty<Half4> To { get; }
    public FuncInputProperty<Float1> Time { get; }

    public LerpColorNode()
    {
        Result = CreateFuncOutput<Half4>("Result", "RESULT", Lerp);
        From = CreateFuncInput<Half4>(FromPropertyName, "FROM", new Half4(Vec4D.Zero));
        To = CreateFuncInput<Half4>(ToPropertyName, "TO", new Half4(Vec4D.One));
        Time = CreateFuncInput<Float1>("Time", "TIME", 0.5);
    }

    private Half4 Lerp(FuncContext arg)
    {
        Half4 from = arg.GetValue(From);
        Half4 to = arg.GetValue(To);
        Float1 time = arg.GetValue(Time);

        if (arg.HasContext)
        {
            return arg.NewHalf4(ShaderMath.Lerp(from, to, time));
        }

        var constFrom = (Vec4D)from.GetConstant();
        var constTo = (Vec4D)to.GetConstant();
        object constTime = time.GetConstant();

        double dTime = constTime switch
        {
            double d => d,
            int i => i,
            float f => f,
            _ => throw new InvalidOperationException("Unsupported constant type for time.")
        };

        Vec4D result = constFrom.Lerp(constTo, dTime).Clamp(0, 1);
        return new Half4("") { ConstantValue = result };
    }

    protected override void OnExecute(RenderContext context)
    {
    }

    public override Node CreateCopy()
    {
        return new LerpColorNode();
    }
}
