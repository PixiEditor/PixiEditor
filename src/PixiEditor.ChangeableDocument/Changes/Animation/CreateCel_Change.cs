using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Animation;

namespace PixiEditor.ChangeableDocument.Changes.Animation;

internal class CreateCel_Change : Change
{
    private readonly Guid _targetLayerGuid;
    private int _frame;
    private readonly Guid? cloneFrom;
    private int? cloneFromFrame;
    private ImageLayerNode? _layer;
    private Guid createdKeyFrameId;

    [GenerateMakeChangeAction]
    public CreateCel_Change(Guid targetLayerGuid, Guid newKeyFrameGuid, int frame,
        int cloneFromFrame = -1,
        Guid cloneFromExisting = default)
    {
        _targetLayerGuid = targetLayerGuid;
        _frame = frame;
        cloneFrom = cloneFromExisting != default ? cloneFromExisting : null;
        createdKeyFrameId = newKeyFrameGuid;
        this.cloneFromFrame = cloneFromFrame < 0 ? null : cloneFromFrame;
    }

    public override bool InitializeAndValidate(Document target)
    {
        var targetLayer = target.FindMember(_targetLayerGuid);
        
        if (targetLayer is null)
        {
            return false;
        }
        
        if(_frame == -1)
        {
            if (targetLayer.KeyFrames.All(x => x.KeyFrameGuid != createdKeyFrameId) || targetLayer.KeyFrames.Count <= 1)
            {
                return false;
            }

            if (targetLayer.KeyFrames.First()?.KeyFrameGuid == createdKeyFrameId)
            {
                return false;
            }
        }
        
        return _frame != 0 && target.TryFindMember(_targetLayerGuid, out _layer);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        var cloneFromImage = cloneFrom.HasValue
            ? target.FindMemberOrThrow<ImageLayerNode>(cloneFrom.Value).GetLayerImageAtFrame(cloneFromFrame ?? 0)
            : null;

        ImageLayerNode targetNode = target.FindMemberOrThrow<ImageLayerNode>(_targetLayerGuid);

        ChunkyImage img = cloneFromImage?.CloneFromCommitted() ?? new ChunkyImage(target.Size, target.ProcessingColorSpace);

        var keyFrame =
            new RasterKeyFrame(createdKeyFrameId, targetNode.Id, _frame, target);

        var existingData = targetNode.KeyFrames.FirstOrDefault(x => x.KeyFrameGuid == createdKeyFrameId);

        bool isVisible = true;
        int duration = 1;

        if (existingData is null)
        {
            if (_frame == -1)
            {
                ignoreInUndo = true;
                return new None();
            }
            targetNode.AddFrame(createdKeyFrameId,
                new KeyFrameData(createdKeyFrameId, _frame, 1, ImageLayerNode.ImageLayerKey) { Data = img, });
        }
        else
        {
            _frame = existingData.StartFrame;
            duration = existingData.Duration;
            isVisible = existingData.IsVisible;
            
            keyFrame.StartFrame = _frame;
            keyFrame.Duration = duration;
            keyFrame.IsVisible = isVisible;
        }

        target.AnimationData.AddKeyFrame(keyFrame);
        ignoreInUndo = false;

        List<IChangeInfo> infos = new()
        {
            new CreateRasterKeyFrame_ChangeInfo(_targetLayerGuid, _frame, createdKeyFrameId, cloneFrom.HasValue),
            new KeyFrameLength_ChangeInfo(createdKeyFrameId, _frame, duration),
            new KeyFrameVisibility_ChangeInfo(_targetLayerGuid, isVisible)
        };

        return infos;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.AnimationData.RemoveKeyFrame(createdKeyFrameId);
        target.FindMemberOrThrow<ImageLayerNode>(_targetLayerGuid).RemoveKeyFrame(createdKeyFrameId);
        return new DeleteKeyFrame_ChangeInfo(createdKeyFrameId);
    }
}
