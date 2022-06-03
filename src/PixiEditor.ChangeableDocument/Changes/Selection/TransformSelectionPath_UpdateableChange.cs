using ChunkyImageLib.Operations;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Selection;
internal class TransformSelectionPath_UpdateableChange : UpdateableChange
{
    private SKPath? originalPath;
    private ShapeCorners originalCorners;
    private ShapeCorners newCorners;

    [GenerateUpdateableChangeActions]
    public TransformSelectionPath_UpdateableChange(ShapeCorners corners)
    {
        this.newCorners = corners;
    }

    [UpdateChangeMethod]
    public void Update(ShapeCorners corners)
    {
        this.newCorners = corners;
    }

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        if (target.Selection.SelectionPath.IsEmpty)
            return new Error();
        originalPath = new(target.Selection.SelectionPath);
        var bounds = originalPath.TightBounds;
        originalCorners = new(bounds);
        return new Success();
    }

    private Selection_ChangeInfo CommonApply(Document target)
    {
        SKPath newPath = new(originalPath);

        var matrix = SKMatrix.CreateTranslation((float)-originalCorners.TopLeft.X, (float)-originalCorners.TopLeft.Y).PostConcat(
            OperationHelper.CreateMatrixFromPoints(newCorners, originalCorners.RectSize));
        newPath.Transform(matrix);

        var toDispose = target.Selection.SelectionPath;
        target.Selection.SelectionPath = newPath;
        toDispose.Dispose();

        return new Selection_ChangeInfo();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, out bool ignoreInUndo)
    {
        ignoreInUndo = false;
        return CommonApply(target);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        return CommonApply(target);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var toDispose = target.Selection.SelectionPath;
        target.Selection.SelectionPath = new SKPath(originalPath);
        toDispose.Dispose();
        return new Selection_ChangeInfo();
    }

    public override void Dispose()
    {
        originalPath?.Dispose();
    }
}
