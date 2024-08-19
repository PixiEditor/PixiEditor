using PixiEditor.ChangeableDocument.ChangeInfos.Animation;

namespace PixiEditor.ChangeableDocument.Changes.Animation;

internal class SetOnionFrames_Change : Change
{
    public int OnionFrames { get; set; }
    
    private int oldOnionFrames;
    
    [GenerateMakeChangeAction]
    public SetOnionFrames_Change(int onionFrames)
    {
        OnionFrames = onionFrames;
    }
    
    public override bool InitializeAndValidate(Document target)
    {
        oldOnionFrames = target.AnimationData.OnionFrames;
        return true;    
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        target.AnimationData.OnionFrames = OnionFrames;
        
        ignoreInUndo = true;
        return new OnionFrames_ChangeInfo(OnionFrames);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.AnimationData.OnionFrames = oldOnionFrames;

        return new OnionFrames_ChangeInfo(oldOnionFrames);
    }
}
