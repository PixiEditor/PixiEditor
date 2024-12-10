using PixiEditor.ChangeableDocument.Enums;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Selection;

internal class SelectEllipse_UpdateableChange : UpdateableChange
{
    private RectI borders;
    private VectorPath? documentConstraint;
    private readonly SelectionMode mode;
    private VectorPath? originalPath;

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

    public override bool InitializeAndValidate(Document target)
    {
        originalPath = new VectorPath(target.Selection.SelectionPath);
        documentConstraint = new VectorPath();
        documentConstraint.AddRect((RectD)new RectI(VecI.Zero, target.Size));
        return true;
    }

    private Selection_ChangeInfo CommonApply(Document target)
    {
        using var ellipsePath = new VectorPath() { FillType = PathFillType.EvenOdd };
        if (!borders.IsZeroArea)
            ellipsePath.AddOval((RectD)borders);

        using var inConstraint = ellipsePath.Op(documentConstraint!, VectorPathOp.Intersect);

        var toDispose = target.Selection.SelectionPath;
        if (mode == SelectionMode.New)
            target.Selection.SelectionPath = new(inConstraint);
        else
            target.Selection.SelectionPath = originalPath!.Op(inConstraint, mode.ToVectorPathOp());
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
        documentConstraint?.Dispose();
    }
}
