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

internal class TransformSelected_UpdateableChange : UpdateableChange
{
    private readonly bool drawOnMask;
    private bool keepOriginal;
    private ShapeCorners masterCorners;
    private VecD originalMasterCornersSize;

    private List<MemberTransformationData> memberData;

    private VectorPath? originalPath;
    private RectD originalSelectionBounds;

    private bool isTransformingSelection;
    private bool hasEnqueudImages = false;
    private int frame;
    private ShapeCorners lastCorners;
    private bool appliedOnce;
    private static Paint RegularPaint { get; } = new() { BlendMode = BlendMode.SrcOver };

    [GenerateUpdateableChangeActions]
    public TransformSelected_UpdateableChange(
        ShapeCorners masterCorners,
        bool keepOriginal,
        Dictionary<Guid, ShapeCorners> memberCorners,
        bool transformMask,
        int frame)
    {
        memberData = new();
        foreach (var corners in memberCorners)
        {
            memberData.Add(new MemberTransformationData(corners.Key) { MemberCorners = corners.Value });
        }

        this.masterCorners = masterCorners;
        lastCorners = masterCorners;
        this.keepOriginal = keepOriginal;
        this.drawOnMask = transformMask;
        this.frame = frame;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (memberData.Count == 0)
            return false;

        RectD originalTightBounds = default;

        if (target.Selection.SelectionPath is { IsEmpty: false })
        {
            originalPath = new VectorPath(target.Selection.SelectionPath) { FillType = PathFillType.EvenOdd };
            originalTightBounds = originalPath.TightBounds;
            originalSelectionBounds = masterCorners.AABBBounds;
            isTransformingSelection = true;
        }

        foreach (var member in memberData)
        {
            StructureNode layer = target.FindMemberOrThrow(member.MemberId);

            if (layer is IReadOnlyImageNode)
            {
                ChunkyImage image =
                    DrawingChangeHelper.GetTargetImageOrThrow(target, member.MemberId, drawOnMask, frame);
                VectorPath pathToExtract = originalPath;
                RectD targetBounds = originalTightBounds;

                if (pathToExtract == null)
                {
                    RectI tightBounds = layer.GetTightBounds(frame).Value;
                    pathToExtract = new VectorPath();
                    pathToExtract.AddRect(tightBounds);
                    targetBounds = pathToExtract.Bounds;
                }

                member.OriginalPath = pathToExtract;
                member.OriginalBounds = targetBounds;
                member.GlobalMatrix = OperationHelper.CreateMatrixFromPoints(member.MemberCorners, targetBounds.Size);
                var extracted = ExtractArea(image, pathToExtract, member.RoundedOriginalBounds.Value);
                if (extracted.IsT0)
                    continue;

                member.AddImage(extracted.AsT1.image, extracted.AsT1.extractedRect.Pos);
            }
            else if (layer is ITransformableObject transformable)
            {
                member.AddTransformableObject(transformable,
                    new ShapeCorners(transformable.Position, transformable.Size)
                        .AsRotated(transformable.RotationRadians, transformable.Position));
            }
        }

        originalMasterCornersSize = masterCorners.RectSize;
        return true;
    }

