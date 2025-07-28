using PixiEditor.ChangeableDocument.Enums;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

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
    public override bool InitializeAndValidate(Document target)
    {
        originalPath = new VectorPath(target.Selection.SelectionPath);
        return true;
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
            var constrained = rect.Intersect(new RectI(VecI.Zero, target.Size));
            rectPath.MoveTo(constrained.TopLeft);
            rectPath.LineTo(constrained.TopRight);
            rectPath.LineTo(constrained.BottomRight);
            rectPath.LineTo(constrained.BottomLeft);
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
