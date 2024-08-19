using PixiEditor.ChangeableDocument.ChangeInfos.Animation;

namespace PixiEditor.ChangeableDocument.Changes.Animation;

internal class SetOnionSettings_Change : Change
{
    public int OnionFrames { get; set; }
    public double Opacity { get; set; }
    
    private int oldOnionFrames;
    private double oldOpacity;
    
    [GenerateMakeChangeAction]
    public SetOnionSettings_Change(int onionFrames, double opacity)
    {
        OnionFrames = onionFrames;
        Opacity = opacity;
    }
    
    public override bool InitializeAndValidate(Document target)
    {
        oldOnionFrames = target.AnimationData.OnionFrames;
        oldOpacity = target.AnimationData.OnionOpacity;
        return true;    
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        target.AnimationData.OnionFrames = OnionFrames;
        target.AnimationData.OnionOpacity = Opacity;
        
        ignoreInUndo = true;
        return new OnionFrames_ChangeInfo(OnionFrames, Opacity);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.AnimationData.OnionFrames = oldOnionFrames;
        target.AnimationData.OnionOpacity = oldOpacity;

        return new OnionFrames_ChangeInfo(oldOnionFrames, oldOpacity);
    }
}
