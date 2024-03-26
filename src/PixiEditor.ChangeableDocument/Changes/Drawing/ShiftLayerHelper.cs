using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal static class ShiftLayerHelper
{
    public static AffectedArea DrawShiftedLayer(Document target, Guid layerGuid, bool keepOriginal, VecI delta)
    {
        var targetImage = target.FindMemberOrThrow<RasterLayer>(layerGuid).LayerImage;
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
