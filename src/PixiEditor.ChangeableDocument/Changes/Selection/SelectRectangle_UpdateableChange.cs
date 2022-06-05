using PixiEditor.ChangeableDocument.Enums;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Selection;

internal class SelectRectangle_UpdateableChange : UpdateableChange
{
    private SKPath? originalPath;
    private RectI rect;
    private readonly SelectionMode mode;

    [GenerateUpdateableChangeActions]
    public SelectRectangle_UpdateableChange(RectI rect, SelectionMode mode)
    {
        this.rect = rect;
        this.mode = mode;
    }
    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        originalPath = new SKPath(target.Selection.SelectionPath);
        return new Success();
    }

    [UpdateChangeMethod]
    public void Update(RectI rect)
    {
        this.rect = rect;
    }

    private Selection_ChangeInfo CommonApply(Document target)
    {
        using var rectPath = new SKPath() { FillType = SKPathFillType.EvenOdd };
        rectPath.MoveTo(rect.TopLeft);
        rectPath.LineTo(rect.TopRight);
        rectPath.LineTo(rect.BottomRight);
        rectPath.LineTo(rect.BottomLeft);
        rectPath.Close();

        var toDispose = target.Selection.SelectionPath;
        if (mode == SelectionMode.New)
            target.Selection.SelectionPath = new(rectPath);
        else
            target.Selection.SelectionPath = originalPath!.Op(rectPath, mode.ToSKPathOp());
        toDispose.Dispose();

        return new Selection_ChangeInfo(new SKPath(target.Selection.SelectionPath));
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        return CommonApply(target);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, out bool ignoreInUndo)
    {
        var changes = CommonApply(target);
        ignoreInUndo = false;
        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        (var toDispose, target.Selection.SelectionPath) = (target.Selection.SelectionPath, new SKPath(originalPath));
        toDispose.Dispose();
        return new Selection_ChangeInfo(new SKPath(target.Selection.SelectionPath));
    }

    public override void Dispose()
    {
        originalPath?.Dispose();
    }
}
