using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class CreateImageNode : Node
{
    private Paint _paint = new();
    
    public OutputProperty<Image> Output { get; }

    public InputProperty<VecI> Size { get; }
    
    public InputProperty<Color> Fill { get; }

    public CreateImageNode()
    {
        Output = CreateOutput<Image>(nameof(Output), "EMPTY_IMAGE", null);
        Size = CreateInput(nameof(Size), "SIZE", new VecI(32, 32));
        Fill = CreateInput(nameof(Fill), "FILL", new Color(0, 0, 0, 255));
    }
    
    protected override Image? OnExecute(RenderingContext context)
    {
        using var surface = new Surface(Size.Value);

        _paint.Color = Fill.Value;
        surface.DrawingSurface.Canvas.DrawPaint(_paint);

        Output.Value = surface.DrawingSurface.Snapshot();

        return Output.Value;
    }

    public override bool Validate() => Size.Value is { X: > 0, Y: > 0 };

    public override Node CreateCopy() => new CreateImageNode();
}
