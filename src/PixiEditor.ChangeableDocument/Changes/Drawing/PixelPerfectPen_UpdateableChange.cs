using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class PixelPerfectPen_UpdateableChange : UpdateableChange
{
    private readonly SKColor color;
    private readonly bool drawOnMask;
    private readonly Guid memberGuid;
    private readonly List<VecI> points = new();
    private CommittedChunkStorage? chunkStorage;

    [GenerateUpdateableChangeActions]
    public PixelPerfectPen_UpdateableChange(Guid memberGuid, VecI pos, SKColor color, bool drawOnMask)
    {
        this.memberGuid = memberGuid;
        this.color = color;
        this.drawOnMask = drawOnMask;
        points.Add(pos);
    }

    [UpdateChangeMethod]
    public void Update(VecI pos)
    {
        if (points[^1] != pos)
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

    private bool IsAngle(int lastPixelIndex)
    {
        if (lastPixelIndex < 3)
            return false;
        VecI first = points[lastPixelIndex - 2];
        VecI second = points[lastPixelIndex - 1];
        VecI third = points[lastPixelIndex];
        return first.X != third.X && first.Y != third.Y && (second - first).TaxicabLength == 1 && (second - third).TaxicabLength == 1;
    }

    private void DoDrawingIteration(ChunkyImage image, int pointsCount)
    {
        if (pointsCount == 1)
        {
            image.EnqueueDrawPixel(points[0], color, SKBlendMode.Src);
            return;
        }

        image.EnqueueDrawBresenhamLine(points[pointsCount - 2], points[pointsCount - 1], color, SKBlendMode.Src);
        if (pointsCount == 3 && IsAngle(pointsCount - 1) ||
            pointsCount >= 4 && IsAngle(pointsCount - 1) && !IsAngle(pointsCount - 2))
        {
            image.EnqueueDrawPixel(points[pointsCount - 2], SKColors.Transparent, SKBlendMode.Src);
        }
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        ChunkyImage image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask);

        int changeCount = image.QueueLength;
        DoDrawingIteration(image, points.Count);
        HashSet<VecI> affChunks = image.FindAffectedChunks(changeCount);
        return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, affChunks, drawOnMask);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, out bool ignoreInUndo)
    {
        if (chunkStorage is not null)
            throw new InvalidOperationException("Trying to save chunks while saved one already exist");

        ignoreInUndo = false;
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask);
        if (image.QueueLength == 0)
        {
            image.SetBlendMode(SKBlendMode.SrcOver);
            DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, image, memberGuid, drawOnMask);
            for (int i = 1; i <= points.Count; i++)
            {
                DoDrawingIteration(image, i);
            }
        }

        var affChunks = image.FindAffectedChunks();
        chunkStorage = new CommittedChunkStorage(image, affChunks);
        image.CommitChanges();
        return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, affChunks, drawOnMask);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var chunks = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(target, memberGuid, drawOnMask, ref chunkStorage);
        return DrawingChangeHelper.CreateChunkChangeInfo(memberGuid, chunks, drawOnMask);
    }
}
