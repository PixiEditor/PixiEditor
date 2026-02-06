using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;
internal class DrawRasterEllipse_UpdateableChange : UpdateableChange
{
    private readonly Guid memberGuid;
    private RectI location;
    private double rotation;
    private Paintable strokeColor;
    private Paintable fillColor;
    private float strokeWidth;
    private readonly bool drawOnMask;
    private int frame;
    private bool antialiased;

    private CommittedChunkStorage? storedChunks;

    [GenerateUpdateableChangeActions]
    public DrawRasterEllipse_UpdateableChange(Guid memberGuid, RectI location, double rotationRad, Paintable strokeColor, Paintable fillColor, float strokeWidth, bool antialiased, bool drawOnMask, int frame)
    {
        this.memberGuid = memberGuid;
        this.location = location;
        this.rotation = rotationRad;
        this.strokeColor = strokeColor;
        this.fillColor = fillColor;
        this.strokeWidth = strokeWidth;
        this.drawOnMask = drawOnMask;
        this.frame = frame;
        this.antialiased = antialiased;
    }

    [UpdateChangeMethod]
    public void Update(RectI location, double rotationRad, Paintable strokeColor, Paintable fillColor, float strokeWidth)
    {
        this.location = location;
        rotation = rotationRad;
        this.strokeColor = strokeColor;
        this.fillColor = fillColor;
        this.strokeWidth = strokeWidth;
    }

    public override bool InitializeAndValidate(Document target)
    {
        return DrawingChangeHelper.IsValidForDrawing(target, memberGuid, drawOnMask, frame);
    }

    private AffectedArea UpdateEllipse(Document target, ChunkyImage targetImage)
    {
        var oldAffectedChunks = targetImage.FindAffectedArea();

        targetImage.CancelChanges();

        if (!location.IsZeroOrNegativeArea)
        {
            DrawingChangeHelper.ApplyClipsSymmetriesEtc(target, targetImage, memberGuid, drawOnMask);
            targetImage.EnqueueDrawEllipse((RectD)location, strokeColor, fillColor, strokeWidth, rotation, antialiased);
        }

        var affectedArea = targetImage.FindAffectedArea();
        affectedArea.UnionWith(oldAffectedChunks);

        return affectedArea;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        if (location.IsZeroOrNegativeArea)
        {
            ignoreInUndo = true;
            return new None();
        }

        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask, frame);
        var area = UpdateEllipse(target, image);
        storedChunks = new CommittedChunkStorage(image, image.FindAffectedArea().Chunks);
        image.CommitChanges();
        ignoreInUndo = false;
        return DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, area, drawOnMask);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask, frame);
        var area = UpdateEllipse(target, image);
        return DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, area, drawOnMask);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var affArea = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(target, memberGuid, drawOnMask, frame, ref storedChunks);
        var changes = DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, affArea, drawOnMask);
        return changes;
    }

    public override void Dispose()
    {
        storedChunks?.Dispose();
    }
}
