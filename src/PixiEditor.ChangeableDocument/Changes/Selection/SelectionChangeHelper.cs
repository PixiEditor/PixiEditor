using ChunkyImageLib.Operations;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Selection;

internal class SelectionChangeHelper
{
    public static Selection_ChangeInfo ResizeSelection(
        Document target, VectorPath originalPath, Matrix3X3 transformation, VecI newSize)
    {
        using VectorPath transformedPath = new(originalPath) { FillType = PathFillType.EvenOdd };
        transformedPath.Transform(transformation);

        using VectorPath documentBounds = new() { FillType = PathFillType.EvenOdd };
        documentBounds.AddRect(new RectD(VecI.Zero, newSize));

        VectorPath constrainedPath = transformedPath.Op(documentBounds, VectorPathOp.Intersect);
        constrainedPath.FillType = PathFillType.EvenOdd;
        return ReplaceSelection(target, constrainedPath);
    }

    public static Selection_ChangeInfo RestoreSelection(Document target, VectorPath originalPath)
    {
        return ReplaceSelection(target, new VectorPath(originalPath) { FillType = PathFillType.EvenOdd });
    }

    private static Selection_ChangeInfo ReplaceSelection(Document target, VectorPath selection)
    {
        var toDispose = target.Selection.SelectionPath;
        target.Selection.SelectionPath = selection;
        toDispose.Dispose();

        return new Selection_ChangeInfo(new VectorPath(selection));
    }

    public static Selection_ChangeInfo DoSelectionTransform(
        Document target, VectorPath originalPath, RectD originalPathTightBounds, ShapeCorners to)
    {
        VectorPath newPath = new(originalPath);

        var matrix = Matrix3X3.CreateTranslation((float)-originalPathTightBounds.X, (float)-originalPathTightBounds.Y)
            .PostConcat(
                OperationHelper.CreateMatrixFromPoints(to, originalPathTightBounds.Size));
        newPath.Transform(matrix);
        var toDispose = target.Selection.SelectionPath;
        target.Selection.SelectionPath = newPath;
        toDispose.Dispose();

        return new Selection_ChangeInfo(new VectorPath(target.Selection.SelectionPath));
    }

    public static IChangeInfo DoSelectionTransform(Document target, VectorPath originalPath,
        RectD originalPathTightBounds, ShapeCorners to, RectD cornersToSelectionOffset, VecD originalCornersSize)
    {
        VectorPath newPath = new(originalPath);

        var matrix =
            Matrix3X3.CreateTranslation(-(float)cornersToSelectionOffset.X, -(float)cornersToSelectionOffset.Y);

        matrix = matrix.PostConcat(Matrix3X3.CreateTranslation(
            (float)(-originalPathTightBounds.X),
            (float)(-originalPathTightBounds.Y)));
        
        matrix = matrix.PostConcat(OperationHelper.CreateMatrixFromPoints(to, originalCornersSize));
        
        newPath.Transform(matrix);
        var toDispose = target.Selection.SelectionPath;
        target.Selection.SelectionPath = newPath;
        toDispose.Dispose();

        return new Selection_ChangeInfo(new VectorPath(target.Selection.SelectionPath));
    }
}
