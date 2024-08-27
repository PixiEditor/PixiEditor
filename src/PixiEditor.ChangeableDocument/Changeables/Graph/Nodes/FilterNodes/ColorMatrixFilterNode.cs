using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("ColorMatrixFilter", "COLOR_MATRIX_TRANSFORM_FILTER_NODE", Category = "FILTERS")]
public class ColorMatrixFilterNode : FilterNode
{
    public InputProperty<ColorMatrix> Matrix { get; }

    private ColorFilter filter;
    private ColorMatrix lastMatrix;
    
    public ColorMatrixFilterNode()
    {
        Matrix = CreateInput(nameof(Matrix), "MATRIX", ColorMatrix.Identity);
    }

    protected override ColorFilter? GetColorFilter()
    {
        if (Matrix.Value.Equals(lastMatrix))
        {
            return filter;
        }
        
        lastMatrix = Matrix.Value;
        filter?.Dispose();
        
        filter = ColorFilter.CreateColorMatrix(Matrix.Value);
        return filter;
    }

    public override Node CreateCopy() => new ColorMatrixFilterNode();
}
