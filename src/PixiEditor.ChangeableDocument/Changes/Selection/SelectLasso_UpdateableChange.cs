using PixiEditor.ChangeableDocument.Enums;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Selection;
internal class SelectLasso_UpdateableChange : UpdateableChange
{
    private SKPath? originalPath;
    private SKPath path = new() { FillType = SKPathFillType.EvenOdd };
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
        originalPath = new SKPath(target.Selection.SelectionPath);
        return new Success();
    }

    private Selection_ChangeInfo CommonApply(Document target)
    {
        var toDispose = target.Selection.SelectionPath;
        if (mode == SelectionMode.New)
        {
            var copy = new SKPath(path);
            copy.Close();
            target.Selection.SelectionPath = copy;
        }
        else
        {
            target.Selection.SelectionPath = originalPath!.Op(path, mode.ToSKPathOp());
        }
        toDispose.Dispose();

        return new Selection_ChangeInfo(new SKPath(target.Selection.SelectionPath));
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
        target.Selection.SelectionPath = new(originalPath);
        toDispose.Dispose();
        return new Selection_ChangeInfo(new SKPath(target.Selection.SelectionPath));
    }

    public override void Dispose()
    {
        originalPath?.Dispose();
        path?.Dispose();
    }
}
