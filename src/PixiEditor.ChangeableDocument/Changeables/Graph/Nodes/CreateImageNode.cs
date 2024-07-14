using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class CreateImageNode : Node
{
    public InputProperty<int> Width { get; }
    public InputProperty<int> Height { get; }
    public FieldInputProperty<Color> Color { get; }
    public OutputProperty<ChunkyImage> Output { get; }
    
    public CreateImageNode() 
    {
        Width = CreateInput("Width", "WIDTH", 32);
        Height = CreateInput("Height", "HEIGHT", 32);
        Color = CreateFieldInput("Color", "COLOR", _ => new Color(0, 0, 0, 255));
        Output = CreateOutput<ChunkyImage>("Output", "OUTPUT", null);
    }

    protected override ChunkyImage? OnExecute(KeyFrameTime frameTime)
    {
        var width = Width.Value;
        var height = Height.Value;
        
        Output.Value = new ChunkyImage(new VecI(Width.Value, Height.Value));

        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < height; x++)
            {
                var context = new CreateImageContext(new VecD((double)x / width, (double)y / width));
                var color = Color.Value(context);

                Output.Value.EnqueueDrawPixel(new VecI(x, y), color, BlendMode.Src);
            }
        }

        Output.Value.CommitChanges();

        return Output.Value;
    }

    public override bool Validate()
    {
        return Width.Value > 0 && Height.Value > 0;
    }

    public override Node CreateCopy() => new CreateImageNode();

    record CreateImageContext(VecD Position) : IFieldContext
    {
    }
}
