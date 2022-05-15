using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.Changes.Root;
internal class SymmetryAxisState_Change : Change
{
    private readonly SymmetryAxisDirection direction;
    private readonly bool newEnabled;
    private bool originalEnabled;

    public SymmetryAxisState_Change(SymmetryAxisDirection direction, bool enabled)
    {
        this.direction = direction;
        this.newEnabled = enabled;
    }

    public override void Initialize(Document target)
    {
        originalEnabled = direction switch
        {
            SymmetryAxisDirection.Horizontal => target.HorizontalSymmetryAxisEnabled,
            SymmetryAxisDirection.Vertical => target.VerticalSymmetryAxisEnabled,
            _ => throw new NotImplementedException(),
        };
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

    public override IChangeInfo? Apply(Document target, out bool ignoreInUndo)
    {
        if (originalEnabled == newEnabled)
        {
            ignoreInUndo = true;
            return null;
        }
        SetState(target, newEnabled);
        ignoreInUndo = false;
        return new SymmetryAxisState_ChangeInfo() { Direction = direction };
    }

    public override IChangeInfo? Revert(Document target)
    {
        if (originalEnabled == newEnabled)
            return null;
        SetState(target, originalEnabled);
        return new SymmetryAxisState_ChangeInfo() { Direction = direction };
    }

    public override bool IsMergeableWith(Change other)
    {
        return other is SymmetryAxisState_Change;
    }
}
