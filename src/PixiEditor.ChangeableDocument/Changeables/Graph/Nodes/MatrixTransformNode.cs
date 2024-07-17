using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class MatrixTransformNode : Node
{
    private ColorMatrix previousMatrix = new(
        (1, 0, 0, 0, 0),
        (0, 1, 0, 0, 0),
        (0, 0, 1, 0, 0),
        (0, 0, 0, 1, 0));
    
    private Paint paint;
    
    public OutputProperty<Chunk> Transformed { get; }
    
    public InputProperty<Chunk?> Input { get; }
    
    public InputProperty<ColorMatrix> Matrix { get; }

    public MatrixTransformNode()
    {
        Transformed = CreateOutput<Chunk>(nameof(Transformed), "TRANSFORMED", null);
        Input = CreateInput<Chunk>(nameof(Input), "INPUT", null);
        Matrix = CreateInput(nameof(Matrix), "MATRIX", previousMatrix);

        paint = new Paint { ColorFilter = ColorFilter.CreateColorMatrix(previousMatrix) };
    }
    
    protected override Chunk? OnExecute(RenderingContext context)
    {
        /*var currentMatrix = Matrix.Value;
        if (previousMatrix != currentMatrix)
        {
            paint.ColorFilter = ColorFilter.CreateColorMatrix(Matrix.Value);
            previousMatrix = currentMatrix;
        }

        var workingSurface = new Surface(Input.Value.Size);
        
        workingSurface.DrawingSurface.Canvas.DrawSurface(Input.Value.DrawingSurface, 0, 0, paint);

        Transformed.Value = workingSurface;
        
        return Transformed.Value;*/
        
        return null;
    }

    public override bool Validate() => Input.Value != null;

    public override Node CreateCopy() => new MatrixTransformNode();
}
