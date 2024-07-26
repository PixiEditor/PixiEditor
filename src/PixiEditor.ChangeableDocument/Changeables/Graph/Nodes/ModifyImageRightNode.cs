using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("ModifyImageRight")]
[PairNode(typeof(ModifyImageLeftNode), "ModifyImageZone")]
public class ModifyImageRightNode : Node, IPairNodeEnd
{
    public Node StartNode { get; set; }

    private Paint drawingPaint = new Paint() { BlendMode = BlendMode.Src };

    public FuncInputProperty<VecD> Coordinate { get; }
    public FuncInputProperty<Color> Color { get; }

    public OutputProperty<Surface> Output { get; }

    public override string DisplayName { get; set; } = "MODIFY_IMAGE_RIGHT_NODE";

    public ModifyImageRightNode()
    {
        Coordinate = CreateFuncInput(nameof(Coordinate), "UV", new VecD());
        Color = CreateFuncInput(nameof(Color), "COLOR", new Color());
        Output = CreateOutput<Surface>(nameof(Output), "OUTPUT", null);
    }

    protected override Surface? OnExecute(RenderingContext renderingContext)
    {
        if (StartNode == null)
        {
            FindStartNode();
            if (StartNode == null)
            {
                return null;
            }
        }

        var startNode = StartNode as ModifyImageLeftNode;
        if (startNode.Image.Value is not { Size: var size })
        {
            return null;
        }

        startNode.PreparePixmap();

        var width = size.X;
        var height = size.Y;

        var surface = new Surface(size);

        var context = new FuncContext();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                context.UpdateContext(new VecD((double)x / width, (double)y / height), new VecI(width, height));
                var color = Color.Value(context);
                var uv = Coordinate.Value(context);
                var position = new VecD(uv.X * width, uv.Y * height);
                
                drawingPaint.Color = color;

                surface.DrawingSurface.Canvas.DrawPixel((int)position.X, (int)position.Y, drawingPaint);
            }
        }

        Output.Value = surface;

        return Output.Value;
    }

    private void FindStartNode()
    {
        TraverseBackwards(node =>
        {
            if (node is ModifyImageLeftNode leftNode)
            {
                StartNode = leftNode;
                return false;
            }

            return true;
        });
    }

    public override Node CreateCopy() => new ModifyImageRightNode();
}
