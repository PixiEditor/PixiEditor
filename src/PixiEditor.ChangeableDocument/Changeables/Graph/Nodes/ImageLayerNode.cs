using System.Diagnostics;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Helpers;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("ImageLayer")]
public class ImageLayerNode : LayerNode, IReadOnlyImageNode
{
    public const string ImageFramesKey = "Frames";
    public const string ImageLayerKey = "LayerImage";

    public const int AccuratePreviewMaxSize = 2048;

    public override VecD GetScenePosition(KeyFrameTime time) => layerImage?.CommittedSize / 2f ?? VecD.Zero;
    public override VecD GetSceneSize(KeyFrameTime time) => layerImage?.CommittedSize ?? VecD.Zero;

    public bool LockTransparency { get; set; }

    private VecI startSize;
    private ColorSpace colorSpace;


    private ChunkyImage layerImage => keyFrames[0]?.Data as ChunkyImage;

    public ImageLayerNode(VecI size, ColorSpace colorSpace)
    {
        if (keyFrames.Count == 0)
        {
            keyFrames.Add(
                new KeyFrameData(Guid.NewGuid(), 0, 0, ImageLayerKey) { Data = new ChunkyImage(size, colorSpace) });
        }

        this.startSize = size;
        this.colorSpace = colorSpace;
    }

    public override RectD? GetTightBounds(KeyFrameTime frameTime)
    {
        return (RectD?)GetLayerImageAtFrame(frameTime.Frame).FindTightLatestBounds();
    }

    public override RectD? GetApproxBounds(KeyFrameTime frameTime)
    {
        var layerImage = GetLayerImageAtFrame(frameTime.Frame);
        return GetApproxBounds(layerImage);
    }

    private static RectD? GetApproxBounds(ChunkyImage layerImage)
    {
        if (layerImage.CommittedSize.LongestAxis <= AccuratePreviewMaxSize)
        {
            ChunkResolution resolution = layerImage.CommittedSize.LongestAxis switch
            {
                <= 256 => ChunkResolution.Full,
                <= 512 => ChunkResolution.Half,
                <= 1024 => ChunkResolution.Quarter,
                _ => ChunkResolution.Eighth
            };

            // Half is efficient enough to be used even for full res chunks
            bool fallbackToChunkAligned = (int)resolution > 2;

            return (RectD?)layerImage.FindTightCommittedBounds(resolution, fallbackToChunkAligned);
        }

        var chunkAlignedBounds = layerImage.FindChunkAlignedCommittedBounds();
        if (chunkAlignedBounds == null)
        {
            return null;
        }

        RectD size = new RectD(chunkAlignedBounds.Value.X, chunkAlignedBounds.Value.Y,
            Math.Min(chunkAlignedBounds.Value.Width, layerImage.LatestSize.X),
            Math.Min(chunkAlignedBounds.Value.Height, layerImage.LatestSize.Y));
        return size;
    }

    protected internal override void DrawLayerInScene(SceneObjectRenderContext ctx,
        Canvas workingSurface,
        bool useFilters = true)
    {
        int scaled = workingSurface.Save();
        float multiplier = (float)ctx.ChunkResolution.InvertedMultiplier();
        workingSurface.Translate(GetScenePosition(ctx.FrameTime));

        base.DrawLayerInScene(ctx, workingSurface, useFilters);

        workingSurface.RestoreToCount(scaled);
    }

    protected internal override void DrawLayerOnTexture(SceneObjectRenderContext ctx,
        Canvas workingSurface,
        ChunkResolution resolution,
        bool useFilters, Paint paint)
    {
        int scaled = workingSurface.Save();
        workingSurface.Translate(GetScenePosition(ctx.FrameTime) * resolution.Multiplier());
        workingSurface.Scale((float)resolution.Multiplier());

        DrawLayerOnto(ctx, workingSurface, useFilters, paint);

        workingSurface.RestoreToCount(scaled);
    }

    protected override void DrawWithoutFilters(SceneObjectRenderContext ctx, Canvas workingSurface,
        Paint paint)
    {
        DrawLayer(workingSurface, paint, ctx, false);
    }

