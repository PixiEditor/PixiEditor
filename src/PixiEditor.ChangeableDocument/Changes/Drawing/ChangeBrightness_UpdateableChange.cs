using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class ChangeBrightness_UpdateableChange : UpdateableChange
{
    private readonly Guid layerGuid;
    private readonly float correctionFactor;
    private readonly int strokeWidth;
    private readonly List<VecI> positions = new();
    private bool ignoreUpdate = false;
    private readonly bool repeat;
    private int frame;

    private List<VecI> ellipseLines;
    
    private CommittedChunkStorage? savedChunks;

    [GenerateUpdateableChangeActions]
    public ChangeBrightness_UpdateableChange(Guid layerGuid, VecI pos, float correctionFactor, int strokeWidth, bool repeat, int frame)
    {
        this.layerGuid = layerGuid;
        this.correctionFactor = correctionFactor;
        this.strokeWidth = strokeWidth;
        this.repeat = repeat;
        this.frame = frame;
        // TODO: pos is unused, check if it should be added to positions
        
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
        ImageLayerNode node = target.FindMemberOrThrow<ImageLayerNode>(layerGuid);
        var layerImage = node.GetLayerImageAtFrame(frame);
        DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, layerImage, layerGuid, false);
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        if (ignoreUpdate)
            return new None();
        VecI pos = positions[^1];
        ImageLayerNode node = target.FindMemberOrThrow<ImageLayerNode>(layerGuid);

        var layerImage = node.GetLayerImageAtFrame(frame);
        int queueLength = layerImage.QueueLength;
        
        ChangeBrightness(ellipseLines, strokeWidth, pos + new VecI(-strokeWidth / 2), correctionFactor, repeat, layerImage);
        
        var affected = layerImage.FindAffectedArea(queueLength);
        
        return new LayerImageArea_ChangeInfo(layerGuid, affected);
    }
    
    private static void ChangeBrightness(
        List<VecI> circleLines, int circleDiameter, VecI offset, float correctionFactor, bool repeat, ChunkyImage layerImage)
    {
        // TODO: Circle diameter is unused, check if it should be used

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
        var layer = target.FindMemberOrThrow<ImageLayerNode>(layerGuid);
        ignoreInUndo = false;

        if (savedChunks is not null)
            throw new InvalidOperationException("Trying to apply while there are saved chunks");
        
        var layerImage = layer.GetLayerImageAtFrame(frame);
        
        if (!firstApply)
        {
            DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, layerImage, layerGuid, false);
            foreach (VecI pos in positions)
            {
                ChangeBrightness(ellipseLines, strokeWidth, pos + new VecI(-strokeWidth / 2), correctionFactor, repeat, layerImage);
            }
        }

        var affArea = layerImage.FindAffectedArea();
        savedChunks = new CommittedChunkStorage(layerImage, affArea.Chunks);
        layerImage.CommitChanges();
        if (firstApply)
            return new None();
        return new LayerImageArea_ChangeInfo(layerGuid, affArea);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var affected = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(target, layerGuid, false, frame, ref savedChunks);
        return new LayerImageArea_ChangeInfo(layerGuid, affected);
    }
}
