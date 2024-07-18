using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

public class SeparateVecINode : Node
{
    public FieldInputProperty<VecI> Vector { get; }
    
    public FieldOutputProperty<int> X { get; }
    
    public FieldOutputProperty<int> Y { get; }

    public SeparateVecINode()
    {
        X = CreateFieldOutput("X", "X", ctx => Vector.Value(ctx).X);
        Y = CreateFieldOutput("Y", "Y", ctx => Vector.Value(ctx).Y);
        Vector = CreateFieldInput("Vector", "VECTOR", new VecI(0, 0));
    }

    protected override string NodeUniqueName => "SeparateVecI";

    protected override Surface? OnExecute(RenderingContext context)
    {
        return null;
    }

    public override bool Validate() => true;

    public override Node CreateCopy() => new SeparateVecINode();
}
