using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal static class ShiftLayerHelper
{
    public static AffectedArea DrawShiftedLayer(Document target, Guid layerGuid, bool keepOriginal, VecI delta,
        int frame, VectorPath? clipPath = null)
    {
        var targetImage = target.FindMemberOrThrow<ImageLayerNode>(layerGuid).GetLayerImageAtFrame(frame);
        var prevArea = targetImage.FindAffectedArea();
        targetImage.CancelChanges();
        if (!keepOriginal)
            targetImage.EnqueueClear();

        if (clipPath != null)
        {
            targetImage.SetClippingPath(clipPath);
        }

        targetImage.EnqueueDrawCommitedChunkyImage(delta, targetImage, false, false);
        var curArea = targetImage.FindAffectedArea();

        curArea.UnionWith(prevArea);
        return curArea;
    }
}
