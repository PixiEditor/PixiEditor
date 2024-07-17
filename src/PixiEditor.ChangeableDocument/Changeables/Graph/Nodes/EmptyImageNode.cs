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
    
    public OutputProperty<ChunkyImage> Output { get; }

    public InputProperty<VecI> Size { get; }
    
    public InputProperty<Color> Fill { get; }

    public CreateImageNode()
    {
        Output = CreateOutput<ChunkyImage>(nameof(Output), "EMPTY_IMAGE", null);
        Size = CreateInput(nameof(Size), "SIZE", new VecI(32, 32));
        Fill = CreateInput(nameof(Fill), "FILL", new Color(0, 0, 0, 255));
    }
    
    private ChunkyImage? Image { get; set; }
    
    protected override Chunk? OnExecute(RenderingContext context)
    {
        if(Image == null || Image.LatestSize != Size.Value)
        {
            Image = new ChunkyImage(Size.Value);
        }
        
        _paint.Color = Fill.Value;
        Image.EnqueueDrawPaint(_paint);

        Chunk result = Chunk.Create(context.ChunkResolution);
        Image.DrawMostUpToDateChunkOn(context.ChunkToUpdate, context.ChunkResolution, result.Surface.DrawingSurface, VecI.Zero);

        Output.Value = Image;

        return result;
    }

    public override bool Validate() => Size.Value is { X: > 0, Y: > 0 };

    public override Node CreateCopy() => new CreateImageNode();
}
