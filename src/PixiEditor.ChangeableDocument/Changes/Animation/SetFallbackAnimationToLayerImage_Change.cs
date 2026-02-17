using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Animation;

namespace PixiEditor.ChangeableDocument.Changes.Animation;

internal class SetFallbackAnimationToLayerImage_Change : Change
{
    public bool FallbackToLayerImage { get; }

    [GenerateMakeChangeAction]
    public SetFallbackAnimationToLayerImage_Change(bool fallbackToLayerImage)
    {
        FallbackToLayerImage = fallbackToLayerImage;
    }

    public override bool InitializeAndValidate(Document target)
    {
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        target.AnimationData.FallbackAnimationToLayerImage = FallbackToLayerImage;
        ignoreInUndo = false;

        target.NodeGraph.TryTraverse(x =>
        {
            if (x is ImageLayerNode imgLayerNode)
            {
                imgLayerNode.FallbackAnimationToLayerImage = FallbackToLayerImage;
            }
        });

        return new FallbackAnimationToLayerImage_ChangeInfo(FallbackToLayerImage);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.AnimationData.FallbackAnimationToLayerImage = !FallbackToLayerImage;

        target.NodeGraph.TryTraverse(x =>
        {
            if (x is ImageLayerNode imgLayerNode)
            {
                imgLayerNode.FallbackAnimationToLayerImage = !FallbackToLayerImage;
            }
        });

        return new FallbackAnimationToLayerImage_ChangeInfo(!FallbackToLayerImage);
    }
}
