using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class ModifyImageRightNode : Node
{
    private ModifyImageLeftNode startNode;
    
    private Paint drawingPaint = new Paint() { BlendMode = BlendMode.Src };
    
    public FieldInputProperty<Color> Color { get; }
    
    public OutputProperty<Surface> Output { get; }
    
    public ModifyImageRightNode(ModifyImageLeftNode startNode)
    {
        this.startNode = startNode;
        Color = CreateFieldInput(nameof(Color), "COLOR", new Color());
        Output = CreateOutput<Surface>(nameof(Output), "OUTPUT", null);
    }

    protected override Surface? OnExecute(RenderingContext renderingContext)
    {
        if (startNode.Image.Value is not { Size: var size })
        {
            return null;
        }
        
        startNode.PreparePixmap();
        
        var width = size.X;
        var height = size.Y;

        var surface = new Surface(size);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var context = new FieldContext(new VecD((double)x / width, (double)y / height), new VecI(width, height));
                var color = Color.Value(context);

                drawingPaint.Color = color;
                surface.DrawingSurface.Canvas.DrawPixel(x, y, drawingPaint);
            }
        }

        Output.Value = surface;

        return Output.Value;
    }

    public override bool Validate() => true;

    public override Node CreateCopy() => throw new NotImplementedException();
}
