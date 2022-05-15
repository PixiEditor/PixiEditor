using PixiEditor.ChangeableDocument.Changes;
using PixiEditor.ChangeableDocument.Changes.Root;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.Actions.Root.SymmetryPosition;
public class SetSymmetryPosition_Action : IStartOrUpdateChangeAction
{
    public SetSymmetryPosition_Action(SymmetryDirection direction, int position)
    {
        Direction = direction;
        Position = position;
    }

    public SymmetryDirection Direction { get; }
    public int Position { get; }

    UpdateableChange IStartOrUpdateChangeAction.CreateCorrespondingChange()
    {
        return new SymmetryPosition_UpdateableChange(Direction, Position);
    }

    void IStartOrUpdateChangeAction.UpdateCorrespodingChange(UpdateableChange change)
    {
        ((SymmetryPosition_UpdateableChange)change).Update(Position);
    }
}
