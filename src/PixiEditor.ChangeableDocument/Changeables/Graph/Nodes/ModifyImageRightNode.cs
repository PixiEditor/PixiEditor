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

    private Surface surface;

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
        
        var width = size.X;
        var height = size.Y;
        
        surface = new Surface(size);

        startNode.PreparePixmap(renderingContext);
        
        using Pixmap targetPixmap = surface.PeekPixels();

        ModifyImageInParallel(renderingContext, targetPixmap, width, height);
        
        startNode.DisposePixmap(renderingContext);

        Output.Value = surface;

        return Output.Value;
    }

    private unsafe void ModifyImageInParallel(RenderingContext renderingContext, Pixmap targetPixmap, int width, int height)
    {
        int threads = Environment.ProcessorCount;
        int chunkHeight = height / threads;

        Parallel.For(0, threads, i =>
        {
            FuncContext context = new(renderingContext);
            
            int startY = i * chunkHeight;
            int endY = (i + 1) * chunkHeight;
            if (i == threads - 1)
            {
                endY = height;
            }

            Half* drawArray = (Half*)targetPixmap.GetPixels();

            for (int y = startY; y < endY; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    context.UpdateContext(new VecD((double)x / width, (double)y / height), new VecI(width, height));
                    var coordinate = Coordinate.Value(context);
                    context.UpdateContext(coordinate, new VecI(width, height));
                    
                    var color = Color.Value(context);
                    ulong colorBits = color.ToULong();
                    
                    int pixelOffset = (y * width + x) * 4;
                    Half* drawPixel = drawArray + pixelOffset;
                    *(ulong*)drawPixel = colorBits;
                }
            }
        });
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
