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

    private List<MemberTransformationData> memberData;

    private VectorPath? originalPath;
    private RectD originalSelectionBounds;
    private VecD originalSize;

    private bool isTransformingSelection;
    private bool hasEnqueudImages = false;
    private int frame;
    private bool appliedOnce;
    
    private Matrix3X3 globalMatrix;
    
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
        this.keepOriginal = keepOriginal;
        this.drawOnMask = transformMask;
        this.frame = frame;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (memberData.Count == 0)
            return false;

        RectD originalTightBounds = default;
        bool hasSelection = target.Selection.SelectionPath is { IsEmpty: false };
        
        if (hasSelection)
        {
            originalPath = new VectorPath(target.Selection.SelectionPath) { FillType = PathFillType.EvenOdd };
            originalTightBounds = originalPath.TightBounds;
            originalSelectionBounds = originalTightBounds;
            originalSize = originalTightBounds.Size;
            isTransformingSelection = true;
        }
        else
        {
            StructureNode firstLayer = target.FindMemberOrThrow(memberData[0].MemberId);
            originalTightBounds = (RectD)firstLayer.GetTightBounds(frame).Value; 
            for (var i = 1; i < memberData.Count; i++)
            {
                StructureNode layer = target.FindMemberOrThrow(memberData[i].MemberId);
                originalTightBounds = originalTightBounds.Union((RectD)layer.GetTightBounds(frame).Value);
            }

            originalSize = originalTightBounds.Size;
        }
        
        foreach (var member in memberData)
        {
            StructureNode layer = target.FindMemberOrThrow(member.MemberId);

            if (layer is IReadOnlyImageNode)
            {
                SetImageMember(target, member, originalTightBounds, layer);
            }
            else if (layer is ITransformableObject transformable)
            {
                SetTransformableMember(layer, member, transformable);
            }
        }
        
        return true;
    }

    private void SetTransformableMember(StructureNode layer, MemberTransformationData member,
        ITransformableObject transformable)
    {
        RectI tightBounds = layer.GetTightBounds(frame).Value;
        member.OriginalBounds = (RectD)tightBounds;
        member.AddTransformableObject(transformable, transformable.TransformationMatrix);
    }

    private void SetImageMember(Document target, MemberTransformationData member, RectD originalTightBounds,
        StructureNode layer)
    {
        ChunkyImage image =
            DrawingChangeHelper.GetTargetImageOrThrow(target, member.MemberId, drawOnMask, frame);
        VectorPath pathToExtract = originalPath;
        RectD targetBounds = originalTightBounds;

        if (pathToExtract == null)
        {
            RectI tightBounds = layer.GetTightBounds(frame).GetValueOrDefault();
            pathToExtract = new VectorPath();
            pathToExtract.AddRect(tightBounds);
        }

        member.OriginalPath = pathToExtract;
        member.OriginalBounds = targetBounds;
        var extracted = ExtractArea(image, pathToExtract, member.RoundedOriginalBounds.Value);
        if (extracted.IsT0)
            return;
                
        member.AddImage(extracted.AsT1.image, extracted.AsT1.extractedRect.Pos);
    }

    [UpdateChangeMethod]
    public void Update(ShapeCorners masterCorners, bool keepOriginal)
    {
        this.keepOriginal = keepOriginal;
        this.masterCorners = masterCorners;
        
        globalMatrix = OperationHelper.CreateMatrixFromPoints(masterCorners, originalSize);

        foreach (var member in memberData)
        {
            member.LocalMatrix = globalMatrix;
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
                member.TransformableObject.TransformationMatrix = member.LocalMatrix;

                // TODO: this is probably wrong
                AffectedArea area = GetTranslationAffectedArea();
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
            if (member.IsImage)
            {
                ChunkyImage targetImage =
                    DrawingChangeHelper.GetTargetImageOrThrow(target, member.MemberId, drawOnMask, frame);

                infos.Add(DrawingChangeHelper.CreateAreaChangeInfo(member.MemberId,
                        DrawImage(member, targetImage), drawOnMask)
                    .AsT1);
            }
            else if (member.IsTransformable)
            {
                member.TransformableObject.TransformationMatrix = member.LocalMatrix; 

                // TODO: this is probably wrong
                AffectedArea translationAffectedArea = GetTranslationAffectedArea();
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
                member.TransformableObject.TransformationMatrix = member.OriginalMatrix!.Value;

                //TODO this is probably wrong
                AffectedArea area = GetTranslationAffectedArea();
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

    private AffectedArea GetTranslationAffectedArea()
    {
        RectI oldBounds = (RectI)masterCorners.AABBBounds.RoundOutwards();

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
    public ShapeCorners MemberCorners { get; init; }

    public ITransformableObject? TransformableObject { get; private set; }
    public Matrix3X3? OriginalMatrix { get; private set; }

    public CommittedChunkStorage? SavedChunks { get; set; }
    public VectorPath? OriginalPath { get; set; }
    public Surface? Image { get; set; }
    public RectD? OriginalBounds { get; set; }
    public VecI? ImagePos { get; set; }
    public bool IsImage => Image != null;
    public bool IsTransformable => TransformableObject != null;
    public RectI? RoundedOriginalBounds => (RectI?)OriginalBounds?.RoundOutwards();
    public Matrix3X3 LocalMatrix { get; set; }

    public MemberTransformationData(Guid memberId)
    {
        MemberId = memberId;
    }

    public void AddTransformableObject(ITransformableObject transformableObject, Matrix3X3 originalMatrix)
    {
        TransformableObject = transformableObject;
        OriginalMatrix = originalMatrix;
        LocalMatrix = originalMatrix;
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
