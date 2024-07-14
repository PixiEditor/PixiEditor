using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class SampleImageNode : Node
{
    public InputProperty<ChunkyImage?> Image { get; }
    
    public FieldInputProperty<VecD> Coordinate { get; }
    
    public FieldOutputProperty<Color> Color { get; }
    
    public SampleImageNode()
    {
        Image = CreateInput<ChunkyImage>(nameof(Image), "IMAGE", null);
        Coordinate = CreateFieldInput(nameof(Coordinate), "COORDINATE", ctx => ctx.Position);
        Color = CreateFieldOutput(nameof(Color), "COLOR", GetColor);
    }

    private Color GetColor(IFieldContext context)
    {
        if (Image.Value is not { } image)
        {
            return new Color();
        }
        
        var pos = new VecI(
            (int)(context.Position.X * context.Size.X),
            (int)(context.Position.Y * context.Size.Y));
        
        return image.GetCommittedPixel(pos);
    }

    protected override ChunkyImage? OnExecute(KeyFrameTime frameTime)
    {
        return Image.Value;
    }

    public override bool Validate() => Image.Value != null;

    public override Node CreateCopy() => new SampleImageNode();
}
