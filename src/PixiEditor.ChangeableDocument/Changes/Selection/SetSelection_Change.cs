using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Selection;
internal class SetSelection_Change : Change
{
    private readonly SKPath selection;
    private SKPath? originalSelection;

    [GenerateMakeChangeAction]
    public SetSelection_Change(SKPath selection)
    {
        this.selection = new SKPath(selection) { FillType = SKPathFillType.EvenOdd };
    }

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        originalSelection = ((IReadOnlySelection)target.Selection).SelectionPath;
        return new Success();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        target.Selection.SelectionPath = new SKPath(selection) { FillType = SKPathFillType.EvenOdd };
        ignoreInUndo = false;
        return new Selection_ChangeInfo(new SKPath(selection) { FillType = SKPathFillType.EvenOdd });
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.Selection.SelectionPath = new SKPath(originalSelection) { FillType = SKPathFillType.EvenOdd };
        return new Selection_ChangeInfo(new SKPath(originalSelection) { FillType = SKPathFillType.EvenOdd });
    }
}