    [UpdateChangeMethod]
    public void Update(ShapeCorners masterCorners, bool keepOriginal)
    {
        this.keepOriginal = keepOriginal;
        lastCorners = this.masterCorners;
        this.masterCorners = masterCorners;

        foreach (var member in memberData)
        {
            if (member.IsImage)
            {
                var corners = MasterToMemberCoords(member.MemberCorners);
                member.GlobalMatrix = OperationHelper.CreateMatrixFromPoints(corners, member.OriginalBounds.Value.Size);
            }
        }
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

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        if (appliedOnce)
            throw new InvalidOperationException("Apply called twice");
        appliedOnce = true;

        List<IChangeInfo> infos = new();

        foreach (var member in memberData)
        {
            if (member.IsImage)
            {
                ChunkyImage memberImage =
                    DrawingChangeHelper.GetTargetImageOrThrow(target, member.MemberId, drawOnMask, frame);
                var area = DrawImage(member, memberImage);
                member.SavedChunks = new(memberImage, memberImage.FindAffectedArea().Chunks);
                memberImage.CommitChanges();
                infos.Add(DrawingChangeHelper.CreateAreaChangeInfo(member.MemberId, area, drawOnMask).AsT1);
            }
            else if (member.IsTransformable)
            {
                ShapeCorners localCorners = MasterToMemberCoords(member.MemberCorners);

                member.TransformableObject.Position = localCorners.RectCenter;
                member.TransformableObject.Size = localCorners.RectSize;
                member.TransformableObject.RotationRadians = localCorners.RectRotation;

                member.MemberCorners = localCorners;

                AffectedArea area = GetTranslationAffectedArea(member.OriginalCorners.Value);
                infos.Add(new TransformObject_ChangeInfo(member.MemberId, area));
            }
        }

        if (isTransformingSelection)
        {
            infos.Add(SelectionChangeHelper.DoSelectionTransform(target, originalPath!, originalSelectionBounds,
                masterCorners));
        }

        hasEnqueudImages = false;
        ignoreInUndo = false;
        return infos;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        List<IChangeInfo> infos = new();

        foreach (var member in memberData)
        {
            ShapeCorners localCorners = MasterToMemberCoords(member.MemberCorners);
            
            if (member.IsImage)
            {
                ChunkyImage targetImage =
                    DrawingChangeHelper.GetTargetImageOrThrow(target, member.MemberId, drawOnMask, frame);
                
                member.MemberCorners = localCorners;
                
                infos.Add(DrawingChangeHelper.CreateAreaChangeInfo(member.MemberId,
                        DrawImage(member, targetImage), drawOnMask)
                    .AsT1);
            }
            else if (member.IsTransformable)
            {
                VecD translated = localCorners.RectCenter;
                member.TransformableObject.Position = translated;
                member.TransformableObject.Size = localCorners.RectSize;
                member.TransformableObject.RotationRadians = localCorners.RectRotation; 

                member.MemberCorners = localCorners;

                AffectedArea translationAffectedArea = GetTranslationAffectedArea(member.OriginalCorners.Value);
                infos.Add(new TransformObject_ChangeInfo(member.MemberId, translationAffectedArea));
            }
        }


        if (isTransformingSelection)
        {
            infos.Add(SelectionChangeHelper.DoSelectionTransform(target, originalPath!, originalSelectionBounds,
                masterCorners));
        }

        return infos;
    }

    private ShapeCorners MasterToMemberCoords(ShapeCorners memberCorner)
    {
        VecD posDiff = masterCorners.RectCenter - lastCorners.RectCenter;
        VecD sizeDiff = masterCorners.RectSize - lastCorners.RectSize;
        double rotDiff = masterCorners.RectRotation - lastCorners.RectRotation;

        ShapeCorners localCorners =
            new ShapeCorners(memberCorner.RectCenter + posDiff, memberCorner.RectSize + sizeDiff)
                .AsRotated(memberCorner.RectRotation, memberCorner.RectCenter + posDiff)
                .AsRotated(rotDiff, masterCorners.RectCenter);

        return localCorners;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        List<IChangeInfo> infos = new();

        foreach (var member in memberData)
        {
            if (member.SavedChunks is not null)
            {
                var storageCopy = member.SavedChunks;
                var chunks =
                    DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(target, member.MemberId, drawOnMask, frame,
                        ref storageCopy);

                member.SavedChunks = null;
                infos.Add(DrawingChangeHelper.CreateAreaChangeInfo(member.MemberId, chunks, drawOnMask).AsT1);
            }
            else if (member.IsTransformable)
            {
                member.TransformableObject.Position = member.OriginalCorners.Value.RectCenter;
                member.TransformableObject.Size = member.OriginalCorners.Value.RectSize;
                member.TransformableObject.RotationRadians = member.OriginalCorners.Value.RectRotation;

                AffectedArea area = GetTranslationAffectedArea(member.OriginalCorners.Value);
                infos.Add(new TransformObject_ChangeInfo(member.MemberId, area));
            }
        }

        if (originalPath != null)
        {
            (var toDispose, target.Selection.SelectionPath) =
                (target.Selection.SelectionPath, new VectorPath(originalPath!));
            toDispose.Dispose();
        }

        infos.Add(new Selection_ChangeInfo(new VectorPath(target.Selection.SelectionPath)));

        return infos;
    }

