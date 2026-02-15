using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;
internal class PasteImage_UpdateableChange : InterruptableUpdateableChange
{
    private ShapeCorners corners;
    private readonly Guid memberGuid;
    private readonly bool ignoreClipsSymmetriesEtc;
    private readonly bool drawOnMask;
    private readonly Surface imageToPaste;
    private CommittedChunkStorage? savedChunks;
    private int? frame;
    private Guid? targetKeyFrameGuid;
    private static Paint RegularPaint { get; } = new Paint() { BlendMode = BlendMode.SrcOver };

    private bool hasEnqueudImage = false;

    [GenerateUpdateableChangeActions]
    public PasteImage_UpdateableChange(Surface image, ShapeCorners corners, Guid memberGuid, bool ignoreClipsSymmetriesEtc, bool isDrawingOnMask, int frame, Guid targetKeyFrameGuid)
    {
        this.corners = corners;
        this.memberGuid = memberGuid;
        this.ignoreClipsSymmetriesEtc = ignoreClipsSymmetriesEtc;
        this.drawOnMask = isDrawingOnMask;
        this.imageToPaste = new Surface(image);
        this.frame = frame;
        this.targetKeyFrameGuid = targetKeyFrameGuid;
    }

    public override bool InitializeAndValidate(Document target)
    {
        return targetKeyFrameGuid != null ? DrawingChangeHelper.IsValidForDrawing(target, memberGuid, drawOnMask, targetKeyFrameGuid.Value) : DrawingChangeHelper.IsValidForDrawing(target, memberGuid, drawOnMask, frame ?? 0);
    }

    [UpdateChangeMethod]
    public void Update(ShapeCorners corners)
    {
        this.corners = corners;
    }

    private AffectedArea DrawImage(Document target, ChunkyImage targetImage)
    {
        var prevAffArea = targetImage.FindAffectedArea();

        targetImage.CancelChanges();
        if (!ignoreClipsSymmetriesEtc)
            DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, targetImage, memberGuid, drawOnMask);
        targetImage.EnqueueDrawImage(corners, imageToPaste, RegularPaint, false);
        hasEnqueudImage = true;

        var affArea = targetImage.FindAffectedArea();
        affArea.UnionWith(prevAffArea);
        return affArea;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        ChunkyImage targetImage;
        if (targetKeyFrameGuid.HasValue && targetKeyFrameGuid != Guid.Empty)
        {
            targetImage = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask, targetKeyFrameGuid.Value);
        }
        else
        {
            targetImage = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask, frame.Value);
        }
        
        var chunks = DrawImage(target, targetImage);
        savedChunks?.Dispose();
        savedChunks = new(targetImage, targetImage.FindAffectedArea().Chunks);
        targetImage.CommitChanges();
        hasEnqueudImage = false;
        ignoreInUndo = false;
        return DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, chunks, drawOnMask);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        ChunkyImage targetImage;
        if (targetKeyFrameGuid.HasValue && targetKeyFrameGuid != Guid.Empty)
        {
            targetImage = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask, targetKeyFrameGuid.Value);
        }
        else
        {
            targetImage = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask, frame.Value);
        }
        return DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, DrawImage(target, targetImage), drawOnMask);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        AffectedArea chunks;
        if (targetKeyFrameGuid.HasValue && targetKeyFrameGuid != Guid.Empty)
        {
            chunks = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(target, memberGuid, drawOnMask, targetKeyFrameGuid.Value, ref savedChunks);
        }
        else
        {
            chunks = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(target, memberGuid, drawOnMask, frame.Value, ref savedChunks);
        }
        
        return DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, chunks, drawOnMask);
    }

    public override void Dispose()
    {
        if (hasEnqueudImage)
            throw new InvalidOperationException("Attempted to dispose the change while it's internally stored image is still used enqueued in some ChunkyImage. Most likely someone tried to dispose a change after ApplyTemporarily was called but before the subsequent call to Apply. Don't do that.");
        imageToPaste.Dispose();
        savedChunks?.Dispose();
    }
}
