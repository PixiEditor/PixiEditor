using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface.Vector;

namespace PixiEditor.ChangeableDocument.Changes.Selection;

internal class SelectRectangle_UpdateableChange : UpdateableChange
{
    private VectorPath? originalPath;
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
        originalPath = new VectorPath(target.Selection.SelectionPath);
        return new Success();
    }

    [UpdateChangeMethod]
    public void Update(RectI rect)
    {
        this.rect = rect;
    }

    private Selection_ChangeInfo CommonApply(Document target)
    {
        using var rectPath = new VectorPath() { FillType = PathFillType.EvenOdd };
        if (!rect.IsZeroArea)
        {
            rectPath.MoveTo(rect.TopLeft);
            rectPath.LineTo(rect.TopRight);
            rectPath.LineTo(rect.BottomRight);
            rectPath.LineTo(rect.BottomLeft);
            rectPath.Close();
        }

        var toDispose = target.Selection.SelectionPath;
        if (mode == SelectionMode.New)
            target.Selection.SelectionPath = new(rectPath);
        else
            target.Selection.SelectionPath = originalPath!.Op(rectPath, mode.ToVectorPathOp());
        toDispose.Dispose();

        return new Selection_ChangeInfo(new VectorPath(target.Selection.SelectionPath));
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
        (var toDispose, target.Selection.SelectionPath) = (target.Selection.SelectionPath, new VectorPath(originalPath!));
        toDispose.Dispose();
        return new Selection_ChangeInfo(new VectorPath(target.Selection.SelectionPath));
    }

    public override void Dispose()
    {
        originalPath?.Dispose();
    }
}
