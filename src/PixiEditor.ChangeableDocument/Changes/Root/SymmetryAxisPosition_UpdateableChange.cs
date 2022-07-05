using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.Changes.Root;
internal class SymmetryAxisPosition_UpdateableChange : UpdateableChange
{
    private readonly SymmetryAxisDirection direction;
    private int newPos;
    private int originalPos;

    [GenerateUpdateableChangeActions]
    public SymmetryAxisPosition_UpdateableChange(SymmetryAxisDirection direction, int pos)
    {
        this.direction = direction;
        newPos = pos;
    }

    [UpdateChangeMethod]
    public void Update(int pos)
    {
        newPos = pos;
    }

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        originalPos = direction switch
        {
            SymmetryAxisDirection.Horizontal => target.HorizontalSymmetryAxisY,
            SymmetryAxisDirection.Vertical => target.VerticalSymmetryAxisX,
            _ => throw new NotImplementedException(),
        };
        return new Success();
    }

    private void SetPosition(Document target, int position)
    {
        if (direction == SymmetryAxisDirection.Horizontal)
            target.HorizontalSymmetryAxisY = position;
        else if (direction == SymmetryAxisDirection.Vertical)
            target.VerticalSymmetryAxisX = position;
        else
            throw new NotImplementedException();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        ignoreInUndo = originalPos == newPos;
        SetPosition(target, newPos);
        return new SymmetryAxisPosition_ChangeInfo(direction, newPos);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        SetPosition(target, newPos);
        return new SymmetryAxisPosition_ChangeInfo(direction, newPos);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        if (originalPos == newPos)
            return new None();
        SetPosition(target, originalPos);
        return new SymmetryAxisPosition_ChangeInfo(direction, originalPos);
    }

    public override bool IsMergeableWith(Change other)
    {
        return other is SymmetryAxisPosition_UpdateableChange change && change.direction == direction;
    }
}