    protected override void DrawWithFilters(SceneObjectRenderContext context, Canvas workingSurface,
        Paint paint)
    {
        DrawLayer(workingSurface, paint, context, true);
    }

    private void DrawLayer(Canvas workingSurface, Paint paint, SceneObjectRenderContext ctx, bool saveLayer)
    {
        int saved = workingSurface.Save();

        var sceneSize = GetSceneSize(ctx.FrameTime);
        if (sceneSize.X == 0 || sceneSize.Y == 0)
        {
            workingSurface.RestoreToCount(saved);
            return;
        }

        RectI latestSize = new(0, 0, layerImage.LatestSize.X, layerImage.LatestSize.Y);
        var region = ctx.VisibleDocumentRegion ?? latestSize;

        VecD topLeft = region.TopLeft - sceneSize / 2;

        topLeft *= ctx.ChunkResolution.Multiplier();
        workingSurface.Scale((float)ctx.ChunkResolution.InvertedMultiplier());
        var img = GetLayerImageAtFrame(ctx.FrameTime.Frame);

        if (saveLayer)
        {
            workingSurface.SaveLayer(paint);
        }

        if (!ctx.FullRerender)
        {
            img.DrawMostUpToDateRegionOnWithAffected(
                region,
                ctx.ChunkResolution,
                workingSurface, ctx.AffectedArea, topLeft, saveLayer ? null : paint, ctx.DesiredSamplingOptions);
        }
        else
        {
            img.DrawMostUpToDateRegionOn(
                region,
                ctx.ChunkResolution,
                workingSurface, topLeft, saveLayer ? null : paint, ctx.DesiredSamplingOptions);
        }

        workingSurface.RestoreToCount(saved);
    }

    public override RectD? GetPreviewBounds(RenderContext context, string elementFor = "")
    {
        if (IsDisposed)
        {
            return null;
        }

        if (elementFor == nameof(EmbeddedMask))
        {
            return base.GetPreviewBounds(context, elementFor);
        }

        if (Guid.TryParse(elementFor, out Guid guid))
        {
            if (guid == Id)
            {
                return new RectD(0, 0, layerImage.CommittedSize.X, layerImage.CommittedSize.Y);
            }

            var keyFrame = keyFrames.FirstOrDefault(x => x.KeyFrameGuid == guid);

            if (keyFrame != null)
            {
                var kf = GetLayerImageByKeyFrameGuid(keyFrame.KeyFrameGuid);
                if (kf == null)
                {
                    return null;
                }

                return new RectD(0, 0, kf.CommittedSize.X, kf.CommittedSize.Y);
            }
        }

        try
        {
            var kf = GetLayerImageAtFrame(context.FrameTime.Frame);
            if (kf == null)
            {
                return null;
            }

            var bounds = GetApproxBounds(kf);
            if (bounds.HasValue)
            {
                return new RectD(bounds.Value.X, bounds.Value.Y,
                    Math.Min(bounds.Value.Width, kf.CommittedSize.X),
                    Math.Min(bounds.Value.Height, kf.CommittedSize.Y));
            }

            return null;
        }
        catch (ObjectDisposedException)
        {
            return null;
        }
    }

    protected override bool ShouldRenderPreview(string elementToRenderName)
    {
        if (IsDisposed)
        {
            return false;
        }

        if (elementToRenderName == nameof(EmbeddedMask))
        {
            return base.ShouldRenderPreview(elementToRenderName);
        }

        return true;
    }

    public override void RenderPreview(DrawingSurface renderOnto, RenderContext context,
        string elementToRenderName)
    {
        if (IsDisposed)
        {
            return;
        }

        if (elementToRenderName == nameof(EmbeddedMask))
        {
            base.RenderPreview(renderOnto, context, elementToRenderName);
            return;
        }

        var img = GetLayerImageAtFrame(context.FrameTime.Frame);

        if (Guid.TryParse(elementToRenderName, out Guid guid))
        {
            var keyFrame = keyFrames.FirstOrDefault(x => x.KeyFrameGuid == guid);

            if (keyFrame != null)
            {
                img = GetLayerImageByKeyFrameGuid(keyFrame.KeyFrameGuid);
            }
            else if (guid == Id)
            {
                img = GetLayerImageAtFrame(0);
            }
        }

        if (img is null)
        {
            return;
        }

        int saved = renderOnto.Canvas.Save();
        renderOnto.Canvas.Scale((float)context.ChunkResolution.InvertedMultiplier());

        img.DrawCommittedRegionOn(
            new RectI(0, 0, img.LatestSize.X, img.LatestSize.Y),
            context.ChunkResolution,
            renderOnto.Canvas, VecI.Zero, replacePaint, context.DesiredSamplingOptions);

        renderOnto.Canvas.RestoreToCount(saved);
    }

