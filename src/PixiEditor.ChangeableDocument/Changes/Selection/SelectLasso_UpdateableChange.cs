using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface.Vector;

namespace PixiEditor.ChangeableDocument.Changes.Selection;
internal class SelectLasso_UpdateableChange : UpdateableChange
{
    private VectorPath? originalPath;
    private VectorPath path = new() { FillType = PathFillType.EvenOdd };
    private readonly SelectionMode mode;

    [GenerateUpdateableChangeActions]
    public SelectLasso_UpdateableChange(VecI point, SelectionMode mode)
    {
        path.MoveTo(point);
        this.mode = mode;
    }

    [UpdateChangeMethod]
    public void Update(VecI point)
    {
        path.LineTo(point);
    }

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        originalPath = new VectorPath(target.Selection.SelectionPath);
        return new Success();
    }

    private Selection_ChangeInfo CommonApply(Document target)
    {
        var toDispose = target.Selection.SelectionPath;
        if (mode == SelectionMode.New)
        {
            var copy = new VectorPath(path);
            copy.Close();
            target.Selection.SelectionPath = copy;
        }
        else
        {
            target.Selection.SelectionPath = originalPath!.Op(path, mode.ToVectorPathOp());
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
