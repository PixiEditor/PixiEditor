using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.ChangeInfos.Animation;

namespace PixiEditor.ChangeableDocument.Changes.Animation;

internal class CreateRasterClip_Change : Change
{
    private readonly Guid _targetLayerGuid;
    private readonly int _frame;
    private readonly bool _cloneFromExisting;
    private RasterLayer? _layer;
    private Guid createdKeyFrameId;

    [GenerateMakeChangeAction]
    public CreateRasterClip_Change(Guid targetLayerGuid, int frame, bool cloneFromExisting = false)
    {
        _targetLayerGuid = targetLayerGuid;
        _frame = frame;
        _cloneFromExisting = cloneFromExisting;
    }
    
    public override bool InitializeAndValidate(Document target)
    {
        return target.TryFindMember(_targetLayerGuid, out _layer);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var keyFrame =
            new RasterKeyFrame(_targetLayerGuid, _frame, target, _cloneFromExisting ? _layer.LayerImage : null);
        createdKeyFrameId = keyFrame.Id;
        target.AnimationData.AddKeyFrame(keyFrame);
        target.AnimationData.ChangePreviewFrame(_frame);
        ignoreInUndo = false;
        return new CreateRasterKeyFrame_ChangeInfo(_targetLayerGuid, _frame, createdKeyFrameId, _cloneFromExisting);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.AnimationData.RemoveKeyFrame(createdKeyFrameId);
        return new DeleteKeyFrame_ChangeInfo(_frame, createdKeyFrameId);
    }
}
