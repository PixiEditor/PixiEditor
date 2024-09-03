using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Objects;
using PixiEditor.ChangeableDocument.Changes.Selection;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.DrawingApi.Core.Surfaces.Vector;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class TransformSelectedArea_UpdateableChange : UpdateableChange
{
    private readonly Guid[] membersToTransform;
    private readonly bool drawOnMask;
    private bool keepOriginal;
    private ShapeCorners corners;

    private Dictionary<Guid, (Surface surface, VecI pos)>? images;
    private Dictionary<Guid, (ITransformableObject, ShapeCorners original)>? transformableObjectMembers;
    private Matrix3X3 globalMatrix;
    private Dictionary<Guid, CommittedChunkStorage>? savedChunks;

    private RectD originalTightBounds;
    private RectI roundedTightBounds;
    private VectorPath? originalPath;

    private bool hasEnqueudImages = false;
    private int frame;

    private static Paint RegularPaint { get; } = new() { BlendMode = BlendMode.SrcOver };

    [GenerateUpdateableChangeActions]
    public TransformSelectedArea_UpdateableChange(
        IEnumerable<Guid> membersToTransform,
        ShapeCorners corners,
        bool keepOriginal,
        bool transformMask,
        int frame)
    {
        this.membersToTransform = membersToTransform.Select(static a => a).ToArray();
        this.corners = corners;
        this.keepOriginal = keepOriginal;
        this.drawOnMask = transformMask;
        this.frame = frame;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (membersToTransform.Length == 0)
            return false;

        VectorPath path = !target.Selection.SelectionPath.IsEmpty
            ? target.Selection.SelectionPath
            : GetSelectionFromMembers(target, membersToTransform);

        if (path.IsEmpty)
            return false;

        originalPath = new VectorPath(path) { FillType = PathFillType.EvenOdd };

        originalTightBounds = originalPath.TightBounds;
        roundedTightBounds = (RectI)originalTightBounds.RoundOutwards();
        //boundsRoundingOffset = bounds.TopLeft - roundedBounds.TopLeft;

        foreach (var guid in membersToTransform)
        {
            StructureNode layer = target.FindMemberOrThrow(guid);

            if (layer is ImageLayerNode)
            {
                ChunkyImage image = DrawingChangeHelper.GetTargetImageOrThrow(target, guid, drawOnMask, frame);
                var extracted = ExtractArea(image, originalPath, roundedTightBounds);
                if (extracted.IsT0)
                    continue;
                
                if (images is null)
                    images = new();
                
                images.Add(guid, (extracted.AsT1.image, extracted.AsT1.extractedRect.Pos));
            }
            else if (layer is ITransformableObject transformable)
            {
                transformableObjectMembers ??= new();
                transformableObjectMembers.Add(guid, (transformable, new ShapeCorners(transformable.Position, transformable.Size).AsRotated(transformable.RotationRadians, transformable.Position)));
            } 
        }
        
        if (images is null && transformableObjectMembers is null)
            return false;

        globalMatrix = OperationHelper.CreateMatrixFromPoints(corners, originalTightBounds.Size);
        return true;
    }

    public OneOf<None, (Surface image, RectI extractedRect)> ExtractArea(ChunkyImage image, VectorPath path,
        RectI pathBounds)
    {
        // get rid of transparent areas on edges
        var memberImageBounds = image.FindChunkAlignedMostUpToDateBounds();
        if (memberImageBounds is null)
            return new None();
        pathBounds = pathBounds.Intersect(memberImageBounds.Value);
        pathBounds = pathBounds.Intersect(new RectI(VecI.Zero, image.LatestSize));
        if (pathBounds.IsZeroOrNegativeArea)
            return new None();

        // shift the clip to account for the image being smaller than the selection
        VectorPath clipPath = new VectorPath(path) { FillType = PathFillType.EvenOdd };
        clipPath.Transform(Matrix3X3.CreateTranslation(-pathBounds.X, -pathBounds.Y));

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

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        if (savedChunks is not null)
            throw new InvalidOperationException("Apply called twice");
        savedChunks = new();

        List<IChangeInfo> infos = new();
        if (images != null)
        {
            foreach (var (guid, (image, pos)) in images)
            {
                ChunkyImage memberImage = DrawingChangeHelper.GetTargetImageOrThrow(target, guid, drawOnMask, frame);
                var area = DrawImage(image, pos, memberImage);
                savedChunks[guid] = new(memberImage, memberImage.FindAffectedArea().Chunks);
                memberImage.CommitChanges();
                infos.Add(DrawingChangeHelper.CreateAreaChangeInfo(guid, area, drawOnMask).AsT1);
            }
        }

        if (transformableObjectMembers != null)
        {
            foreach (var (guid, (transformable, pos)) in transformableObjectMembers!)
            {
                transformable.Position = corners.RectCenter;
                transformable.Size = corners.RectSize;
                transformable.RotationRadians = corners.RectRotation;
                
                AffectedArea area = GetTranslationAffectedArea();
                infos.Add(new TransformObject_ChangeInfo(guid, area));
            }
        }

        infos.Add(SelectionChangeHelper.DoSelectionTransform(target, originalPath!, originalTightBounds, corners));
        
        hasEnqueudImages = false;
        ignoreInUndo = false;
        return infos;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        List<IChangeInfo> infos = new();
        if (images != null)
        {
            foreach (var (guid, (image, pos)) in images)
            {
                ChunkyImage targetImage = DrawingChangeHelper.GetTargetImageOrThrow(target, guid, drawOnMask, frame);
                infos.Add(DrawingChangeHelper.CreateAreaChangeInfo(guid, DrawImage(image, pos, targetImage), drawOnMask)
                    .AsT1);
            }
        }

        if (transformableObjectMembers != null)
        {
            foreach (var (guid, (transformable, pos)) in transformableObjectMembers)
            {
                VecD translated = corners.RectCenter; 
                transformable.Position = translated;
                transformable.Size = corners.RectSize; 
                transformable.RotationRadians = corners.RectRotation;
                
                AffectedArea translationAffectedArea = GetTranslationAffectedArea();
                infos.Add(new TransformObject_ChangeInfo(guid, translationAffectedArea));
            }
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
            var chunks =
                DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(target, guid, drawOnMask, frame,
                    ref storageCopy);
            infos.Add(DrawingChangeHelper.CreateAreaChangeInfo(guid, chunks, drawOnMask).AsT1);
        }

        if (transformableObjectMembers != null)
        {
            foreach (var (guid, (transformable, original)) in transformableObjectMembers)
            {
                transformable.Position = original.RectCenter;
                transformable.Size = original.RectSize;
                transformable.RotationRadians = original.RectRotation;
                
                AffectedArea area = GetTranslationAffectedArea();
                infos.Add(new TransformObject_ChangeInfo(guid, area));
            }
        }

        (var toDispose, target.Selection.SelectionPath) =
            (target.Selection.SelectionPath, new VectorPath(originalPath!));
        toDispose.Dispose();
        infos.Add(new Selection_ChangeInfo(new VectorPath(target.Selection.SelectionPath)));

        savedChunks = null;
        return infos;
    }

    public override void Dispose()
    {
        if (hasEnqueudImages)
            throw new InvalidOperationException(
                "Attempted to dispose the change while it's internally stored image is still used enqueued in some ChunkyImage. Most likely someone tried to dispose a change after ApplyTemporarily was called but before the subsequent call to Apply. Don't do that.");

        if (images is not null)
        {
            foreach (var (_, (image, _)) in images)
            {
                image.Dispose();
            }
        }

        if (savedChunks is not null)
        {
            foreach (var (_, chunks) in savedChunks)
            {
                chunks.Dispose();
            }
        }
    }
    
    private AffectedArea GetTranslationAffectedArea()
    {
        RectD oldBounds = originalTightBounds;
        
        HashSet<VecI> chunks = new();
        VecI topLeftChunk = new VecI((int)oldBounds.Left / ChunkyImage.FullChunkSize, (int)oldBounds.Top / ChunkyImage.FullChunkSize);
        VecI bottomRightChunk = new VecI((int)oldBounds.Right / ChunkyImage.FullChunkSize, (int)oldBounds.Bottom / ChunkyImage.FullChunkSize);
        
        for (int x = topLeftChunk.X; x <= bottomRightChunk.X; x++)
        {
            for (int y = topLeftChunk.Y; y <= bottomRightChunk.Y; y++)
            {
                chunks.Add(new VecI(x, y));
            }
        }

        return new AffectedArea(chunks);
    }

    private AffectedArea DrawImage(Surface image, VecI originalPos, ChunkyImage memberImage)
    {
        var prevAffArea = memberImage.FindAffectedArea();

        memberImage.CancelChanges();

        if (!keepOriginal)
            memberImage.EnqueueClearPath(originalPath!, roundedTightBounds);
        Matrix3X3 localMatrix = Matrix3X3.CreateTranslation(originalPos.X - (float)originalTightBounds.Left,
            originalPos.Y - (float)originalTightBounds.Top);
        localMatrix = localMatrix.PostConcat(globalMatrix);
        memberImage.EnqueueDrawImage(localMatrix, image, RegularPaint, false);
        hasEnqueudImages = true;

        var affectedArea = memberImage.FindAffectedArea();
        affectedArea.UnionWith(prevAffArea);
        return affectedArea;
    }

    private VectorPath GetSelectionFromMembers(Document target, IEnumerable<Guid> members)
    {
        VectorPath path = new VectorPath();
        foreach (var guid in members)
        {
            var bounds = target.FindMember(guid).GetTightBounds(frame);
            if (bounds.HasValue)
            {
                path.AddRect(bounds.Value);
            }
        }

        return path;
    }
}
