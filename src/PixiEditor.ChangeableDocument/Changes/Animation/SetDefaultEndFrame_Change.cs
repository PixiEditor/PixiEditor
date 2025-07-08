using PixiEditor.ChangeableDocument.ChangeInfos.Animation;

namespace PixiEditor.ChangeableDocument.Changes.Animation;

internal class SetDefaultEndFrame_Change : Change
{
    public int NewDefaultEndFrame { get; }

    private int originalDefaultEndFrame;

    [GenerateMakeChangeAction]
    public SetDefaultEndFrame_Change(int newDefaultEndFrame)
    {
        NewDefaultEndFrame = newDefaultEndFrame;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (target.AnimationData is null)
        {
            return false;
        }

        if (NewDefaultEndFrame < 0)
        {
            return false;
        }

        originalDefaultEndFrame = target.AnimationData.DefaultEndFrame;

        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        ignoreInUndo = false;

        if (target.AnimationData is null)
        {
            return new None();
        }

        target.AnimationData.DefaultEndFrame = NewDefaultEndFrame;

        return new DefaultEndFrame_ChangeInfo(NewDefaultEndFrame);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.AnimationData.DefaultEndFrame = originalDefaultEndFrame;
        return new DefaultEndFrame_ChangeInfo(originalDefaultEndFrame);
    }
}
