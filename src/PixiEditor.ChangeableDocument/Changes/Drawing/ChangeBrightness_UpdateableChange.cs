using ChunkyImageLib.Operations;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class ChangeBrightness_UpdateableChange : UpdateableChange
{
    private readonly Guid layerGuid;
    private readonly float correctionFactor;
    private readonly int strokeWidth;
    private readonly List<VecI> positions = new();
    private bool ignoreUpdate = false;
    private readonly bool repeat;

    private List<VecI> ellipseLines;
    
    private CommittedChunkStorage? savedChunks;

    [GenerateUpdateableChangeActions]
    public ChangeBrightness_UpdateableChange(Guid layerGuid, VecI pos, float correctionFactor, int strokeWidth, bool repeat)
    {
        this.layerGuid = layerGuid;
        this.correctionFactor = correctionFactor;
        this.strokeWidth = strokeWidth;
        this.repeat = repeat;

        ellipseLines = EllipseHelper.SplitEllipseIntoLines((EllipseHelper.GenerateEllipseFromRect(new RectI(0, 0, strokeWidth, strokeWidth))));
    }

    [UpdateChangeMethod]
    public void Update(VecI pos)
    {
        ignoreUpdate = positions.Count > 0 && positions[^1] == pos;
        if (!ignoreUpdate)
            positions.Add(pos);
    }
    
    public override bool InitializeAndValidate(Document target)
    {
        if (!DrawingChangeHelper.IsValidForDrawing(target, layerGuid, false))
            return false;
        RasterLayer layer = target.FindMemberOrThrow<RasterLayer>(layerGuid);
        DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, layer.LayerImage, layerGuid, false);
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        if (ignoreUpdate)
            return new None();
        VecI pos = positions[^1];
        RasterLayer layer = target.FindMemberOrThrow<RasterLayer>(layerGuid);

        int queueLength = layer.LayerImage.QueueLength;
        
        ChangeBrightness(ellipseLines, strokeWidth, pos + new VecI(-strokeWidth / 2), correctionFactor, repeat, layer.LayerImage);
        
        var affected = layer.LayerImage.FindAffectedArea(queueLength);
        
        return new LayerImageArea_ChangeInfo(layerGuid, affected);
    }
    
    private static void ChangeBrightness(
        List<VecI> circleLines, int circleDiameter, VecI offset, float correctionFactor, bool repeat, ChunkyImage layerImage)
    {

        for (var i = 0; i < circleLines.Count - 1; i++)
        {
            VecI left = circleLines[i];
            VecI right = circleLines[i + 1];
            int y = left.Y;
            
            for (VecI pos = new VecI(left.X, y); pos.X <= right.X; pos.X++)
            {
                layerImage.EnqueueDrawPixel(
                    pos + offset,
                    (commitedColor, upToDateColor) =>
                    {
                        Color newColor = ColorHelper.ChangeColorBrightness(repeat ? upToDateColor : commitedColor, correctionFactor);
                        return ColorHelper.ChangeColorBrightness(newColor, correctionFactor);
                    },
                    BlendMode.Src);
            }
        }
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var layer = target.FindMemberOrThrow<RasterLayer>(layerGuid);
        ignoreInUndo = false;

        if (savedChunks is not null)
            throw new InvalidOperationException("Trying to apply while there are saved chunks");
        
        if (!firstApply)
        {
            DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, layer.LayerImage, layerGuid, false);
            foreach (VecI pos in positions)
            {
                ChangeBrightness(ellipseLines, strokeWidth, pos + new VecI(-strokeWidth / 2), correctionFactor, repeat, layer.LayerImage);
            }
        }

        var affArea = layer.LayerImage.FindAffectedArea();
        savedChunks = new CommittedChunkStorage(layer.LayerImage, affArea.Chunks);
        layer.LayerImage.CommitChanges();
        if (firstApply)
            return new None();
        return new LayerImageArea_ChangeInfo(layerGuid, affArea);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var affected = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(target, layerGuid, false, ref savedChunks);
        return new LayerImageArea_ChangeInfo(layerGuid, affected);
    }
}
