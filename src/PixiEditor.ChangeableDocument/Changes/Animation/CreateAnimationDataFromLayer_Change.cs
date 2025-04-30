using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Animation;

namespace PixiEditor.ChangeableDocument.Changes.Animation;

internal class CreateAnimationDataFromLayer_Change : Change
{
    private readonly Guid layerGuid;

    [GenerateMakeChangeAction]
    public CreateAnimationDataFromLayer_Change(Guid layerGuid)
    {
        this.layerGuid = layerGuid;
    }

    public override bool InitializeAndValidate(Document target)
    {
        return target.TryFindMember<LayerNode>(layerGuid, out LayerNode? layer) && layer.KeyFrames != null &&
               layer.KeyFrames.Count != 0;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        LayerNode layer = target.FindNode(layerGuid) as LayerNode;
        List<IChangeInfo> infos = new List<IChangeInfo>();
        foreach (var frame in layer.KeyFrames)
        {
            Guid keyFrameId = frame.KeyFrameGuid;
            target.AnimationData.AddKeyFrame(new RasterKeyFrame(keyFrameId, layer.Id, frame.StartFrame, target)
            {
                Duration = frame.Duration
            });
            infos.Add(new CreateRasterKeyFrame_ChangeInfo(layer.Id, frame.StartFrame, keyFrameId, true));
        }

        ignoreInUndo = false;
        return infos;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var layer = target.FindNode(layerGuid) as LayerNode;
        List<IChangeInfo> infos = new List<IChangeInfo>();

        var keyFrame = target.AnimationData.KeyFrames;
        var ids = keyFrame.Where(x => x.NodeId == layer.Id).Select(x => x.Id).ToList();

        foreach (var id in ids)
        {
            target.AnimationData.RemoveKeyFrame(id);
            infos.Add(new DeleteKeyFrame_ChangeInfo(id));
        }

        return infos;
    }
}
