using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Calculations;

[NodeInfo("Remap")]
public class RemapNode : Node
{
    public FuncInputProperty<Float1, ShaderFuncContext> OldMin { get; }
    public FuncInputProperty<Float1, ShaderFuncContext> OldMax { get; }
    public FuncInputProperty<Float1, ShaderFuncContext> NewMin { get; }
    public FuncInputProperty<Float1, ShaderFuncContext> NewMax { get; }

    public FuncInputProperty<Float1, ShaderFuncContext> Value { get; }

    public FuncOutputProperty<Float1, ShaderFuncContext> Result { get; }

    public RemapNode()
    {
        OldMin = CreateFuncInput<Float1, ShaderFuncContext>("OldMin", "OLD_MIN", 0.0);
        OldMax = CreateFuncInput<Float1, ShaderFuncContext>("OldMax", "OLD_MAX", 1.0);
        NewMin = CreateFuncInput<Float1, ShaderFuncContext>("NewMin", "NEW_MIN", 0.0);
        NewMax = CreateFuncInput<Float1, ShaderFuncContext>("NewMax", "NEW_MAX", 1.0);
        Value = CreateFuncInput<Float1, ShaderFuncContext>("Value", "VALUE", 0.5);

        Result = CreateFuncOutput<Float1, ShaderFuncContext>("Result", "RESULT", Remap);
    }

    private Float1 Remap(ShaderFuncContext context)
    {
        if (context.HasContext)
        {
            var oldMin = context.GetValue(OldMin);
            var oldMax = context.GetValue(OldMax);
            var newMin = context.GetValue(NewMin);
            var newMax = context.GetValue(NewMax);
            var value = context.GetValue(Value);

            return context.NewFloat1(context.Builder.Functions.GetRemap(value, oldMin, oldMax, newMin, newMax));
        }

        double oldMinValue = context.GetValue(OldMin).GetConstant() as double? ?? 0.0;
        double oldMaxValue = context.GetValue(OldMax).GetConstant() as double? ?? 1.0;
        double newMinValue = context.GetValue(NewMin).GetConstant() as double? ?? 0.0;
        double newMaxValue = context.GetValue(NewMax).GetConstant() as double? ?? 1.0;
        double valueValue = context.GetValue(Value).GetConstant() as double? ?? 0.5;
        double resultValue = newMinValue + (valueValue - oldMinValue) * (newMaxValue - newMinValue) / (oldMaxValue - oldMinValue);
        return new Float1(string.Empty) { ConstantValue = resultValue };
    }

    protected override void OnExecute(RenderContext context)
    {

    }

    public override Node CreateCopy()
    {
        return new RemapNode();
    }
}
