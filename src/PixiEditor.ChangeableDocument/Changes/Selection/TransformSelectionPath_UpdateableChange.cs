using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Selection;
internal class TransformSelectionPath_UpdateableChange : UpdateableChange
{
    private SKPath? originalPath;
    private RectI originalTightBounds;
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
        originalTightBounds = (RectI)originalPath.TightBounds;
        return new Success();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        ignoreInUndo = false;
        return SelectionChangeHelper.DoSelectionTransform(target, originalPath!, originalTightBounds, newCorners);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        return SelectionChangeHelper.DoSelectionTransform(target, originalPath!, originalTightBounds, newCorners);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var toDispose = target.Selection.SelectionPath;
        target.Selection.SelectionPath = new SKPath(originalPath);
        toDispose.Dispose();
        return new Selection_ChangeInfo(new SKPath(target.Selection.SelectionPath));
    }

    public override void Dispose()
    {
        originalPath?.Dispose();
    }
}
