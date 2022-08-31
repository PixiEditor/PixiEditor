using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Changes.Selection;
using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;
internal class TransformSelectedArea_UpdateableChange : UpdateableChange
{
    private readonly Guid[] membersToTransform;
    private readonly bool drawOnMask;
    private bool keepOriginal;
    private ShapeCorners corners;

    private Dictionary<Guid, (Surface surface, VecI pos)>? images;
    private SKMatrix globalMatrix;
    private RectI originalTightBounds;
    private Dictionary<Guid, CommittedChunkStorage>? savedChunks;

    private SKPath? originalPath;

    private bool hasEnqueudImages = false;

    private static SKPaint RegularPaint { get; } = new SKPaint() { BlendMode = SKBlendMode.SrcOver };

    [GenerateUpdateableChangeActions]
    public TransformSelectedArea_UpdateableChange(
        IEnumerable<Guid> membersToTransform,
        ShapeCorners corners,
        bool keepOriginal,
        bool transformMask)
    {
        this.membersToTransform = membersToTransform.Select(static a => a).ToArray();
        this.corners = corners;
        this.keepOriginal = keepOriginal;
        this.drawOnMask = transformMask;
    }

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        if (membersToTransform.Length == 0 || target.Selection.SelectionPath.IsEmpty)
            return new Error();

        foreach (var guid in membersToTransform)
        {
            if (!DrawingChangeHelper.IsValidForDrawing(target, guid, drawOnMask))
                return new Error();
        }

        originalPath = new SKPath(target.Selection.SelectionPath) { FillType = SKPathFillType.EvenOdd };
        RectI bounds = (RectI)originalPath.TightBounds;

        images = new();
        foreach (var guid in membersToTransform)
        {
            ChunkyImage image = DrawingChangeHelper.GetTargetImageOrThrow(target, guid, drawOnMask);
            var extracted = ExtractArea(image, originalPath, bounds);
            if (extracted.IsT0)
                continue;
            images.Add(guid, (extracted.AsT1.image, extracted.AsT1.extractedRect.Pos));
        }
        if (images.Count == 0)
            return new Error();
        originalTightBounds = bounds;
        globalMatrix = OperationHelper.CreateMatrixFromPoints(corners, originalTightBounds.Size);
        return new Success();
    }

    public OneOf<None, (Surface image, RectI extractedRect)> ExtractArea(ChunkyImage image, SKPath path, RectI pathBounds)
    {
        // get rid of transparent areas on edges
        var memberImageBounds = image.FindLatestBounds();
        if (memberImageBounds is null)
            return new None();
        pathBounds = pathBounds.Intersect(memberImageBounds.Value);
        pathBounds = pathBounds.Intersect(new RectI(VecI.Zero, image.LatestSize));
        if (pathBounds.IsZeroOrNegativeArea)
            return new None();

        // shift the clip to account for the image being smaller than the selection
        SKPath clipPath = new SKPath(path) { FillType = SKPathFillType.EvenOdd };
        clipPath.Transform(SKMatrix.CreateTranslation(-pathBounds.X, -pathBounds.Y));

        // draw
        Surface output = new(pathBounds.Size);
        output.DrawingSurface.Canvas.Save();
        output.DrawingSurface.Canvas.ClipPath(clipPath);
        image.DrawMostUpToDateRegionOn(pathBounds, ChunkResolution.Full, output.DrawingSurface, VecI.Zero);
        output.DrawingSurface.Canvas.Restore();

        return (output, pathBounds);
    }

    [UpdateChangeMethod]
    public void Update(ShapeCorners corners, bool keepOriginal)
    {
        this.keepOriginal = keepOriginal;
        this.corners = corners;
        globalMatrix = OperationHelper.CreateMatrixFromPoints(corners, originalTightBounds.Size);
    }

    private HashSet<VecI> DrawImage(Document doc, Guid memberGuid, Surface image, VecI originalPos, ChunkyImage memberImage)
    {
        var prevChunks = memberImage.FindAffectedChunks();

        memberImage.CancelChanges();

        if (!keepOriginal)
            memberImage.EnqueueClearPath(originalPath!, originalTightBounds);
        SKMatrix localMatrix = SKMatrix.CreateTranslation(originalPos.X - originalTightBounds.Left, originalPos.Y - originalTightBounds.Top);
        localMatrix = localMatrix.PostConcat(globalMatrix);
        memberImage.EnqueueDrawImage(localMatrix, image, RegularPaint, false);
        hasEnqueudImages = true;

        var affectedChunks = memberImage.FindAffectedChunks();
        affectedChunks.UnionWith(prevChunks);
        return affectedChunks;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        if (savedChunks is not null)
            throw new InvalidOperationException("Apply called twice");
        savedChunks = new();

        List<IChangeInfo> infos = new();
        foreach (var (guid, (image, pos)) in images!)
        {
            ChunkyImage memberImage = DrawingChangeHelper.GetTargetImageOrThrow(target, guid, drawOnMask);
            var chunks = DrawImage(target, guid, image, pos, memberImage);
            savedChunks[guid] = new(memberImage, memberImage.FindAffectedChunks());
            memberImage.CommitChanges();
            infos.Add(DrawingChangeHelper.CreateChunkChangeInfo(guid, chunks, drawOnMask).AsT1);
        }

        infos.Add(SelectionChangeHelper.DoSelectionTransform(target, originalPath!, originalTightBounds, corners));

        hasEnqueudImages = false;
        ignoreInUndo = false;
        return infos;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        List<IChangeInfo> infos = new();
        foreach (var (guid, (image, pos)) in images!)
        {
            ChunkyImage targetImage = DrawingChangeHelper.GetTargetImageOrThrow(target, guid, drawOnMask);
            infos.Add(DrawingChangeHelper.CreateChunkChangeInfo(guid, DrawImage(target, guid, image, pos, targetImage), drawOnMask).AsT1);
        }
        infos.Add(SelectionChangeHelper.DoSelectionTransform(target, originalPath!, originalTightBounds, corners));
        return infos;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        List<IChangeInfo> infos = new();
        foreach (var (guid, storage) in savedChunks!)
        {
            var storageCopy = storage;
            var chunks = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(target, guid, drawOnMask, ref storageCopy);
            infos.Add(DrawingChangeHelper.CreateChunkChangeInfo(guid, chunks, drawOnMask).AsT1);
        }
        savedChunks = null;
        return infos;
    }

    public override void Dispose()
    {
        if (hasEnqueudImages)
            throw new InvalidOperationException("Attempted to dispose the change while it's internally stored image is still used enqueued in some ChunkyImage. Most likely someone tried to dispose a change after ApplyTemporarily was called but before the subsequent call to Apply. Don't do that.");
        foreach (var (_, (image, _)) in images!)
        {
            image.Dispose();
        }

        if (savedChunks is not null)
        {
            foreach (var (_, chunks) in savedChunks)
            {
                chunks.Dispose();
            }
        }
    }
}
