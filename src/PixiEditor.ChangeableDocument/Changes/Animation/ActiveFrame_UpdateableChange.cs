using PixiEditor.ChangeableDocument.ChangeInfos.Animation;

namespace PixiEditor.ChangeableDocument.Changes.Animation;

internal class ActiveFrame_UpdateableChange : UpdateableChange
{
    private int newFrame;
    private int originalFrame;
    
    [GenerateUpdateableChangeActions]
    public ActiveFrame_UpdateableChange(int activeFrame)
    {
        newFrame = activeFrame;
    }
    
    [UpdateChangeMethod]
    public void Update(int activeFrame)
    {
        newFrame = activeFrame;
    }
    
    public override bool InitializeAndValidate(Document target)
    {
        originalFrame = target.AnimationData.ActiveFrame;
        return true;
    }
    
    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        if (target.AnimationData.ActiveFrame == newFrame)
        {
            return new None();
        }
        target.AnimationData.ActiveFrame = newFrame;
        return new ActiveFrame_ChangeInfo(newFrame);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        ignoreInUndo = false;
        if (target.AnimationData.ActiveFrame == newFrame)
        {
            return new None();
        }
        
        target.AnimationData.ActiveFrame = newFrame;
        return new ActiveFrame_ChangeInfo(newFrame);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.AnimationData.ActiveFrame = originalFrame;
        return new ActiveFrame_ChangeInfo(originalFrame);
    }

    public override bool IsMergeableWith(Change other)
    {
        return other is ActiveFrame_UpdateableChange;
    }
}
