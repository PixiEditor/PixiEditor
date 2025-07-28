using PixiEditor.ChangeableDocument.ChangeInfos.Animation;

namespace PixiEditor.ChangeableDocument.Changes.Animation;

internal class SetFrameRate_Change : Change
{
    private int frameRate;
    private int oldFrameRate;
    
    [GenerateMakeChangeAction]
    public SetFrameRate_Change(int frameRate)
    {
        this.frameRate = frameRate;
    }
    
    public override bool InitializeAndValidate(Document target)
    {
        oldFrameRate = target.AnimationData.FrameRate;
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        target.AnimationData.FrameRate = frameRate;
        
        ignoreInUndo = false;

        return new FrameRate_ChangeInfo(frameRate);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.AnimationData.FrameRate = oldFrameRate;

        return new FrameRate_ChangeInfo(oldFrameRate);
    }
}
