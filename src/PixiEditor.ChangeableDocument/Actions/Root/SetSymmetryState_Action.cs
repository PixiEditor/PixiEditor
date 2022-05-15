using PixiEditor.ChangeableDocument.Changes;
using PixiEditor.ChangeableDocument.Changes.Root;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.Actions.Root;
public record class SetSymmetryState_Action : IMakeChangeAction
{
    public SetSymmetryState_Action(SymmetryDirection direction, bool state)
    {
        Direction = direction;
        State = state;
    }

    public SymmetryDirection Direction { get; }
    public bool State { get; }

    Change IMakeChangeAction.CreateCorrespondingChange()
    {
        return new SymmetryState_Change(Direction, State);
    }
}
