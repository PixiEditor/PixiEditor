using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class SeparateVecI : Node
{
    public InputProperty<Func<IFieldContext, VecD>> Vector { get; }
    
    public OutputProperty<Func<IFieldContext, double>> X { get; }
    
    public OutputProperty<Func<IFieldContext, double>> Y { get; }

    public SeparateVecI()
    {
        X = CreateOutput<Func<IFieldContext, double>>("X", "X", ctx => Vector.Value(ctx).X);
        Y = CreateOutput<Func<IFieldContext, double>>("Y", "Y", ctx => Vector.Value(ctx).Y);
        Vector = CreateInput<Func<IFieldContext, VecD>>("Vector", "VECTOR", _ => new VecD(0, 0));
    }
    
    protected override ChunkyImage? OnExecute(KeyFrameTime frameTime)
    {
        return null;
    }

    public override bool Validate() => true;

    public override Node CreateCopy() => new SeparateVecI();
}
