using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.ChangeInfos.Animation;

namespace PixiEditor.ChangeableDocument.Changes.Animation;

internal class DeleteKeyFrame_Change : Change
{
    private readonly Guid _keyFrameId;
    private KeyFrame clonedKeyFrame;
    
    [GenerateMakeChangeAction]
    public DeleteKeyFrame_Change(Guid keyFrameId)
    {
        _keyFrameId = keyFrameId;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (target.AnimationData.TryFindKeyFrame(_keyFrameId, out KeyFrame keyFrame))
        {
            clonedKeyFrame = keyFrame.Clone();
            return true;
        }

        return false;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        target.AnimationData.RemoveKeyFrame(_keyFrameId);
        ignoreInUndo = false;
        return new DeleteKeyFrame_ChangeInfo(_keyFrameId);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.AnimationData.AddKeyFrame(clonedKeyFrame.Clone());
        return new CreateRasterKeyFrame_ChangeInfo(clonedKeyFrame.LayerGuid, clonedKeyFrame.StartFrame, clonedKeyFrame.Id, false);   
    }

    public override void Dispose()
    {
        clonedKeyFrame.Dispose();
    }
}