    public override void Dispose()
    {
        if (hasEnqueudImages)
            throw new InvalidOperationException(
                "Attempted to dispose the change while it's internally stored image is still used enqueued in some ChunkyImage. Most likely someone tried to dispose a change after ApplyTemporarily was called but before the subsequent call to Apply. Don't do that.");

        foreach (var member in memberData)
        {
            member.Dispose();
        }
    }

    private AffectedArea GetTranslationAffectedArea(ShapeCorners originalCorners)
    {
        RectI oldBounds = (RectI)originalCorners.AABBBounds.RoundOutwards();

        HashSet<VecI> chunks = new();
        VecI topLeftChunk = new VecI((int)oldBounds.Left / ChunkyImage.FullChunkSize,
            (int)oldBounds.Top / ChunkyImage.FullChunkSize);
        VecI bottomRightChunk = new VecI((int)oldBounds.Right / ChunkyImage.FullChunkSize,
            (int)oldBounds.Bottom / ChunkyImage.FullChunkSize);

        for (int x = topLeftChunk.X; x <= bottomRightChunk.X; x++)
        {
            for (int y = topLeftChunk.Y; y <= bottomRightChunk.Y; y++)
            {
                chunks.Add(new VecI(x, y));
            }
        }

        return new AffectedArea(chunks);
    }

    private AffectedArea DrawImage(MemberTransformationData data, ChunkyImage memberImage)
    {
        var prevAffArea = memberImage.FindAffectedArea();

        memberImage.CancelChanges();
        
        Matrix3X3 globalMatrix = data.GlobalMatrix!.Value; 

        var originalPos = data.ImagePos!.Value;

        if (!keepOriginal)
            memberImage.EnqueueClearPath(data.OriginalPath!, data.RoundedOriginalBounds!.Value);
        Matrix3X3 localMatrix = Matrix3X3.CreateTranslation(originalPos.X - (float)data.OriginalBounds.Value.Left,
            originalPos.Y - (float)data.OriginalBounds.Value.Top);
        localMatrix = localMatrix.PostConcat(globalMatrix);
        memberImage.EnqueueDrawImage(localMatrix, data.Image, RegularPaint, false);
        hasEnqueudImages = true;

        var affectedArea = memberImage.FindAffectedArea();
        affectedArea.UnionWith(prevAffArea);
        return affectedArea;
    }
}

class MemberTransformationData : IDisposable
{
    public Guid MemberId { get; }
    public ShapeCorners MemberCorners { get; set; }

    public ITransformableObject? TransformableObject { get; private set; }
    public ShapeCorners? OriginalCorners { get; private set; }

    public CommittedChunkStorage? SavedChunks { get; set; }
    public VectorPath? OriginalPath { get; set; }
    public Surface? Image { get; set; }
    public RectD? OriginalBounds { get; set; }
    public VecI? ImagePos { get; set; }
    public bool IsImage => Image != null;
    public bool IsTransformable => TransformableObject != null;
    public RectI? RoundedOriginalBounds => (RectI?)OriginalBounds?.RoundOutwards();
    public Matrix3X3? GlobalMatrix { get; set; }

    public MemberTransformationData(Guid memberId)
    {
        MemberId = memberId;
    }

    public void AddTransformableObject(ITransformableObject transformableObject, ShapeCorners originalCorners)
    {
        TransformableObject = transformableObject;
        OriginalCorners = originalCorners;
    }

    public void AddImage(Surface img, VecI extractedRectPos)
    {
        Image = img;
        ImagePos = extractedRectPos;
    }

    public void Dispose()
    {
        Image?.Dispose();
        Image = null;
        OriginalPath?.Dispose();
        OriginalPath = null;
        SavedChunks?.Dispose();
    }
}
