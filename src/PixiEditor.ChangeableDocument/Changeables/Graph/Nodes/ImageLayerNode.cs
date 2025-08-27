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

    public override VecD GetScenePosition(KeyFrameTime time) => layerImage.CommittedSize / 2f;
    public override VecD GetSceneSize(KeyFrameTime time) => layerImage.CommittedSize;

    public bool LockTransparency { get; set; }

    private VecI startSize;
    private ColorSpace colorSpace;
    private ChunkyImage layerImage => keyFrames[0]?.Data as ChunkyImage;

    private Texture fullResrenderedSurface;
    private int renderedSurfaceFrame = -1;

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
        return (RectD?)GetLayerImageAtFrame(frameTime.Frame).FindTightCommittedBounds();
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
        DrawingSurface workingSurface,
        bool useFilters = true)
    {
        int scaled = workingSurface.Canvas.Save();
        float multiplier = (float)ctx.ChunkResolution.InvertedMultiplier();
        workingSurface.Canvas.Translate(GetScenePosition(ctx.FrameTime));

        base.DrawLayerInScene(ctx, workingSurface, useFilters);

        workingSurface.Canvas.RestoreToCount(scaled);
    }

    protected internal override void DrawLayerOnTexture(SceneObjectRenderContext ctx,
        DrawingSurface workingSurface,
        ChunkResolution resolution,
        bool useFilters, Paint paint)
    {
        int scaled = workingSurface.Canvas.Save();
        workingSurface.Canvas.Translate(GetScenePosition(ctx.FrameTime) * resolution.Multiplier());
        workingSurface.Canvas.Scale((float)resolution.Multiplier());

        DrawLayerOnto(ctx, workingSurface, useFilters, paint);

        workingSurface.Canvas.RestoreToCount(scaled);
    }

    protected override void DrawWithoutFilters(SceneObjectRenderContext ctx, DrawingSurface workingSurface,
        Paint paint)
    {
        DrawLayer(workingSurface, paint, ctx);
    }

    protected override void DrawWithFilters(SceneObjectRenderContext context, DrawingSurface workingSurface,
        Paint paint)
    {
        DrawLayer(workingSurface, paint, context);
    }

    private void DrawLayer(DrawingSurface workingSurface, Paint paint, SceneObjectRenderContext ctx)
    {
        int saved = workingSurface.Canvas.Save();

        var sceneSize = GetSceneSize(ctx.FrameTime);
        VecD topLeft = sceneSize / 2f;

        //if (renderedSurfaceFrame == null || ctx.FullRerender || ctx.FrameTime.Frame != renderedSurfaceFrame)
        {
            topLeft *= ctx.ChunkResolution.Multiplier();
            workingSurface.Canvas.Scale((float)ctx.ChunkResolution.InvertedMultiplier());
            if (ctx.AffectedArea.Chunks.Count > 0)
            {
                GetLayerImageAtFrame(ctx.FrameTime.Frame).DrawMostUpToDateRegionOnWithAffected(
                    new RectI(0, 0, layerImage.LatestSize.X, layerImage.LatestSize.Y),
                    ctx.ChunkResolution,
                    workingSurface, ctx.AffectedArea, -topLeft, paint);
            }
            else
            {
                GetLayerImageAtFrame(ctx.FrameTime.Frame).DrawMostUpToDateRegionOn(
                    new RectI(0, 0, layerImage.LatestSize.X, layerImage.LatestSize.Y),
                    ctx.ChunkResolution,
                    workingSurface, -topLeft, paint);
            }
        }
        /*else
        {
            if (ctx.DesiredSamplingOptions == SamplingOptions.Default)
            {
                workingSurface.Canvas.DrawSurface(
                    fullResrenderedSurface.DrawingSurface, -(float)topLeft.X, -(float)topLeft.Y, paint);
            }
            else
            {
                using var snapshot = fullResrenderedSurface.DrawingSurface.Snapshot();
                workingSurface.Canvas.DrawImage(snapshot, -(float)topLeft.X, -(float)topLeft.Y,
                    ctx.DesiredSamplingOptions,
                    paint);
            }
        }*/

        workingSurface.Canvas.RestoreToCount(saved);
    }

    public override RectD? GetPreviewBounds(int frame, string elementFor = "")
    {
        if (IsDisposed)
        {
            return null;
        }

        if (elementFor == nameof(EmbeddedMask))
        {
            return base.GetPreviewBounds(frame, elementFor);
        }

        if (Guid.TryParse(elementFor, out Guid guid))
        {
            var keyFrame = keyFrames.FirstOrDefault(x => x.KeyFrameGuid == guid);

            if (keyFrame != null)
            {
                var kf = GetLayerImageByKeyFrameGuid(keyFrame.KeyFrameGuid);
                if (kf == null)
                {
                    return null;
                }

                RectI? bounds = (RectI?)GetApproxBounds(kf);
                if (bounds.HasValue)
                {
                    return new RectD(bounds.Value.X, bounds.Value.Y,
                        Math.Min(bounds.Value.Width, kf.CommittedSize.X),
                        Math.Min(bounds.Value.Height, kf.CommittedSize.Y));
                }
            }
        }

        try
        {
            var kf = GetLayerImageAtFrame(frame);
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

    public override bool RenderPreview(DrawingSurface renderOnto, RenderContext context,
        string elementToRenderName)
    {
        if (IsDisposed)
        {
            return false;
        }

        if (elementToRenderName == nameof(EmbeddedMask))
        {
            return base.RenderPreview(renderOnto, context, elementToRenderName);
        }

        var img = GetLayerImageAtFrame(context.FrameTime.Frame);

        int cacheFrame = context.FrameTime.Frame;
        if (Guid.TryParse(elementToRenderName, out Guid guid))
        {
            var keyFrame = keyFrames.FirstOrDefault(x => x.KeyFrameGuid == guid);

            if (keyFrame != null)
            {
                img = GetLayerImageByKeyFrameGuid(keyFrame.KeyFrameGuid);
                cacheFrame = keyFrame.StartFrame;
            }
            else if (guid == Id)
            {
                img = GetLayerImageAtFrame(0);
                cacheFrame = 0;
            }
        }

        if (img is null)
        {
            return false;
        }

        if (renderedSurfaceFrame == cacheFrame)
        {
            int saved = renderOnto.Canvas.Save();
            renderOnto.Canvas.Scale((float)context.ChunkResolution.Multiplier());
            if (context.DesiredSamplingOptions == SamplingOptions.Default)
            {
                renderOnto.Canvas.DrawSurface(
                    fullResrenderedSurface.DrawingSurface, 0, 0, blendPaint);
            }
            else
            {
                using var snapshot = fullResrenderedSurface.DrawingSurface.Snapshot();
                renderOnto.Canvas.DrawImage(snapshot, 0, 0, context.DesiredSamplingOptions, blendPaint);
            }

            renderOnto.Canvas.RestoreToCount(saved);
        }
        else
        {
            img.DrawMostUpToDateRegionOn(
                new RectI(0, 0, img.LatestSize.X, img.LatestSize.Y),
                context.ChunkResolution,
                renderOnto, VecI.Zero, blendPaint, context.DesiredSamplingOptions);
        }

        return true;
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

    public override void Dispose()
    {
        base.Dispose();
        fullResrenderedSurface?.Dispose();
    }

    IReadOnlyChunkyImage IReadOnlyImageNode.GetLayerImageAtFrame(int frame) => GetLayerImageAtFrame(frame);

    IReadOnlyChunkyImage IReadOnlyImageNode.GetLayerImageByKeyFrameGuid(Guid keyFrameGuid) =>
        GetLayerImageByKeyFrameGuid(keyFrameGuid);

    void IReadOnlyImageNode.SetLayerImageAtFrame(int frame, IReadOnlyChunkyImage newLayerImage) =>
        SetLayerImageAtFrame(frame, (ChunkyImage)newLayerImage);

    void IReadOnlyImageNode.ForEveryFrame(Action<IReadOnlyChunkyImage> action) => ForEveryFrame(action);

    public override void RenderChunk(VecI chunkPos, ChunkResolution resolution, KeyFrameTime frameTime,
        ColorSpace processColorSpace)
    {
        return;
        base.RenderChunk(chunkPos, resolution, frameTime, processColorSpace);

        var img = GetLayerImageAtFrame(frameTime.Frame);

        RenderChunkyImageChunk(chunkPos, resolution, img, 85, processColorSpace, ref fullResrenderedSurface);
        renderedSurfaceFrame = frameTime.Frame;
    }

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
