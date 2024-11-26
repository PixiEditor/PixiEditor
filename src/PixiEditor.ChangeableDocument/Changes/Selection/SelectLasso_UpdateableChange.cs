using PixiEditor.ChangeableDocument.Enums;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Selection;
internal class SelectLasso_UpdateableChange : UpdateableChange
{
    private RectI constraint;
    private VecI initialPoint;
    private VectorPath? originalPath;
    private VectorPath path = new() { FillType = PathFillType.EvenOdd };
    private readonly SelectionMode mode;

    [GenerateUpdateableChangeActions]
    public SelectLasso_UpdateableChange(VecI point, SelectionMode mode)
    {
        initialPoint = point;
        this.mode = mode;
    }

    [UpdateChangeMethod]
    public void Update(VecI point)
    {
        path.LineTo(point.KeepInside(constraint));
    }

    public override bool InitializeAndValidate(Document target)
    {
        constraint = new RectI(VecI.Zero, target.Size);
        path.MoveTo(initialPoint.KeepInside(constraint));
        originalPath = new VectorPath(target.Selection.SelectionPath);
        return true;
    }

    private Selection_ChangeInfo CommonApply(Document target)
    {
        var toDispose = target.Selection.SelectionPath;
        if (mode == SelectionMode.New)
        {
            var copy = path.PointCount > 2 ? new VectorPath(path) : new VectorPath();
            copy.Close();
            target.Selection.SelectionPath = copy;
        }
        else
        {
            target.Selection.SelectionPath = path.PointCount > 2 ? originalPath!.Op(path, mode.ToVectorPathOp()) : new VectorPath(originalPath!);
        }
        toDispose.Dispose();

        return new Selection_ChangeInfo(new VectorPath(target.Selection.SelectionPath));
    }
    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
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
        target.Selection.SelectionPath = new(originalPath!);
        toDispose.Dispose();
        return new Selection_ChangeInfo(new VectorPath(target.Selection.SelectionPath));
    }

    public override void Dispose()
    {
        originalPath?.Dispose();
        path.Dispose();
    }
}
