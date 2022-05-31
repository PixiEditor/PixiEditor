using PixiEditor.ChangeableDocument.Enums;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Selection;

internal class SelectRectangle_UpdateableChange : UpdateableChange
{
    private VecI pos;
    private VecI size;
    private SKPath? originalPath;
    private readonly SelectionMode mode;

    [GenerateUpdateableChangeActions]
    public SelectRectangle_UpdateableChange(VecI pos, VecI size, SelectionMode mode)
    {
        Update(pos, size);
        this.mode = mode;
    }
    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        originalPath = new SKPath(target.Selection.SelectionPath);
        return new Success();
    }

    [UpdateChangeMethod]
    public void Update(VecI pos, VecI size)
    {
        this.pos = pos;
        this.size = size;
    }

    private Selection_ChangeInfo CommonApply(Document target)
    {
        using var rect = new SKPath();
        rect.MoveTo(pos);
        rect.LineTo(pos.X + size.X, pos.Y);
        rect.LineTo(pos + size);
        rect.LineTo(pos.X, pos.Y + size.Y);
        rect.LineTo(pos);

        var toDispose = target.Selection.SelectionPath;
        if (mode == SelectionMode.New)
            target.Selection.SelectionPath = new(rect);
        else
            target.Selection.SelectionPath = originalPath!.Op(rect, mode.ToSKPathOp());
        toDispose.Dispose();

        return new Selection_ChangeInfo();
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
        var changes = new Selection_ChangeInfo();
        target.Selection.SelectionPath.Dispose();
        target.Selection.SelectionPath = new SKPath(originalPath);
        return changes;
    }

    public override void Dispose()
    {
        originalPath?.Dispose();
    }
}
