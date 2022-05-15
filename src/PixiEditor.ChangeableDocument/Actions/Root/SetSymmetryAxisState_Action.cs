using PixiEditor.ChangeableDocument.Changes;
using PixiEditor.ChangeableDocument.Changes.Root;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.Actions.Root;
public record class SetSymmetryAxisState_Action : IMakeChangeAction
{
    public SetSymmetryAxisState_Action(SymmetryAxisDirection direction, bool state)
    {
        Direction = direction;
        State = state;
    }

    public SymmetryAxisDirection Direction { get; }
    public bool State { get; }

    Change IMakeChangeAction.CreateCorrespondingChange()
    {
        return new SymmetryAxisState_Change(Direction, State);
    }
}
