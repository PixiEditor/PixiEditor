using Drawie.Backend.Core.Vector;

namespace PixiEditor.ChangeableDocument.Changes.Selection;

internal class ClearSelection_Change : Change
{
    private VectorPath? originalPath;

    [GenerateMakeChangeAction]
    public ClearSelection_Change() { }

    public override bool InitializeAndValidate(Document target)
    {
        if (target.Selection.SelectionPath.IsEmpty)
            return false;
        originalPath = new VectorPath(target.Selection.SelectionPath);
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        (var toDispose, target.Selection.SelectionPath) = (target.Selection.SelectionPath, new VectorPath());
        toDispose.Dispose();

        ignoreInUndo = false;
        return new Selection_ChangeInfo(new VectorPath());
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        (var toDispose, target.Selection.SelectionPath) = (target.Selection.SelectionPath, new VectorPath(originalPath!));
        toDispose.Dispose();

        return new Selection_ChangeInfo(new VectorPath(originalPath!));
    }

    public override void Dispose()
    {
        originalPath?.Dispose();
    }
}
