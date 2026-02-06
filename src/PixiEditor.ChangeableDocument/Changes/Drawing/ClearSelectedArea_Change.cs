using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;
internal class ClearSelectedArea_Change : Change
{
    private VectorPath clearArea;
    private readonly Guid memberGuid;
    private readonly bool drawOnMask;
    private CommittedChunkStorage? savedChunks;
    private int frame;

    [GenerateMakeChangeAction]
    public ClearSelectedArea_Change(Guid memberGuid, VectorPath clearArea, bool drawOnMask, int frame)
    {
        this.memberGuid = memberGuid;
        this.drawOnMask = drawOnMask;
        this.clearArea = clearArea;
        this.frame = frame;
    }

    public override bool InitializeAndValidate(Document target)
    {
        return clearArea is { IsEmpty: false } && DrawingChangeHelper.IsValidForDrawing(target, memberGuid, drawOnMask, frame);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        if (savedChunks is not null)
            throw new InvalidOperationException("trying to save chunks while they are already saved");

        var image = DrawingChangeHelper.GetTargetImageOrThrow(target, memberGuid, drawOnMask, frame);

        RectD bounds = clearArea.Bounds;
        RectI intBounds = (RectI)bounds.Intersect(new RectD(0, 0, target.Size.X, target.Size.Y)).RoundOutwards();

        image.EnqueueClearPath(clearArea, intBounds);
        var affArea = image.FindAffectedArea();
        savedChunks = new(image, affArea.Chunks);
        image.CommitChanges();
        ignoreInUndo = false;
        return DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, affArea, drawOnMask);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var affArea = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(target, memberGuid, drawOnMask, frame, ref savedChunks);
        return DrawingChangeHelper.CreateAreaChangeInfo(memberGuid, affArea, drawOnMask);
    }

    public override void Dispose()
    {
        savedChunks?.Dispose();
    }
}
