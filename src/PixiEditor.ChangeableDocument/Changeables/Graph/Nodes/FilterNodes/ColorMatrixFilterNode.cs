using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("ColorMatrixFilter", "COLOR_MATRIX_TRANSFORM_FILTER_NODE")]
public class ColorMatrixFilterNode : FilterNode
{
    public InputProperty<ColorMatrix> Matrix { get; }

    public ColorMatrixFilterNode()
    {
        Matrix = CreateInput(nameof(Matrix), "MATRIX", ColorMatrix.Identity);
    }

    protected override ColorFilter? GetColorFilter() => ColorFilter.CreateColorMatrix(Matrix.Value);

    public override Node CreateCopy() => new ColorMatrixFilterNode();
}
