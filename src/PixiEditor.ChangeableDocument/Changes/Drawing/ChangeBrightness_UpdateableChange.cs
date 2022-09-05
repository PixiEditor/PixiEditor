using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class ChangeBrightness_UpdateableChange : UpdateableChange
{
    private readonly Guid layerGuid;
    private readonly int strokeWidth;
    private readonly List<VecI> positions = new();
    private bool ignoreUpdate = false;
    private readonly bool repeat;
    private readonly bool darken;
    private readonly Paint paint;
    private readonly Color color;
    private CommittedChunkStorage? savedChunks;

    [GenerateUpdateableChangeActions]
    public ChangeBrightness_UpdateableChange(Guid layerGuid, VecI pos, float correctionFactor, int strokeWidth, bool repeat, bool darken)
    {
        this.layerGuid = layerGuid;
        this.strokeWidth = strokeWidth;
        this.positions.Add(pos);
        this.repeat = repeat;
        this.darken = darken;

        color = (darken ? Colors.Black : Colors.White)
            .WithAlpha((byte)Math.Clamp(correctionFactor * 255 / 100, 0, 255)); 
        paint = new Paint { BlendMode = repeat ? BlendMode.SrcOver : BlendMode.Src };
    }

    [UpdateChangeMethod]
    public void Update(VecI pos)
    {
        ignoreUpdate = positions[^1] == pos;
        if (!ignoreUpdate)
            positions.Add(pos);
    }
    
    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        if (!DrawingChangeHelper.IsValidForDrawing(target, layerGuid, false))
            return new Error();
        Layer layer = (Layer)target.FindMemberOrThrow(layerGuid);
        DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, layer.LayerImage, layerGuid, false);
        layer.LayerImage.SetBlendMode(darken ? BlendMode.Multiply : BlendMode.Screen);
        return new Success();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        if (ignoreUpdate)
            return new None();
        VecI pos = positions[^1];
        Layer layer = (Layer)target.FindMemberOrThrow(layerGuid);

        int queueLength = layer.LayerImage.QueueLength;
        layer.LayerImage.EnqueueDrawEllipse(
            new RectI(pos + new VecI(-strokeWidth/2), new(strokeWidth)),
            Colors.Transparent, color, 0, paint);
        var affected = layer.LayerImage.FindAffectedChunks(queueLength);
        
        return new LayerImageChunks_ChangeInfo(layerGuid, affected);
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
            layer.LayerImage.SetBlendMode(darken ? BlendMode.Multiply : BlendMode.Screen);
            foreach (VecI pos in positions)
            {
                layer.LayerImage.EnqueueDrawEllipse(
                    new RectI(pos + new VecI(-strokeWidth/2), new(strokeWidth)),
                    Colors.Transparent, color, 0, paint);
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
        paint.Dispose();
    }
}
