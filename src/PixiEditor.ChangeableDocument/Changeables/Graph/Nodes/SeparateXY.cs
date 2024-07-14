using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class SeparateVecI : Node
{
    public FieldInputProperty<VecD> Vector { get; }
    
    public FieldOutputProperty<double> X { get; }
    
    public FieldOutputProperty<double> Y { get; }

    public SeparateVecI()
    {
        X = CreateFieldOutput("X", "X", ctx => Vector.Value(ctx).X);
        Y = CreateFieldOutput("Y", "Y", ctx => Vector.Value(ctx).Y);
        Vector = CreateFieldInput("Vector", "VECTOR", _ => new VecD(0, 0));
    }

    protected override ChunkyImage? OnExecute(KeyFrameTime frameTime)
    {
        return null;
    }

    public override bool Validate() => true;

    public override Node CreateCopy() => new SeparateVecI();
}
