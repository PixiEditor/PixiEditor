using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.Changes.Root;
internal class SymmetryAxisState_Change : Change
{
    private readonly SymmetryAxisDirection direction;
    private readonly bool newEnabled;
    private bool originalEnabled;

    [GenerateMakeChangeAction]
    public SymmetryAxisState_Change(SymmetryAxisDirection direction, bool enabled)
    {
        this.direction = direction;
        this.newEnabled = enabled;
    }

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        originalEnabled = direction switch
        {
            SymmetryAxisDirection.Horizontal => target.HorizontalSymmetryAxisEnabled,
            SymmetryAxisDirection.Vertical => target.VerticalSymmetryAxisEnabled,
            _ => throw new NotImplementedException(),
        };
        if (originalEnabled == newEnabled)
            return new Error();
        return new Success();
    }

    private void SetState(Document target, bool state)
    {
        if (direction == SymmetryAxisDirection.Horizontal)
            target.HorizontalSymmetryAxisEnabled = state;
        else if (direction == SymmetryAxisDirection.Vertical)
            target.VerticalSymmetryAxisEnabled = state;
        else
            throw new NotImplementedException();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        SetState(target, newEnabled);
        ignoreInUndo = false;
        return new SymmetryAxisState_ChangeInfo(direction, newEnabled);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        SetState(target, originalEnabled);
        return new SymmetryAxisState_ChangeInfo(direction, originalEnabled);
    }

    public override bool IsMergeableWith(Change other)
    {
        return other is SymmetryAxisState_Change change && change.direction == direction;
    }
}
