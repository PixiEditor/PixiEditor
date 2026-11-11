using PixiEditor.ChangeableDocument.ChangeInfos.Guides;

namespace PixiEditor.ChangeableDocument.Changes.Root.Guides;

internal class RemoveGuide_Change : Change
{
    private Guid guideId;
    private Guide originalGuide;

    [GenerateMakeChangeAction]
    public RemoveGuide_Change(Guid id)
    {
        guideId = id;
    }

    public override bool InitializeAndValidate(Document target)
    {
        var guide = target.Guides.FirstOrDefault(x => x.Id == guideId);
        if (guide == null)
        {
            return false;
        }

        originalGuide = guide;
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var guide = target.Guides.FirstOrDefault(x => x.Id == guideId);
        target.Guides.Remove(guide);

        ignoreInUndo = false;
        return new RemoveGuide_ChangeInfo(guideId);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.Guides.Add(originalGuide);
        return new Guide_ChangeInfo(originalGuide.Id, originalGuide.IsVertical, originalGuide.Offset);
    }
}
