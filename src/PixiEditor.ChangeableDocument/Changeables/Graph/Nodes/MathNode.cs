using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Surface.ImageData;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class MathNode : Node
{
    public FieldOutputProperty<double> Result { get; }

    public InputProperty<MathNodeMode> Mode { get; }
    
    public InputProperty<bool> Clamp { get; }

    public FieldInputProperty<double> X { get; }
    
    public FieldInputProperty<double> Y { get; }
    
    public MathNode()
    {
        Result = CreateFieldOutput(nameof(Result), "RESULT", Calculate);
        Mode = CreateInput(nameof(Mode), "MATH_MODE", MathNodeMode.Add);
        Clamp = CreateInput(nameof(Clamp), "CLAMP", false);
        X = CreateFieldInput(nameof(X), "X", 0d);
        Y = CreateFieldInput(nameof(Y), "Y", 0d);
    }

    private double Calculate(FieldContext context)
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

    private (double x, double y) GetValues(FieldContext context) => (X.Value(context), Y.Value(context));

    protected override string NodeUniqueName => "Math";

    protected override Surface? OnExecute(RenderingContext context)
    {
        return null;
    }

    public override bool Validate() => true;

    public override Node CreateCopy() => new MathNode();
}
