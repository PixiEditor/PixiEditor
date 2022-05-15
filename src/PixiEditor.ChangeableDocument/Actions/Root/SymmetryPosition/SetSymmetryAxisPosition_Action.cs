using PixiEditor.ChangeableDocument.Changes;
using PixiEditor.ChangeableDocument.Changes.Root;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.Actions.Root.SymmetryPosition;
public class SetSymmetryAxisPosition_Action : IStartOrUpdateChangeAction
{
    public SetSymmetryAxisPosition_Action(SymmetryAxisDirection direction, int position)
    {
        Direction = direction;
        Position = position;
    }

    public SymmetryAxisDirection Direction { get; }
    public int Position { get; }

    UpdateableChange IStartOrUpdateChangeAction.CreateCorrespondingChange()
    {
        return new SymmetryAxisPosition_UpdateableChange(Direction, Position);
    }

    void IStartOrUpdateChangeAction.UpdateCorrespodingChange(UpdateableChange change)
    {
        ((SymmetryAxisPosition_UpdateableChange)change).Update(Position);
    }
}
