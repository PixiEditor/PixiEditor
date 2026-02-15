using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders.Generation;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Numerics;
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


    // TODO: Connection validation
    public SwitchNode()
    {
        Condition = CreateFuncInput("Condition", "CONDITION", new Bool("") { ConstantValue = false });
        InputTrue = CreateSyncedTypeInput("InputTrue", "ON_TRUE", null)
            .AllowGenericFallback(true);
        AddTrueFuncInputHandlers(new Float1("") { ConstantValue = 1f });
        AddTrueFuncInputHandlers(new Half4("") { ConstantValue = Colors.Black });
        AddTrueFuncInputHandlers(new Bool("") { ConstantValue = true });
        AddTrueFuncInputHandlers(new Int1("") { ConstantValue = 1 });
        AddTrueFuncInputHandlers(new Int2("") { ConstantValue = VecI.Zero });
        AddTrueFuncInputHandlers(new Float2("") { ConstantValue = VecD.Zero });
        AddTrueFuncInputHandlers(new Float3("") { ConstantValue = Vec3D.Zero });
        AddTrueFuncInputHandlers(new Half3("") { ConstantValue = Vec3D.Zero });
        AddTrueFuncInputHandlers(new Float3x3("") { ConstantValue = Matrix3X3.Identity });

        InputFalse = CreateSyncedTypeInput("InputFalse", "ON_FALSE", InputTrue)
            .AllowGenericFallback(true);
        AddFalseFuncInputHandlers(new Float1("") { ConstantValue = 1f });
        AddFalseFuncInputHandlers(new Half4("") { ConstantValue = Colors.Black });
        AddFalseFuncInputHandlers(new Bool("") { ConstantValue = true });
        AddFalseFuncInputHandlers(new Int1("") { ConstantValue = 1 });
        AddFalseFuncInputHandlers(new Int2("") { ConstantValue = VecI.Zero });
        AddFalseFuncInputHandlers(new Float2("") { ConstantValue = VecD.Zero });
        AddFalseFuncInputHandlers(new Float3("") { ConstantValue = Vec3D.Zero });
        AddFalseFuncInputHandlers(new Half3("") { ConstantValue = Vec3D.Zero });
        AddFalseFuncInputHandlers(new Float3x3("") { ConstantValue = Matrix3X3.Identity });

        Output = CreateSyncedTypeOutput("Output", "RESULT", InputTrue).AllowGenericFallback();
        AddOutputFuncHandlers(HandleConditionalFloat1);
        AddOutputFuncHandlers(HandleConditionalHalf4);
        AddOutputFuncHandlers(HandleConditionalBool);
        AddOutputFuncHandlers(HandleConditionalInt1);
        AddOutputFuncHandlers(HandleConditionalInt2);
        AddOutputFuncHandlers(HandleConditionalFloat2);
        AddOutputFuncHandlers(HandleConditionalFloat3);
        AddOutputFuncHandlers(HandleConditionalHalf3);
        AddOutputFuncHandlers(HandleConditionalFloat3x3);
    }

    protected override void OnExecute(RenderContext context)
    {
        if (!Output.InternalProperty.ValueType.IsAssignableTo(typeof(Delegate)))
        {
            bool condition = false;
            var val = Condition.Value.Invoke(FuncContext.NoContext).GetConstant();
            if (val is bool b)
            {
                condition = b;
            }
            else
            {
                try
                {
                    condition = Convert.ToBoolean(val);
                }
                catch
                {
                    condition = false;
                }
            }

            Output.Value = condition ? InputTrue.Value : InputFalse.Value;
            if (Output.Value is Delegate del)
            {
                Output.Value = del.DynamicInvoke(FuncContext.NoContext);
                if (Output.Value is ShaderExpressionVariable expr)
                {
                    Output.Value = expr.GetConstant();
                }
            }
        }
    }

    private Float1 HandleConditionalFloat1(FuncContext context)
    {
        if (!context.HasContext)
        {
            return ((bool)context.GetValue(Condition).GetConstant())
                ? context.GetValue(InputTrue.InternalProperty as FuncInputProperty<Float1>)
                : context.GetValue(InputFalse.InternalProperty as FuncInputProperty<Float1>);
        }

        if (!HandleConditional(context, out var value))
        {
            return null;
        }

        return context.ConditionalVariable(value,
            context.GetValue(InputTrue.InternalProperty as FuncInputProperty<Float1>),
            context.GetValue(InputFalse.InternalProperty as FuncInputProperty<Float1>));
    }

    private Half4 HandleConditionalHalf4(FuncContext context)
    {
        if (!context.HasContext)
        {
            return ((bool)context.GetValue(Condition).GetConstant())
                ? context.GetValue(InputTrue.InternalProperty as FuncInputProperty<Half4>)
                : context.GetValue(InputFalse.InternalProperty as FuncInputProperty<Half4>);
        }

        if (!HandleConditional(context, out var value))
        {
            return null;
        }

        return context.ConditionalVariable(value,
            context.GetValue(InputTrue.InternalProperty as FuncInputProperty<Half4>),
            context.GetValue(InputFalse.InternalProperty as FuncInputProperty<Half4>));
    }

    private Int1 HandleConditionalInt1(FuncContext context)
    {
        if (!context.HasContext)
        {
            return ((bool)context.GetValue(Condition).GetConstant())
                ? context.GetValue(InputTrue.InternalProperty as FuncInputProperty<Int1>)
                : context.GetValue(InputFalse.InternalProperty as FuncInputProperty<Int1>);
        }

        if (!HandleConditional(context, out var value))
        {
            return null;
        }

        return context.ConditionalVariable(value,
            context.GetValue(InputTrue.InternalProperty as FuncInputProperty<Int1>),
            context.GetValue(InputFalse.InternalProperty as FuncInputProperty<Int1>));
    }

    private Int2 HandleConditionalInt2(FuncContext context)
    {
        if (!context.HasContext)
        {
            return ((bool)context.GetValue(Condition).GetConstant())
                ? context.GetValue(InputTrue.InternalProperty as FuncInputProperty<Int2>)
                : context.GetValue(InputFalse.InternalProperty as FuncInputProperty<Int2>);
        }

        if (!HandleConditional(context, out var value))
        {
            return null;
        }

        return context.ConditionalVariable(value,
            context.GetValue(InputTrue.InternalProperty as FuncInputProperty<Int2>),
            context.GetValue(InputFalse.InternalProperty as FuncInputProperty<Int2>));
    }

    private Float2 HandleConditionalFloat2(FuncContext context)
    {
        if (!context.HasContext)
        {
            return ((bool)context.GetValue(Condition).GetConstant())
                ? context.GetValue(InputTrue.InternalProperty as FuncInputProperty<Float2>)
                : context.GetValue(InputFalse.InternalProperty as FuncInputProperty<Float2>);
        }

        if (!HandleConditional(context, out var value))
        {
            return null;
        }

        return context.ConditionalVariable(value,
            context.GetValue(InputTrue.InternalProperty as FuncInputProperty<Float2>),
            context.GetValue(InputFalse.InternalProperty as FuncInputProperty<Float2>));
    }

    private Float3 HandleConditionalFloat3(FuncContext context)
    {
        if (!context.HasContext)
        {
            return ((bool)context.GetValue(Condition).GetConstant())
                ? context.GetValue(InputTrue.InternalProperty as FuncInputProperty<Float3>)
                : context.GetValue(InputFalse.InternalProperty as FuncInputProperty<Float3>);
        }

        if (!HandleConditional(context, out var value))
        {
            return null;
        }

        return context.ConditionalVariable(value,
            context.GetValue(InputTrue.InternalProperty as FuncInputProperty<Float3>),
            context.GetValue(InputFalse.InternalProperty as FuncInputProperty<Float3>));
    }

    private Half3 HandleConditionalHalf3(FuncContext context)
    {
        if (!context.HasContext)
        {
            return ((bool)context.GetValue(Condition).GetConstant())
                ? context.GetValue(InputTrue.InternalProperty as FuncInputProperty<Half3>)
                : context.GetValue(InputFalse.InternalProperty as FuncInputProperty<Half3>);
        }

        if (!HandleConditional(context, out var value))
        {
            return null;
        }

        return context.ConditionalVariable(value,
            context.GetValue(InputTrue.InternalProperty as FuncInputProperty<Half3>),
            context.GetValue(InputFalse.InternalProperty as FuncInputProperty<Half3>));
    }

    private Float3x3 HandleConditionalFloat3x3(FuncContext context)
    {
        if (!context.HasContext)
        {
            return ((bool)context.GetValue(Condition).GetConstant())
                ? context.GetValue(InputTrue.InternalProperty as FuncInputProperty<Float3x3>)
                : context.GetValue(InputFalse.InternalProperty as FuncInputProperty<Float3x3>);
        }

        if (!HandleConditional(context, out var value))
        {
            return null;
        }

        return context.ConditionalVariable(value,
            context.GetValue(InputTrue.InternalProperty as FuncInputProperty<Float3x3>),
            context.GetValue(InputFalse.InternalProperty as FuncInputProperty<Float3x3>));
    }

    private Bool HandleConditionalBool(FuncContext context)
    {
        if (!context.HasContext)
        {
            return ((bool)context.GetValue(Condition).GetConstant())
                ? context.GetValue(InputTrue.InternalProperty as FuncInputProperty<Bool>)
                : context.GetValue(InputFalse.InternalProperty as FuncInputProperty<Bool>);
        }

        if (!HandleConditional(context, out var value))
        {
            return null;
        }

        return context.ConditionalVariable(value,
            context.GetValue(InputTrue.InternalProperty as FuncInputProperty<Bool>),
            context.GetValue(InputFalse.InternalProperty as FuncInputProperty<Bool>));
    }

    private bool HandleConditional(FuncContext context, out Bool value)
    {
        if (!context.HasContext)
        {
            value = null;
            return false;
        }

        if (!InputTrue.InternalProperty.ValueType.IsAssignableTo(typeof(Delegate)) ||
            !InputFalse.InternalProperty.ValueType.IsAssignableTo(typeof(Delegate)))
        {
            value = null;
            return false;
        }

        value = context.GetValue(Condition);
        return true;
    }

    public override Node CreateCopy()
    {
        return new SwitchNode();
    }

    private void AddTrueFuncInputHandlers<TValue>(
        TValue constant)
    {
        InputTrue.AddTypeHandler<Func<FuncContext, TValue>>(() => new FuncInputProperty<TValue>(
            this,
            "InputTrue",
            "ON_TRUE",
            constant));
    }

    private void AddFalseFuncInputHandlers<TValue>(
        TValue constant)
    {
        InputFalse.AddTypeHandler<Func<FuncContext, TValue>>(() => new FuncInputProperty<TValue>(
            this,
            "InputFalse",
            "ON_FALSE",
            constant));
    }

    private void AddOutputFuncHandlers<TValue>(
        Func<FuncContext, TValue> handler)
    {
        Output.AddTypeHandler<Func<FuncContext, TValue>>(() => new FuncOutputProperty<TValue>(
            this,
            "Output",
            "RESULT",
            handler));
    }
}
