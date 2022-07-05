using PixiEditor.ChangeableDocument.Enums;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Selection;

internal class SelectEllipse_UpdateableChange : UpdateableChange
{
    private RectI borders;
    private readonly SelectionMode mode;
    private SKPath? originalPath;

    [GenerateUpdateableChangeActions]
    public SelectEllipse_UpdateableChange(RectI borders, SelectionMode mode)
    {
        this.borders = borders;
        this.mode = mode;
    }

    [UpdateChangeMethod]
    public void Update(RectI borders)
    {
        this.borders = borders;
    }

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        originalPath = new SKPath(target.Selection.SelectionPath);
        return new Success();
    }

    private Selection_ChangeInfo CommonApply(Document target)
    {
        using var ellipsePath = new SKPath() { FillType = SKPathFillType.EvenOdd };
        ellipsePath.AddOval(borders);

        var toDispose = target.Selection.SelectionPath;
        if (mode == SelectionMode.New)
            target.Selection.SelectionPath = new(ellipsePath);
        else
            target.Selection.SelectionPath = originalPath!.Op(ellipsePath, mode.ToSKPathOp());
        toDispose.Dispose();

        return new Selection_ChangeInfo(new SKPath(target.Selection.SelectionPath));
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        return CommonApply(target);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
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
