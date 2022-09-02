using ChunkyImageLib.Operations;
using PixiEditor.DrawingApi.Core.Numerics;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Selection;
internal class SelectionChangeHelper
{
    public static Selection_ChangeInfo DoSelectionTransform(
        Document target, SKPath originalPath, RectI originalPathTightBounds, ShapeCorners to)
    {
        SKPath newPath = new(originalPath);

        var matrix = SKMatrix.CreateTranslation((float)-originalPathTightBounds.X, (float)-originalPathTightBounds.Y).PostConcat(
            OperationHelper.CreateMatrixFromPoints(to, originalPathTightBounds.Size));
        newPath.Transform(matrix);

        var toDispose = target.Selection.SelectionPath;
        target.Selection.SelectionPath = newPath;
        toDispose.Dispose();

        return new Selection_ChangeInfo(new SKPath(target.Selection.SelectionPath));
    }
}
