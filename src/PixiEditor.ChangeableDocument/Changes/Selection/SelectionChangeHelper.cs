using ChunkyImageLib.Operations;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface.Vector;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Selection;
internal class SelectionChangeHelper
{
    public static Selection_ChangeInfo DoSelectionTransform(
        Document target, VectorPath originalPath, RectD originalPathTightBounds, ShapeCorners to)
    {
        VectorPath newPath = new(originalPath);

        var matrix = Matrix3X3.CreateTranslation((float)-originalPathTightBounds.X, (float)-originalPathTightBounds.Y).PostConcat(
            OperationHelper.CreateMatrixFromPoints(to, originalPathTightBounds.Size));
        newPath.Transform(matrix);
        var toDispose = target.Selection.SelectionPath;
        target.Selection.SelectionPath = newPath;
        toDispose.Dispose();

        return new Selection_ChangeInfo(new VectorPath(target.Selection.SelectionPath));
    }
}
