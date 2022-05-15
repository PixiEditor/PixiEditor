using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.Changes.Root;
internal class SymmetryState_Change : Change
{
    private readonly SymmetryDirection direction;
    private readonly bool newEnabled;
    private bool originalEnabled;

    public SymmetryState_Change(SymmetryDirection direction, bool enabled)
    {
        this.direction = direction;
        this.newEnabled = enabled;
    }

    public override void Initialize(Document target)
    {
        originalEnabled = direction switch
        {
            SymmetryDirection.Horizontal => target.HorizontalSymmetryEnabled,
            SymmetryDirection.Vertical => target.VerticalSymmetryEnabled,
            _ => throw new NotImplementedException(),
        };
    }

    private void SetState(Document target, bool state)
    {
        if (direction == SymmetryDirection.Horizontal)
            target.HorizontalSymmetryEnabled = state;
        else if (direction == SymmetryDirection.Vertical)
            target.VerticalSymmetryEnabled = state;
        else
            throw new NotImplementedException();
    }

    public override IChangeInfo? Apply(Document target, out bool ignoreInUndo)
    {
        if (originalEnabled == newEnabled)
        {
            ignoreInUndo = true;
            return null;
        }
        SetState(target, newEnabled);
        ignoreInUndo = false;
        return new SymmetryState_ChangeInfo() { Direction = direction };
    }

    public override IChangeInfo? Revert(Document target)
    {
        if (originalEnabled == newEnabled)
            return null;
        SetState(target, originalEnabled);
        return new SymmetryState_ChangeInfo() { Direction = direction };
    }

    public override bool IsMergeableWith(Change other)
    {
        return other is SymmetryState_Change;
    }
}
