using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class MatrixTransformNode : FilterNode
{
    public InputProperty<ColorMatrix> Matrix { get; }

    public override string DisplayName { get; set; } = "COLOR_MATRIX_FILTER_NODE";
    
    public MatrixTransformNode()
    {
        Matrix = CreateInput(nameof(Matrix), "MATRIX", ColorMatrix.Identity);
    }

    protected override string NodeUniqueName => "ColorMatrixFilter";

    protected override ColorFilter? GetColorFilter() => ColorFilter.CreateColorMatrix(Matrix.Value);

    public override Node CreateCopy() => new MatrixTransformNode();
}
