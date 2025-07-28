using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Properties;

namespace PixiEditor.ChangeableDocument.Changes.Properties;

internal class ChangeProcessingColorSpace_Change : Change
{
    private ColorSpace toColorSpace;
    private ColorSpace original;

    [GenerateMakeChangeAction]
    public ChangeProcessingColorSpace_Change(ColorSpace newColorSpace)
    {
        this.toColorSpace = newColorSpace;
    }

    public override bool InitializeAndValidate(Document target)
    {
        original = target.ProcessingColorSpace;
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        ignoreInUndo = false;
        target.ProcessingColorSpace = toColorSpace;

        ConvertImageNodes(target, toColorSpace);

        return new ProcessingColorSpace_ChangeInfo(toColorSpace);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.ProcessingColorSpace = original;

        ConvertImageNodes(target, original);

        return new ProcessingColorSpace_ChangeInfo(original);
    }

    private void ConvertImageNodes(Document target, ColorSpace newColorSpace)
    {
        foreach (var node in target.NodeGraph.Nodes)
        {
            if (node is ImageLayerNode imageLayerNode)
            {
                foreach (var keyFrame in imageLayerNode.KeyFrames)
                {
                    if (keyFrame.Data is ChunkyImage chunkyImage)
                    {
                        ChunkyImage img = new ChunkyImage(chunkyImage.LatestSize, newColorSpace);
                        img.EnqueueDrawCommitedChunkyImage(VecI.Zero, chunkyImage);
                        img.CommitChanges();

                        keyFrame.Data = img;
                    }
                }
            }
        }
    }
}
