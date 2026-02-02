using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Shaders.Generation;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Utility;

[NodeInfo("Switch")]
public class SwitchNode : Node
{
    public FuncInputProperty<Bool> Condition { get; }
    public SyncedTypeInputProperty InputTrue { get; }
    public SyncedTypeInputProperty InputFalse { get; }
    public SyncedTypeOutputProperty Output { get; }

    // TODO: More func handlers
    // TODO: CPU
    // TODO: Connection validation
    // TODO: Saving doesn't work
    public SwitchNode()
    {
        Condition = CreateFuncInput("Condition", "CONDITION", new Bool("") { ConstantValue = false });
        InputTrue = CreateSyncedTypeInput("InputTrue", "ON_TRUE", null)
            .AddTypeHandler<Func<FuncContext, Float1>>(() => new FuncInputProperty<Float1>(this, "InputTrue", "ON_TRUE", new Float1(""){ ConstantValue = 0f }))
            .AddTypeHandler<Func<FuncContext, Half4>>(() => new FuncInputProperty<Half4>(this, "InputTrue", "ON_TRUE", new Half4(""){ ConstantValue = new Color() }));
        InputFalse = CreateSyncedTypeInput("InputFalse", "ON_FALSE", InputTrue)
            .AddTypeHandler<Func<FuncContext, Float1>>(() => new FuncInputProperty<Float1>(this, "InputFalse", "ON_FALSE", new Float1(""){ ConstantValue = 0f }))
            .AddTypeHandler<Func<FuncContext, Half4>>(() => new FuncInputProperty<Half4>(this, "InputFalse", "ON_FALSE", new Half4(""){ ConstantValue = new Color() }));
        Output = CreateSyncedTypeOutput("Output", "RESULT", InputTrue)
            .AddTypeHandler<Func<FuncContext, Float1>>(HandleFloat1Output)
            .AddTypeHandler<Func<FuncContext, Half4>>(HandleHalf4Output);
    }

    protected override void OnExecute(RenderContext context)
    {
        /*Output.Value = Condition.Value ? InputTrue.Value : InputFalse.Value;
        if(Output.Value is Delegate del)
        {
            Output.Value = del.DynamicInvoke(FuncContext.NoContext);
            if(Output.Value is ShaderExpressionVariable expr)
            {
                Output.Value = expr.GetConstant();
            }
        }*/
    }

    private OutputProperty HandleFloat1Output()
    {
        return new FuncOutputProperty<Float1>(this, "RESULT", "RESULT", HandleConditionalFloat1);
    }

    private OutputProperty HandleHalf4Output()
    {
        return new FuncOutputProperty<Half4>(this, "RESULT", "RESULT", HandleConditionalHalf4);
    }

    private Float1 HandleConditionalFloat1(FuncContext context)
    {
        if(!context.HasContext) return null;
        if(!InputTrue.InternalProperty.ValueType.IsAssignableTo(typeof(Delegate)) ||
           !InputFalse.InternalProperty.ValueType.IsAssignableTo(typeof(Delegate)))
            return null;

        Bool value = context.GetValue(Condition);
        return context.ConditionalVariable(value,
            context.GetValue(InputTrue.InternalProperty as FuncInputProperty<Float1>),
            context.GetValue(InputFalse.InternalProperty as FuncInputProperty<Float1>));
    }

    private Half4 HandleConditionalHalf4(FuncContext context)
    {
        if(!context.HasContext) return null;
        if(!InputTrue.InternalProperty.ValueType.IsAssignableTo(typeof(Delegate)) ||
           !InputFalse.InternalProperty.ValueType.IsAssignableTo(typeof(Delegate)))
            return null;

        Bool value = context.GetValue(Condition);
        return context.ConditionalVariable(value,
            context.GetValue(InputTrue.InternalProperty as FuncInputProperty<Half4>),
            context.GetValue(InputFalse.InternalProperty as FuncInputProperty<Half4>));
    }


    public override Node CreateCopy()
    {
        return new SwitchNode();
    }
}
