using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class ClearSelection_Change : Change
{
    private SKPath? originalPath;

    [GenerateMakeChangeAction]
    public ClearSelection_Change() { }

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        if (target.Selection.SelectionPath.IsEmpty)
            return new Error();
        originalPath = new SKPath(target.Selection.SelectionPath);
        return new Success();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, out bool ignoreInUndo)
    {
        target.Selection.SelectionPath.Dispose();
        target.Selection.SelectionPath = new SKPath();

        ignoreInUndo = false;
        return new Selection_ChangeInfo();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.Selection.SelectionPath.Dispose();
        target.Selection.SelectionPath = new SKPath(originalPath);

        return new Selection_ChangeInfo();
    }

    public override void Dispose()
    {
        originalPath?.Dispose();
    }
}
