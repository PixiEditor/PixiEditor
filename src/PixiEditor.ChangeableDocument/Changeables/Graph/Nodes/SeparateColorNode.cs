using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surface.ImageData;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class SeparateColorNode : Node
{
    public FieldInputProperty<Color> Color { get; }
    
    public FieldOutputProperty<double> R { get; }
    
    public FieldOutputProperty<double> G { get; }
    
    public FieldOutputProperty<double> B { get; }
    
    public FieldOutputProperty<double> A { get; }

    public SeparateColorNode()
    {
        Color = CreateFieldInput(nameof(Color), "COLOR", _ => new Color());
        R = CreateFieldOutput(nameof(R), "R", ctx => Color.Value(ctx).R / 255d);
        G = CreateFieldOutput(nameof(G), "G", ctx => Color.Value(ctx).G / 255d);
        B = CreateFieldOutput(nameof(B), "B", ctx => Color.Value(ctx).B / 255d);
        A = CreateFieldOutput(nameof(A), "A", ctx => Color.Value(ctx).A / 255d);
    }

    protected override Image? OnExecute(KeyFrameTime frameTime)
    {
        return null;
    }

    public override bool Validate() => true;

    public override Node CreateCopy() => new SeparateColorNode();
}
