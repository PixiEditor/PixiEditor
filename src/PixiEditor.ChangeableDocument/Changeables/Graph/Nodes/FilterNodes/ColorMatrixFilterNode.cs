using PixiEditor.DrawingApi.Core.Surfaces.Surface.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

public class ColorMatrixFilterNode : FilterNode
{
    public InputProperty<ColorMatrix> Matrix { get; }

    public override string DisplayName { get; set; } = "COLOR_MATRIX_FILTER_NODE";
    
    public ColorMatrixFilterNode()
    {
        Matrix = CreateInput(nameof(Matrix), "MATRIX", ColorMatrix.Identity);
    }

    protected override string NodeUniqueName => "ColorMatrixFilter";

    protected override ColorFilter? GetColorFilter() => ColorFilter.CreateColorMatrix(Matrix.Value);

    public override Node CreateCopy() => new ColorMatrixFilterNode();
}
