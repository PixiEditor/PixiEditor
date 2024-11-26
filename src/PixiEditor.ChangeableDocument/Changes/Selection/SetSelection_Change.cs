using Drawie.Backend.Core.Vector;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changes.Selection;
internal class SetSelection_Change : Change
{
    private readonly VectorPath selection;
    private VectorPath? originalSelection;

    [GenerateMakeChangeAction]
    public SetSelection_Change(VectorPath selection)
    {
        this.selection = new VectorPath(selection) { FillType = PathFillType.EvenOdd };
    }

    public override bool InitializeAndValidate(Document target)
    {
        originalSelection = ((IReadOnlySelection)target.Selection).SelectionPath;
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        target.Selection.SelectionPath = new VectorPath(selection) { FillType = PathFillType.EvenOdd };
        ignoreInUndo = false;
        return new Selection_ChangeInfo(new VectorPath(selection) { FillType = PathFillType.EvenOdd });
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.Selection.SelectionPath = new VectorPath(originalSelection!) { FillType = PathFillType.EvenOdd };
        return new Selection_ChangeInfo(new VectorPath(originalSelection!) { FillType = PathFillType.EvenOdd });
    }
}
