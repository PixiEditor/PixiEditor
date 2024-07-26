using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Math")]
public class MathNode : Node
{
    public FuncOutputProperty<double> Result { get; }

    public InputProperty<MathNodeMode> Mode { get; }
    
    public InputProperty<bool> Clamp { get; }

    public FuncInputProperty<double> X { get; }
    
    public FuncInputProperty<double> Y { get; }
    
    
    public override string DisplayName { get; set; } = "MATH_NODE";
    
    public MathNode()
    {
        Result = CreateFuncOutput(nameof(Result), "RESULT", Calculate);
        Mode = CreateInput(nameof(Mode), "MATH_MODE", MathNodeMode.Add);
        Clamp = CreateInput(nameof(Clamp), "CLAMP", false);
        X = CreateFuncInput(nameof(X), "X", 0d);
        Y = CreateFuncInput(nameof(Y), "Y", 0d);
    }

    private double Calculate(FuncContext context)
    {
        var (x, y) = GetValues(context);

        var result = Mode.Value switch
        {
            MathNodeMode.Add => x + y,
            MathNodeMode.Subtract => x - y,
            MathNodeMode.Multiply => x * y,
            MathNodeMode.Divide => x / y
        };

        if (Clamp.Value)
        {
            result = Math.Clamp(result, 0, 1);
        }
        
        return result;
    }

    private (double x, double y) GetValues(FuncContext context) => (X.Value(context), Y.Value(context));


    protected override Surface? OnExecute(RenderingContext context)
    {
        return null;
    }


    public override Node CreateCopy() => new MathNode();
}
