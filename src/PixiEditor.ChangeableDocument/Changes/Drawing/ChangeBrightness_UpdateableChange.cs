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
    
    private Surface tempSurface;
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
        tempSurface = new Surface(new VecI(strokeWidth, strokeWidth));
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
        Layer layer = (Layer)target.FindMemberOrThrow(layerGuid);
        DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, layer.LayerImage, layerGuid, false);
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        if (ignoreUpdate)
            return new None();
        VecI pos = positions[^1];
        Layer layer = (Layer)target.FindMemberOrThrow(layerGuid);

        int queueLength = layer.LayerImage.QueueLength;
        
        ChangeBrightness(ellipseLines, strokeWidth, pos + new VecI(-strokeWidth / 2), correctionFactor, repeat, tempSurface, layer.LayerImage);
        
        var affected = layer.LayerImage.FindAffectedChunks(queueLength);
        
        return new LayerImageChunks_ChangeInfo(layerGuid, affected);
    }
    
    private static void ChangeBrightness(
        List<VecI> circleLines, int circleDiameter, VecI offset, float correctionFactor, bool repeat, Surface tempSurface, ChunkyImage layerImage)
    {
        tempSurface.DrawingSurface.Canvas.Clear();
        if (repeat)
        {
            layerImage.DrawMostUpToDateRegionOn
                (new RectI(offset, new(circleDiameter, circleDiameter)), ChunkResolution.Full, tempSurface.DrawingSurface, new VecI(0));
        }
        else
        {
            layerImage.DrawCommittedRegionOn
                (new RectI(offset, new(circleDiameter, circleDiameter)), ChunkResolution.Full, tempSurface.DrawingSurface, new VecI(0));
        }
        
        for (var i = 0; i < circleLines.Count - 1; i++)
        {
            VecI left = circleLines[i];
            VecI right = circleLines[i + 1];
            int y = left.Y;
            
            for (VecI pos = new VecI(left.X, y); pos.X <= right.X; pos.X++)
            {
                Color pixel = tempSurface.GetSRGBPixel(pos);
                Color newColor = ColorHelper.ChangeColorBrightness(pixel, correctionFactor);
                layerImage.EnqueueDrawPixel(pos + offset, newColor, BlendMode.Src);
            }
        }
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var layer = (Layer)target.FindMemberOrThrow(layerGuid);
        ignoreInUndo = false;

        if (savedChunks is not null)
            throw new InvalidOperationException("Trying to apply while there are saved chunks");
        
        if (!firstApply)
        {
            DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, layer.LayerImage, layerGuid, false);
            foreach (VecI pos in positions)
            {
                ChangeBrightness(ellipseLines, strokeWidth, pos + new VecI(-strokeWidth / 2), correctionFactor, repeat, tempSurface, layer.LayerImage);
            }
        }

        var affChunks = layer.LayerImage.FindAffectedChunks();
        savedChunks = new CommittedChunkStorage(layer.LayerImage, affChunks);
        layer.LayerImage.CommitChanges();
        if (firstApply)
            return new None();
        return new LayerImageChunks_ChangeInfo(layerGuid, affChunks);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var affected = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(target, layerGuid, false, ref savedChunks);
        return new LayerImageChunks_ChangeInfo(layerGuid, affected);
    }

    public override void Dispose()
    {
        tempSurface.Dispose();
    }
}
