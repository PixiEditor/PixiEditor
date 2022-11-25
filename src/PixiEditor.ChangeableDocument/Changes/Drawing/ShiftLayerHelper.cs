using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal static class ShiftLayerHelper
{
    public static HashSet<VecI> DrawShiftedLayer(Document target, Guid layerGuid, bool keepOriginal, VecI delta)
    {
        var targetImage = target.FindMemberOrThrow<Layer>(layerGuid).LayerImage;
        var prevChunks = targetImage.FindAffectedChunks();
        targetImage.CancelChanges();
        if (!keepOriginal)
            targetImage.EnqueueClear();
        targetImage.EnqueueDrawChunkyImage(delta, targetImage, false, false);
        var curChunks = targetImage.FindAffectedChunks();
        curChunks.UnionWith(prevChunks);
        return curChunks;
    }
}