    private KeyFrameData GetFrameWithImage(KeyFrameTime frame)
    {
        if (keyFrames.Count == 1)
        {
            return keyFrames[0];
        }

        var imageFrame = keyFrames.OrderBy(x => x.StartFrame).LastOrDefault(x => x.IsInFrame(frame.Frame));
        if (imageFrame?.Data is not ChunkyImage)
        {
            return keyFrames[0];
        }

        var frameImage = imageFrame;
        return frameImage;
    }

    protected override bool CacheChanged(RenderContext context)
    {
        var frame = GetFrameWithImage(context.FrameTime);

        return base.CacheChanged(context) || frame?.RequiresUpdate == true;
    }

    protected override void UpdateCache(RenderContext context)
    {
        base.UpdateCache(context);
        var imageFrame = GetFrameWithImage(context.FrameTime);
        if (imageFrame is not null && imageFrame.RequiresUpdate)
        {
            imageFrame.RequiresUpdate = false;
        }
    }

    public override Node CreateCopy()
    {
        var image = new ImageLayerNode(startSize, colorSpace)
        {
            MemberName = this.MemberName,
            LockTransparency = this.LockTransparency,
            ClipToPreviousMember = this.ClipToPreviousMember,
            EmbeddedMask = this.EmbeddedMask?.CloneFromCommitted()
        };

        image.keyFrames.Clear();

        return image;
    }

    IReadOnlyChunkyImage IReadOnlyImageNode.GetLayerImageAtFrame(int frame) => GetLayerImageAtFrame(frame);

    IReadOnlyChunkyImage IReadOnlyImageNode.GetLayerImageByKeyFrameGuid(Guid keyFrameGuid) =>
        GetLayerImageByKeyFrameGuid(keyFrameGuid);

    void IReadOnlyImageNode.SetLayerImageAtFrame(int frame, IReadOnlyChunkyImage newLayerImage) =>
        SetLayerImageAtFrame(frame, (ChunkyImage)newLayerImage);

    void IReadOnlyImageNode.ForEveryFrame(Action<IReadOnlyChunkyImage> action) => ForEveryFrame(action);

    public void ForEveryFrame(Action<ChunkyImage> action)
    {
        foreach (var frame in keyFrames)
        {
            if (frame.Data is ChunkyImage imageFrame)
            {
                action(imageFrame);
            }
        }
    }

    public void ForEveryFrame(Action<ChunkyImage, Guid> action)
    {
        foreach (var frame in keyFrames)
        {
            if (frame.Data is ChunkyImage imageFrame)
            {
                action(imageFrame, frame.KeyFrameGuid);
            }
        }
    }

    public ChunkyImage GetLayerImageAtFrame(int frame)
    {
        return GetFrameWithImage(frame).Data as ChunkyImage;
    }

    public ChunkyImage GetLayerImageByKeyFrameGuid(Guid keyFrameGuid)
    {
        foreach (var keyFrame in keyFrames)
        {
            if (keyFrame.KeyFrameGuid == keyFrameGuid)
            {
                return keyFrame.Data as ChunkyImage;
            }
        }

        return layerImage;
    }

    public void SetLayerImageAtFrame(int frame, ChunkyImage newLayerImage)
    {
        var existingFrame = keyFrames.FirstOrDefault(x => x.IsInFrame(frame));
        if (existingFrame is not null && existingFrame.Data is ChunkyImage)
        {
            existingFrame.Dispose();
            existingFrame.Data = newLayerImage;
        }
    }
}
