using PixiEditor.ChangeableDocument.ChangeInfos.Guides;

namespace PixiEditor.ChangeableDocument.Changes.Root.Guides;

internal class AddGuide_Change : Change
{
    private Guid guideId;
    private bool isVertical;
    private double offset;

    [GenerateMakeChangeAction]
    public AddGuide_Change(bool vertical, double offset)
    {
        guideId = Guid.NewGuid();
        this.isVertical = vertical;
        this.offset = offset;
    }

    public override bool InitializeAndValidate(Document target)
    {
        return target.Guides.All(x => Math.Abs(x.Offset - offset) > 0.01 || x.IsVertical != isVertical);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        target.Guides.Add(new Guide(guideId, isVertical, offset));

        ignoreInUndo = false;
        return new Guide_ChangeInfo(guideId, isVertical, offset);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var guide = target.Guides.FirstOrDefault(x => x.Id == guideId);
        target.Guides.Remove(guide);

        return new RemoveGuide_ChangeInfo(guideId);
    }
}
