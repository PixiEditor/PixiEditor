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
    private bool shiftKeyframesAfter;
    private bool inheritDuration;

    private KeyFrameData? _cloneFromCel;
    private ImageLayerNode? _cloneFromLayer;

    private Dictionary<Guid, int> originalStartFrames = new Dictionary<Guid, int>();

    [GenerateMakeChangeAction]
    public CreateCel_Change(Guid targetLayerGuid, Guid newKeyFrameGuid, int frame,
        int cloneFromFrame = -1,
        Guid cloneFromExisting = default, bool inheritDuration = false)
    {
        _targetLayerGuid = targetLayerGuid;
        _frame = frame;
        cloneFrom = cloneFromExisting != default ? cloneFromExisting : null;
        createdKeyFrameId = newKeyFrameGuid;
        this.cloneFromFrame = cloneFromFrame < 0 ? null : cloneFromFrame;
        shiftKeyframesAfter = frame > 0;
        this.inheritDuration = inheritDuration;
    }

    public override bool InitializeAndValidate(Document target)
    {
        var targetLayer = target.FindMember(_targetLayerGuid);

        if (targetLayer is null)
        {
            return false;
        }

        if (cloneFrom.HasValue)
        {
            var cloneFromMember = cloneFrom.HasValue ? target.FindMember(cloneFrom.Value) : null;

            _cloneFromLayer = cloneFromMember as ImageLayerNode;

            if (_cloneFromLayer is null)
            {
                _cloneFromCel =
                    targetLayer.KeyFrames.FirstOrDefault(x => x.KeyFrameGuid == cloneFrom.Value) as KeyFrameData;
            }

            if (_cloneFromCel is null && _cloneFromLayer is null)
            {
                return false;
            }
        }

        if (_frame == -1)
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

        if (_frame == 0 || !target.TryFindMember(_targetLayerGuid, out _layer))
        {
            return false;
        }

        var kfAtFrame = target.AnimationData.TryGetKeyFrameAtFrame(_targetLayerGuid, _frame);
        _frame = kfAtFrame?.EndFrame + 1 ?? _frame;

        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        ChunkyImage? cloneFromImage = null;

        if (_cloneFromCel is not null)
        {
            cloneFromImage = _cloneFromCel.Data as ChunkyImage;
        }
        else if (_cloneFromLayer is not null)
        {
            cloneFromImage = _cloneFromLayer.GetLayerImageAtFrame(cloneFromFrame ?? 0);
        }

        ChunkyImage img = cloneFromImage?.CloneFromCommitted() ?? new ChunkyImage(target.Size);

        ImageLayerNode targetNode = target.FindMemberOrThrow<ImageLayerNode>(_targetLayerGuid);

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

            if (_cloneFromCel is not null)
            {
                duration = _cloneFromCel.Duration;
            }
            else if (inheritDuration && cloneFromFrame.HasValue)
            {
                var frameToCloneFrom = targetNode.KeyFrames
                    .FirstOrDefault(x => x.IsInFrame(cloneFromFrame.Value));

                if (frameToCloneFrom is not null)
                {
                    duration = frameToCloneFrom.Duration;
                }
            }

            targetNode.AddFrame(
                createdKeyFrameId,
                new KeyFrameData(createdKeyFrameId, _frame, duration, ImageLayerNode.ImageLayerKey)
                {
                    Data = img,
                });
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

        var rootCelGroup = target.AnimationData.KeyFrames.FirstOrDefault(x => x.Id == targetNode.Id) as GroupKeyFrame;

        List<IChangeInfo> infos = new();
        if (rootCelGroup != null)
        {
            bool isCelAtFrame = target.AnimationData.TryGetKeyFrameAtFrame(targetNode.Id, _frame) != null;
            if ((isCelAtFrame || inheritDuration) && shiftKeyframesAfter)
            {
                var celsAfter = rootCelGroup.Children.OfType<RasterKeyFrame>().Where(x => x.EndFrame >= _frame).ToList();

                foreach (var rasterKeyFrame in celsAfter)
                {
                    if (rasterKeyFrame.Id != createdKeyFrameId)
                    {
                        if (!originalStartFrames.ContainsKey(rasterKeyFrame.Id))
                        {
                            originalStartFrames.Add(rasterKeyFrame.Id, rasterKeyFrame.StartFrame);
                        }

                        rasterKeyFrame.StartFrame += duration;
                        infos.Add(new KeyFrameLength_ChangeInfo(rasterKeyFrame.Id, rasterKeyFrame.StartFrame,
                            rasterKeyFrame.Duration));
                    }
                }
            }
        }

        target.AnimationData.AddKeyFrame(keyFrame);
        ignoreInUndo = false;

        infos.Add(new CreateRasterKeyFrame_ChangeInfo(_targetLayerGuid, _frame, createdKeyFrameId, cloneFrom.HasValue));
        infos.Add(new KeyFrameLength_ChangeInfo(createdKeyFrameId, _frame, duration));
        infos.Add(new KeyFrameVisibility_ChangeInfo(_targetLayerGuid, isVisible));

        return infos;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var rootCelGroup = target.AnimationData.KeyFrames.FirstOrDefault(x => x.Id == _layer!.Id) as GroupKeyFrame;
        target.AnimationData.RemoveKeyFrame(createdKeyFrameId);
        target.FindMemberOrThrow<ImageLayerNode>(_targetLayerGuid).RemoveKeyFrame(createdKeyFrameId);

        List<IChangeInfo> infos = new();
        var root = target.AnimationData.KeyFrames.FirstOrDefault(x => x.Id == _layer.Id) as GroupKeyFrame;

        foreach (var kvp in originalStartFrames)
        {
            if (root?.Children.FirstOrDefault(x => x.Id == kvp.Key) is RasterKeyFrame rasterKeyFrame)
            {
                rasterKeyFrame.StartFrame = kvp.Value;
                infos.Add(new KeyFrameLength_ChangeInfo(rasterKeyFrame.Id, rasterKeyFrame.StartFrame,
                    rasterKeyFrame.Duration));
            }
        }

        infos.Add(new DeleteKeyFrame_ChangeInfo(createdKeyFrameId));
        return infos;
    }
}
