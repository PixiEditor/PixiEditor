using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.Changes.Root;
internal class SymmetryPosition_UpdateableChange : UpdateableChange
{
    private readonly SymmetryDirection direction;
    private int newPos;
    private int originalPos;

    public SymmetryPosition_UpdateableChange(SymmetryDirection direction, int pos)
    {
        this.direction = direction;
        newPos = pos;
    }

    public void Update(int pos)
    {
        newPos = pos;
    }

    public override void Initialize(Document target)
    {
        originalPos = direction switch
        {
            SymmetryDirection.Horizontal => target.HorizontalSymmetryPosition,
            SymmetryDirection.Vertical => target.VerticalSymmetryPosition,
            _ => throw new NotImplementedException(),
        };
    }

    private void SetPosition(Document target, int position)
    {
        if (direction == SymmetryDirection.Horizontal)
            target.HorizontalSymmetryPosition = position;
        else if (direction == SymmetryDirection.Vertical)
            target.VerticalSymmetryPosition = position;
        else
            throw new NotImplementedException();
    }

    public override IChangeInfo? Apply(Document target, out bool ignoreInUndo)
    {
        ignoreInUndo = originalPos == newPos;
        SetPosition(target, newPos);
        return new SymmetryPosition_ChangeInfo() { Direction = direction };
    }

    public override IChangeInfo? ApplyTemporarily(Document target)
    {
        SetPosition(target, newPos);
        return new SymmetryPosition_ChangeInfo() { Direction = direction };
    }

    public override IChangeInfo? Revert(Document target)
    {
        if (originalPos == newPos)
            return null;
        SetPosition(target, originalPos);
        return new SymmetryPosition_ChangeInfo() { Direction = direction };
    }

    public override bool IsMergeableWith(Change other)
    {
        return other is SymmetryPosition_UpdateableChange;
    }
}
