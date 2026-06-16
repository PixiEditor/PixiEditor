using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;
internal class PathBasedPen_UpdateableChange : UpdateableChange
{
    private readonly Guid memberGuid;
    private readonly Color color;
    private readonly float strokeWidth;
    private readonly bool drawOnMask;

    private CommittedChunkStorage? storedChunks;
    private VectorPath tempPath = new();

    private List<VecD> points = new();
    private int frame;

    [GenerateUpdateableChangeActions]
    public PathBasedPen_UpdateableChange(Guid memberGuid, VecD pos, Color color, float strokeWidth, bool drawOnMask, int frame)
    {
        this.memberGuid = memberGuid;
        this.color = color;
        this.strokeWidth = strokeWidth;
        this.drawOnMask = drawOnMask;
        points.Add(pos);
        this.frame = frame;
    }

    [UpdateChangeMethod]
    public void Update(VecD pos)
    {
        points.Add(pos);
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (!DrawingChangeHelper.IsValidForDrawing(target, memberGuid, drawOnMask, frame))
            return false;
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask, frame);
        image.SetBlendMode(BlendMode.SrcOver);
        DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, image, memberGuid, drawOnMask);
        return true;
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
            image.EnqueueDrawPath(tempPath, color, strokeWidth, StrokeCap.Round, BlendMode.Src);
        }
    }

    private void UpdateTempPathFinish()
    {
        tempPath.Reset();
        if (points.Count == 1)
        {
            tempPath.MoveTo((VecF)points[0]);
            return;
        }
        if (points.Count == 2)
        {
            tempPath.MoveTo((VecF)points[0]);
            tempPath.LineTo((VecF)points[1]);
            return;
        }
        var (mid, _) = FindCubicPoints(points[^3], points[^2], points[^1], points[^1]);
        tempPath.MoveTo((VecF)points[^2]);
        tempPath.QuadTo((VecF)mid, (VecF)points[^1]);
    }

    private void UpdateTempPath(int pointsCount)
    {
        tempPath.Reset();
        if (pointsCount is 1 or 2)
        {
            tempPath.MoveTo((VecF)points[0]);
            return;
        }
        if (pointsCount == 3)
        {
            var (mid, _) = FindCubicPoints(points[0], points[1], points[2], points[2]);
            tempPath.MoveTo((VecF)points[0]);
            tempPath.QuadTo((VecF)mid, (VecF)points[2]);
            return;
        }

        var (mid1, mid2) = FindCubicPoints(points[pointsCount - 4], points[pointsCount - 3], points[pointsCount - 2], points[pointsCount - 1]);
        tempPath.MoveTo((VecF)points[pointsCount - 3]);
        tempPath.CubicTo((VecF)mid1, (VecF)mid2, (VecF)points[pointsCount - 2]);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        if (storedChunks is not null)
            throw new InvalidOperationException("Trying to save chunks while there are saved chunks already");
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask, frame);

        ignoreInUndo = false;
        if (firstApply)
        {
            UpdateTempPathFinish();

            image.EnqueueDrawPath(tempPath, color, strokeWidth, StrokeCap.Round, BlendMode.Src);
            var affArea = image.FindAffectedArea();
            storedChunks = new CommittedChunkStorage(image, affArea.Chunks);
            image.CommitChanges();

            return DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, affArea, drawOnMask);
        }
        else
        {
            image.SetBlendMode(BlendMode.SrcOver);
            DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, image, memberGuid, drawOnMask);

            FastforwardEnqueueDrawPath(image);
            var affArea = image.FindAffectedArea();
            storedChunks = new CommittedChunkStorage(image, affArea.Chunks);
            image.CommitChanges();

            return DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, affArea, drawOnMask);
        }
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        UpdateTempPath(points.Count);
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask, frame);

        int opCount = image.QueueLength;
        image.EnqueueDrawPath(tempPath, color, strokeWidth, StrokeCap.Round, BlendMode.Src);
        var affArea = image.FindAffectedArea(opCount);

        return DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, affArea, drawOnMask);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var affected = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(target, memberGuid, drawOnMask, frame, ref storedChunks);
        return DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, affected, drawOnMask);
    }

    public override void Dispose()
    {
        storedChunks?.Dispose();
        tempPath.Dispose();
    }
}
