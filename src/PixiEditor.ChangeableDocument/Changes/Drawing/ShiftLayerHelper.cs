using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal static class ShiftLayerHelper
{
    public static AffectedArea DrawShiftedLayer(Document target, Guid layerGuid, bool keepOriginal, VecI delta, int frame)
    {
        var targetImage = target.FindMemberOrThrow<ImageLayerNode>(layerGuid).GetLayerImageAtFrame(frame);
        var prevArea = targetImage.FindAffectedArea();
        targetImage.CancelChanges();
        if (!keepOriginal)
            targetImage.EnqueueClear();
        targetImage.EnqueueDrawChunkyImage(delta, targetImage, false, false);
        var curArea = targetImage.FindAffectedArea();

        curArea.UnionWith(prevArea);
        return curArea;
    }
}
