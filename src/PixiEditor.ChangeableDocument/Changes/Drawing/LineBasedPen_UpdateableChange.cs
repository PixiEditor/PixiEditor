using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;
internal class LineBasedPen_UpdateableChange : UpdateableChange
{
    private readonly Guid memberGuid;
    private readonly SKColor color;
    private readonly int strokeWidth;
    private readonly bool replacing;
    private readonly bool drawOnMask;

    bool firstApply = true;

    private CommittedChunkStorage? storedChunks;
    private readonly List<VecI> points = new();

    [GenerateUpdateableChangeActions]
    public LineBasedPen_UpdateableChange(Guid memberGuid, SKColor color, VecI pos, int strokeWidth, bool replacing, bool drawOnMask)
    {
        this.memberGuid = memberGuid;
        this.color = color;
        this.strokeWidth = strokeWidth;
        this.replacing = replacing;
        this.drawOnMask = drawOnMask;
        points.Add(pos);
    }

    [UpdateChangeMethod]
    public void Update(VecI pos)
    {
        points.Add(pos);
    }

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        if (!DrawingChangeHelper.IsValidForDrawing(target, memberGuid, drawOnMask))
            return new Error();
        if (strokeWidth < 1)
            return new Error();
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask);
        if (!replacing)
            image.SetBlendMode(SKBlendMode.SrcOver);
        DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, image, memberGuid, drawOnMask);
        return new Success();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask);

        var (from, to) = points.Count > 1 ? (points[^2], points[^1]) : (points[0], points[0]);

        int opCount = image.QueueLength;

        if (strokeWidth == 1)
            image.EnqueueDrawBresenhamLine(from, to, color, SKBlendMode.Src);
        else
            image.EnqueueDrawSkiaLine(from, to, SKStrokeCap.Round, strokeWidth, color, SKBlendMode.Src);
        var affChunks = image.FindAffectedChunks(opCount);

        return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, affChunks, drawOnMask);
    }

    private void FastforwardEnqueueDrawLines(ChunkyImage targetImage)
    {
        if (points.Count == 1)
        {
            if (strokeWidth == 1)
                targetImage.EnqueueDrawBresenhamLine(points[0], points[0], color, SKBlendMode.Src);
            else
                targetImage.EnqueueDrawSkiaLine(points[0], points[0], SKStrokeCap.Round, strokeWidth, color, SKBlendMode.Src);
            return;
        }
        for (int i = 1; i < points.Count; i++)
        {
            if (strokeWidth == 1)
                targetImage.EnqueueDrawBresenhamLine(points[i - 1], points[i], color, SKBlendMode.Src);
            else
                targetImage.EnqueueDrawSkiaLine(points[i - 1], points[i], SKStrokeCap.Round, strokeWidth, color, SKBlendMode.Src);
        }
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, out bool ignoreInUndo)
    {
        if (storedChunks is not null)
            throw new InvalidOperationException("Trying to save chunks while there are saved chunks already");
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask);

        ignoreInUndo = false;
        if (firstApply)
        {
            firstApply = false;

            var affChunks = image.FindAffectedChunks();
            storedChunks = new CommittedChunkStorage(image, affChunks);
            image.CommitChanges();

            return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, affChunks, drawOnMask);
        }
        else
        {
            if (!replacing)
                image.SetBlendMode(SKBlendMode.SrcOver);
            DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, image, memberGuid, drawOnMask);

            FastforwardEnqueueDrawLines(image);
            var affChunks = image.FindAffectedChunks();
            storedChunks = new CommittedChunkStorage(image, affChunks);
            image.CommitChanges();

            return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, affChunks, drawOnMask);
        }
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var affected = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(target, memberGuid, drawOnMask, ref storedChunks);
        return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, affected, drawOnMask);
    }

    public override void Dispose()
    {
        storedChunks?.Dispose();
    }
}
