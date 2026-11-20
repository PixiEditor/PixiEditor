using PixiEditor.ChangeableDocument.ChangeInfos.Guides;
using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.Changes.Root.Guides;

internal class GuidePosition_UpdateableChange : UpdateableChange
{
    private readonly Guid guideId;
    private double newPos;
    private double originalPos;

    private Guide guide;

    [GenerateUpdateableChangeActions]
    public GuidePosition_UpdateableChange(Guid id, double pos)
    {
        guideId = id;
        newPos = pos;
    }

    [UpdateChangeMethod]
    public void Update(double pos)
    {
        newPos = pos;
    }

    public override bool InitializeAndValidate(Document target)
    {
        guide = target.Guides.FirstOrDefault(g => g.Id == guideId);
        if (guide != null)
        {
            originalPos = guide.Offset;
        }

        return guide != null;
    }

    private void SetPosition(double position)
    {
        guide.Offset = position;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        ignoreInUndo = originalPos == newPos;
        SetPosition(newPos);
        return new Guide_ChangeInfo(guideId, guide.IsVertical, newPos);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        SetPosition(newPos);
        return new Guide_ChangeInfo(guideId, guide.IsVertical, newPos);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        if (originalPos == newPos)
            return new None();

        SetPosition(originalPos);
        return new Guide_ChangeInfo(guideId, guide.IsVertical, originalPos);
    }

    public override bool IsMergeableWith(Change other)
    {
        return other is GuidePosition_UpdateableChange change && change.guideId == guideId;
    }
}
