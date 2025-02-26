using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("ColorMatrixFilter")]
public class ColorMatrixFilterNode : FilterNode
{
    public InputProperty<ColorMatrix> Matrix { get; }

    private ColorFilter filter;
    private ColorMatrix lastMatrix;
    
    public ColorMatrixFilterNode()
    {
        Matrix = CreateInput(nameof(Matrix), "MATRIX", ColorMatrix.Identity);
    }

    protected override ColorFilter? GetColorFilter(ColorSpace colorSpace)
    {
        if (Matrix.Value.Equals(lastMatrix))
        {
            return filter;
        }
        
        lastMatrix = Matrix.Value;
        filter?.Dispose();
        
        filter = ColorFilter.CreateColorMatrix(AdjustMatrixForColorSpace(Matrix.Value));
        return filter;
    }

    public override Node CreateCopy() => new ColorMatrixFilterNode();
}
