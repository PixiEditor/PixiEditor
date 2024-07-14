using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class ModifyImageRightNode : Node
{
    private Paint drawingPaint = new Paint() { BlendMode = BlendMode.Src };
    
    public InputProperty<ChunkyImage?> _InternalImage { get; }
    
    public FieldInputProperty<Color> Color { get; }
    
    public OutputProperty<ChunkyImage> Output { get; }
    
    public ModifyImageRightNode() 
    {
        _InternalImage = CreateInput<ChunkyImage>(nameof(_InternalImage), "_InternalImage", null);
        Color = CreateFieldInput(nameof(Color), "COLOR", _ => new Color(0, 0, 0, 255));
        Output = CreateOutput<ChunkyImage>(nameof(Output), "OUTPUT", null);
    }

    protected override ChunkyImage? OnExecute(KeyFrameTime frameTime)
    {
        if (_InternalImage.Value == null)
        {
            return null;
        }
        
        var size = _InternalImage.Value.CommittedSize;
        var width = size.X;
        var height = size.Y;

        Output.Value = new ChunkyImage(size);
        using var surface = new Surface(size);

        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < height; x++)
            {
                var context = new FieldContext(new VecD((double)x / width, (double)y / width), new VecI(width, height));
                var color = Color.Value(context);

                drawingPaint.Color = color;
                surface.DrawingSurface.Canvas.DrawPixel(x, y, drawingPaint);
            }
        }

        Output.Value.EnqueueDrawImage(VecI.Zero, surface);
        Output.Value.CommitChanges();

        return Output.Value;
    }

    public override bool Validate() => true;

    public override Node CreateCopy() => new ModifyImageRightNode();
}
