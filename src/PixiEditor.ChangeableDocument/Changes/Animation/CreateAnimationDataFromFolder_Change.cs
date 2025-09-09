using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Animation;

namespace PixiEditor.ChangeableDocument.Changes.Animation;

internal class CreateAnimationDataFromFolder_Change : Change
{
    private readonly Guid folderGuid;
    private Guid[] layerGuids;

    [GenerateMakeChangeAction]
    public CreateAnimationDataFromFolder_Change(Guid folderGuid)
    {
        this.folderGuid = folderGuid;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (!target.TryFindMember<FolderNode>(folderGuid, out FolderNode? layer))
        {
            return false;
        }

        var layers = target.ExtractLayers([layer.Id]);
        if (layers.Count == 0) return false;

        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        FolderNode folder = target.FindNode(folderGuid) as FolderNode;
        List<IChangeInfo> infos = new List<IChangeInfo>();
        layerGuids = target.ExtractLayers([folder.Id]).ToArray();

        foreach (var layer in layerGuids)
        {
            var node = target.FindNode(layer);
            if(node is not LayerNode) continue;
            foreach (var frame in node.KeyFrames)
            {
                if(frame.StartFrame == 0 && frame.Duration == 0) continue;
                Guid keyFrameId = frame.KeyFrameGuid;
                target.AnimationData.AddKeyFrame(new RasterKeyFrame(keyFrameId, node.Id, frame.StartFrame, target)
                {
                    Duration = frame.Duration,
                    IsVisible = frame.IsVisible,
                });
                infos.Add(new CreateRasterKeyFrame_ChangeInfo(node.Id, frame.StartFrame, keyFrameId, true));
                infos.Add(new KeyFrameLength_ChangeInfo(keyFrameId, frame.StartFrame, frame.Duration));
                infos.Add(new KeyFrameVisibility_ChangeInfo(keyFrameId, frame.IsVisible));
            }
        }

        ignoreInUndo = false;
        return infos;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var layer = target.FindNode(folderGuid) as FolderNode;
        List<IChangeInfo> infos = new List<IChangeInfo>();

        var keyFrame = target.AnimationData.KeyFrames;
        var ids = keyFrame.Where(x => x.NodeId == layer.Id || layerGuids.Contains(x.NodeId)).Select(x => x.Id).ToList();

        foreach (var id in ids)
        {
            target.AnimationData.RemoveKeyFrame(id);
            infos.Add(new DeleteKeyFrame_ChangeInfo(id));
        }

        return infos;
    }
}
