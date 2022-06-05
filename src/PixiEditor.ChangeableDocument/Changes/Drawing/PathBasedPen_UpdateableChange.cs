using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;
internal class PathBasedPen_UpdateableChange : UpdateableChange
{
    private readonly Guid memberGuid;
    private readonly SKColor color;
    private readonly float strokeWidth;
    private readonly bool drawOnMask;

    bool firstApply = true;

    private CommittedChunkStorage? storedChunks;
    private SKPath tempPath = new();

    private List<VecD> points = new();

    [GenerateUpdateableChangeActions]
    public PathBasedPen_UpdateableChange(Guid memberGuid, VecD pos, SKColor color, float strokeWidth, bool drawOnMask)
    {
        this.memberGuid = memberGuid;
        this.color = color;
        this.strokeWidth = strokeWidth;
        this.drawOnMask = drawOnMask;
        points.Add(pos);
    }

    [UpdateChangeMethod]
    public void Update(VecD pos)
    {
        points.Add(pos);
    }

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        if (!DrawingChangeHelper.IsValidForDrawing(target, memberGuid, drawOnMask))
            return new Error();
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask);
        image.SetBlendMode(SKBlendMode.SrcOver);
        DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, image, memberGuid, drawOnMask);
        return new Success();
    }

    private static (VecD, VecD) FindCubicPoints(VecD prev, VecD mid1, VecD mid2, VecD next)
    {
        var ampl = (mid1 - mid2).Length / 3;
        var vec1 = (mid2 - prev).Normalize() * ampl;
        var vec2 = (mid1 - next).Normalize() * ampl;
        return (mid1 + vec1, mid2 + vec2);
    }

    private void FastforwardEnqueueDrawPath(ChunkyImage image)
    {
        for (int i = 0; i < points.Count; i++)
        {
            UpdateTempPath(i + 1);
            image.EnqueueDrawPath(tempPath, color, strokeWidth, SKStrokeCap.Round, SKBlendMode.Src);
        }
    }

    private void UpdateTempPathFinish()
    {
        tempPath.Reset();
        if (points.Count == 1)
        {
            tempPath.MoveTo((SKPoint)points[0]);
            return;
        }
        if (points.Count == 2)
        {
            tempPath.MoveTo((SKPoint)points[0]);
            tempPath.LineTo((SKPoint)points[1]);
            return;
        }
        var (mid, _) = FindCubicPoints(points[^3], points[^2], points[^1], points[^1]);
        tempPath.MoveTo((SKPoint)points[^2]);
        tempPath.QuadTo((SKPoint)mid, (SKPoint)points[^1]);
        return;
    }

    private void UpdateTempPath(int pointsCount)
    {
        tempPath.Reset();
        if (pointsCount is 1 or 2)
        {
            tempPath.MoveTo((SKPoint)points[0]);
            return;
        }
        if (pointsCount == 3)
        {
            var (mid, _) = FindCubicPoints(points[0], points[1], points[2], points[2]);
            tempPath.MoveTo((SKPoint)points[0]);
            tempPath.QuadTo((SKPoint)mid, (SKPoint)points[2]);
            return;
        }

        var (mid1, mid2) = FindCubicPoints(points[pointsCount - 4], points[pointsCount - 3], points[pointsCount - 2], points[pointsCount - 1]);
        tempPath.MoveTo((SKPoint)points[pointsCount - 3]);
        tempPath.CubicTo((SKPoint)mid1, (SKPoint)mid2, (SKPoint)points[pointsCount - 2]);
        return;
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
            UpdateTempPathFinish();

            image.EnqueueDrawPath(tempPath, color, strokeWidth, SKStrokeCap.Round, SKBlendMode.Src);
            var affChunks = image.FindAffectedChunks();
            storedChunks = new CommittedChunkStorage(image, affChunks);
            image.CommitChanges();

            return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, affChunks, drawOnMask);
        }
        else
        {
            image.SetBlendMode(SKBlendMode.SrcOver);
            DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, image, memberGuid, drawOnMask);

            FastforwardEnqueueDrawPath(image);
            var affChunks = image.FindAffectedChunks();
            storedChunks = new CommittedChunkStorage(image, affChunks);
            image.CommitChanges();

            return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, affChunks, drawOnMask);
        }
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        UpdateTempPath(points.Count);
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask);

        int opCount = image.QueueLength;
        image.EnqueueDrawPath(tempPath, color, strokeWidth, SKStrokeCap.Round, SKBlendMode.Src);
        var affChunks = image.FindAffectedChunks(opCount);

        return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, affChunks, drawOnMask);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var affected = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(target, memberGuid, drawOnMask, ref storedChunks);
        return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, affected, drawOnMask);
    }

    public override void Dispose()
    {
        storedChunks?.Dispose();
        tempPath.Dispose();
    }
}
