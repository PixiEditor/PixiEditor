using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Objects;
using PixiEditor.ChangeableDocument.Changes.Selection;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.ChangeInfos.Vectors;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class PreviewTransformSelected_UpdateableChange : InterruptableUpdateableChange
{
    private readonly bool drawOnMask;
    private ShapeCorners masterCorners;

    private List<MemberTransformationData> memberData;

    private VectorPath? originalPath;
    private VecD selectionAwareSize;
    private VecD tightBoundsSize;
    private RectD cornersToSelectionOffset;
    private VecD originalCornersSize;

    private bool isTransformingSelection;
    private bool hasEnqueudImages = false;
    private int frame;
    private bool appliedOnce;
    private AffectedArea lastAffectedArea;

    private static Paint RegularPaint { get; } = new() { BlendMode = BlendMode.SrcOver };

    [GenerateUpdateableChangeActions]
    public PreviewTransformSelected_UpdateableChange(
        ShapeCorners masterCorners,
        Dictionary<Guid, ShapeCorners> memberCorners,
        int frame)
    {
        memberData = new();
        foreach (var corners in memberCorners)
        {
            memberData.Add(new MemberTransformationData(corners.Key) { MemberCorners = corners.Value });
        }

        this.masterCorners = masterCorners;
        this.frame = frame;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (memberData.Count == 0)
            return false;

        originalCornersSize = masterCorners.RectSize;
        RectD tightBoundsWithSelection = default;
        bool hasSelection = target.Selection.SelectionPath is { IsEmpty: false };

        if (hasSelection)
        {
            originalPath = new VectorPath(target.Selection.SelectionPath) { FillType = PathFillType.EvenOdd };
            tightBoundsWithSelection = originalPath.TightBounds;
            selectionAwareSize = tightBoundsWithSelection.Size;
            isTransformingSelection = true;

            tightBoundsSize = tightBoundsWithSelection.Size;
            cornersToSelectionOffset = new RectD(masterCorners.TopLeft - tightBoundsWithSelection.TopLeft,
                tightBoundsSize - masterCorners.RectSize);
        }

        StructureNode firstLayer = target.FindMemberOrThrow(memberData[0].MemberId);
        RectD tightBounds = firstLayer.GetTightBounds(frame) ?? default;

        if (memberData.Count == 1 && firstLayer is VectorLayerNode vectorLayer)
        {
            tightBounds = vectorLayer.ShapeData?.GeometryAABB ?? default;
        }

        for (var i = 1; i < memberData.Count; i++)
        {
            StructureNode layer = target.FindMemberOrThrow(memberData[i].MemberId);

            var layerTightBounds = layer.GetTightBounds(frame);

            if (tightBounds == default)
            {
                tightBounds = layerTightBounds.GetValueOrDefault();
            }

            if (layerTightBounds is not null)
            {
                tightBounds = tightBounds.Union(layerTightBounds.Value);
            }
        }

        if (tightBounds == default)
            return false;

        tightBoundsSize = tightBounds.Size;

        foreach (var member in memberData)
        {
            StructureNode layer = target.FindMemberOrThrow(member.MemberId);

            if (layer is IReadOnlyImageNode)
            {
                var targetBounds = tightBoundsWithSelection != default ? tightBoundsWithSelection : tightBounds;
                SetImageMember(target, member, targetBounds, layer);
            }
            else if (layer is IReadOnlyVectorNode vectorNode)
            {
                SetVectorMember(member, vectorNode, tightBounds);
            }
        }

        return true;
    }

    private void SetVectorMember(MemberTransformationData member,
        IReadOnlyVectorNode vectorNode, RectD tightBounds)
    {
        member.OriginalBounds = tightBounds;
        VecD posRelativeToMaster = member.OriginalBounds.Value.TopLeft - masterCorners.TopLeft;

        member.OriginalMatrix = vectorNode.ShapeData.TransformationMatrix;
        member.OriginalPos = (VecI)posRelativeToMaster;
        member.OriginalShapeData = vectorNode.ShapeData as ShapeVectorData;
        member.OriginalPath = vectorNode.ShapeData?.ToPath();
        member.OriginalPath.Transform(vectorNode.ShapeData.TransformationMatrix);
    }

    private void SetImageMember(Document target, MemberTransformationData member, RectD originalTightBounds,
        StructureNode layer)
    {
        ChunkyImage image =
            DrawingChangeHelper.GetTargetImageOrThrow(target, member.MemberId, drawOnMask, frame);
        VectorPath? pathToExtract = originalPath;
        RectD targetBounds = originalTightBounds;

        if (pathToExtract == null)
        {
            RectD tightBounds = layer.GetTightBounds(frame).GetValueOrDefault();
            pathToExtract = new VectorPath();
            pathToExtract.AddRect(tightBounds.RoundOutwards());
        }

        member.OriginalPath = pathToExtract;
        member.OriginalBounds = targetBounds;
        var extracted = ExtractArea(image, pathToExtract, member.RoundedOriginalBounds.Value);
        if (extracted.IsT0)
            return;

        member.AddImage(extracted.AsT1.image, extracted.AsT1.extractedRect.Pos);
    }

    [UpdateChangeMethod]
    public void Update(ShapeCorners masterCorners)
    {
        this.masterCorners = masterCorners;

        var globalMatrixWithSelection = OperationHelper.CreateMatrixFromPoints(masterCorners, originalCornersSize);
        var tightBoundsGlobalMatrix = OperationHelper.CreateMatrixFromPoints(masterCorners, tightBoundsSize);

        foreach (var member in memberData)
        {
            Matrix3X3 localMatrix = tightBoundsGlobalMatrix;

            if (member.IsImage)
            {
                localMatrix =
                    Matrix3X3.CreateTranslation(
                            (float)-cornersToSelectionOffset.TopLeft.X, (float)-cornersToSelectionOffset.TopLeft.Y)
                        .PostConcat(
                            Matrix3X3.CreateTranslation(
                                (float)member.OriginalPos.Value.X - (float)member.OriginalBounds.Value.Left,
                                (float)member.OriginalPos.Value.Y - (float)member.OriginalBounds.Value.Top));

                localMatrix = localMatrix.PostConcat(selectionAwareSize.Length > 0
                    ? globalMatrixWithSelection
                    : tightBoundsGlobalMatrix);
            }
            else if (member.OriginalMatrix is not null)
            {
                if (memberData.Count > 1)
                {
                    localMatrix = member.OriginalMatrix.Value;
                    localMatrix = localMatrix.PostConcat(Matrix3X3.CreateTranslation(
                            (float)member.OriginalPos.Value.X - (float)member.OriginalBounds.Value.Left,
                            (float)member.OriginalPos.Value.Y - (float)member.OriginalBounds.Value.Top))
                        .PostConcat(tightBoundsGlobalMatrix);
                }
                else
                {
                    localMatrix = Matrix3X3.CreateTranslation(
                        (float)-member.OriginalBounds.Value.X,
                        (float)-member.OriginalBounds.Value.Y);
                    localMatrix = localMatrix.PostConcat(tightBoundsGlobalMatrix);
                }
            }

            member.LocalMatrix = localMatrix;
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
        List<IChangeInfo> infos = new();
        
        foreach (var data in memberData)
        {
            if (data.IsImage)
            {
                ChunkyImage targetImage =
                    DrawingChangeHelper.GetTargetImageOrThrow(target, data.MemberId, drawOnMask, frame);
                targetImage.CancelChanges();
            }
            else if (data.OriginalPath != null)
            {
                var memberNode = target.FindMemberOrThrow(data.MemberId);
                if (memberNode is VectorLayerNode vectorNode)
                {
                    (vectorNode.ShapeData as PathVectorData)?.Path.Dispose();
                    vectorNode.ShapeData = data.OriginalShapeData;
                    infos.Add(new VectorShape_ChangeInfo(data.MemberId, GetTranslationAffectedArea()));
                }
            }
        }


        if (isTransformingSelection)
        {
            VectorPath? newPath = originalPath == null ? null : new VectorPath(originalPath);
            target.Selection.SelectionPath = newPath;
            infos.Add(new Selection_ChangeInfo(newPath));
        }

        hasEnqueudImages = false;
        ignoreInUndo = true;

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
            else if (member.OriginalPath != null)
            {
                VectorPath newPath = new VectorPath(member.OriginalPath);

                VecD translation = VecD.Zero;
                    //member.OriginalBounds.Value.TopLeft;
                
                var finalMatrix = member.LocalMatrix
                    .Concat(member.OriginalShapeData.TransformationMatrix.Invert())
                    .PostConcat(Matrix3X3.CreateTranslation(-(float)translation.X, -(float)translation.Y));
                    
                newPath.AddPath(member.OriginalPath, finalMatrix, AddPathMode.Append);

                var memberNode = target.FindMemberOrThrow(member.MemberId);
                if (memberNode is VectorLayerNode vectorNode)
                {
                    StrokeCap cap = vectorNode.ShapeData is PathVectorData pathData
                        ? pathData.StrokeLineCap
                        : StrokeCap.Round;
                    StrokeJoin join = vectorNode.ShapeData is PathVectorData pathData1
                        ? pathData1.StrokeLineJoin
                        : StrokeJoin.Round;
                    vectorNode.ShapeData = new PathVectorData(newPath)
                    {
                        Fill = vectorNode.ShapeData.Fill,
                        FillColor = vectorNode.ShapeData.FillColor,
                        StrokeWidth = vectorNode.ShapeData.StrokeWidth,
                        StrokeColor = vectorNode.ShapeData.StrokeColor,
                        Path = newPath,
                        StrokeLineCap = cap,
                        StrokeLineJoin = join
                    };
                }
                else
                {
                    continue;
                }

                AffectedArea translationAffectedArea = GetTranslationAffectedArea();
                var tmp = new AffectedArea(translationAffectedArea);
                if (lastAffectedArea.Chunks != null)
                {
                    translationAffectedArea.UnionWith(lastAffectedArea);
                }

                lastAffectedArea = tmp;
                infos.Add(new VectorShape_ChangeInfo(member.MemberId, translationAffectedArea));
            }
        }

        return infos;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        return new None();
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

        var final = new AffectedArea(chunks);
        return final;
    }

    private AffectedArea DrawImage(MemberTransformationData data, ChunkyImage memberImage)
    {
        var prevAffArea = memberImage.FindAffectedArea();

        memberImage.CancelChanges();

        memberImage.EnqueueDrawImage(data.LocalMatrix, data.Image, RegularPaint, false);
        hasEnqueudImages = true;

        var affectedArea = memberImage.FindAffectedArea();
        affectedArea.UnionWith(prevAffArea);
        return affectedArea;
    }
}
