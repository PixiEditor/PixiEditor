using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.ChangeInfos.Animation;

namespace PixiEditor.ChangeableDocument.Changes.Animation;

internal class KeyFrameVisibility_Change : Change
{
    private readonly Guid _keyFrameId;
    private bool _isVisible;
    private bool _originalVisibility;
    
    [GenerateMakeChangeAction]
    public KeyFrameVisibility_Change(Guid keyFrameId, bool isVisible)
    {
        _keyFrameId = keyFrameId;
        _isVisible = isVisible;
    }
    
    public override bool InitializeAndValidate(Document target)
    {
        if (target.AnimationData.TryFindKeyFrame<KeyFrame>(_keyFrameId, out KeyFrame keyFrame))
        {
            _originalVisibility = keyFrame.IsVisible;
            return true;
        }
        
        return false;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        if (target.AnimationData.TryFindKeyFrame<KeyFrame>(_keyFrameId, out var keyFrame))
        {
            keyFrame.IsVisible = _isVisible;
            ignoreInUndo = false;
            return new KeyFrameVisibility_ChangeInfo(_keyFrameId, _isVisible);
        }
        
        ignoreInUndo = true;
        return new None();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        if (target.AnimationData.TryFindKeyFrame<KeyFrame>(_keyFrameId, out var keyFrame))
        {
            keyFrame.IsVisible = _originalVisibility;
            return new KeyFrameVisibility_ChangeInfo(_keyFrameId, !_isVisible);
        }
        
        return new None();
    }
}
