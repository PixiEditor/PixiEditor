using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Helpers;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
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
        bool useFilters)
    {
        int scaled = workingSurface.Canvas.Save();
        workingSurface.Canvas.Translate(GetScenePosition(ctx.FrameTime) * ctx.ChunkResolution.Multiplier());
        workingSurface.Canvas.Scale((float)ctx.ChunkResolution.Multiplier());

        DrawLayerOnto(ctx, workingSurface, useFilters);

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
        if (renderedSurfaceFrame == null || ctx.FullRerender || ctx.FrameTime.Frame != renderedSurfaceFrame)
        {
            GetLayerImageAtFrame(ctx.FrameTime.Frame).DrawMostUpToDateRegionOn(
                new RectI(0, 0, layerImage.LatestSize.X, layerImage.LatestSize.Y),
                ChunkResolution.Full,
                workingSurface, -topLeft, paint);
        }
        else
        {
            workingSurface.Canvas.DrawSurface(fullResrenderedSurface.DrawingSurface, -topLeft, paint);
        }

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

                RectI? bounds = kf.FindChunkAlignedCommittedBounds(); // Don't use tight bounds, very expensive
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

            var bounds = kf.FindChunkAlignedCommittedBounds(); // Don't use tight bounds, very expensive
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
            renderOnto.Canvas.DrawSurface(fullResrenderedSurface.DrawingSurface, VecI.Zero, blendPaint);
        }
        else
        {
            img.DrawMostUpToDateRegionOn(
                new RectI(0, 0, img.LatestSize.X, img.LatestSize.Y),
                context.ChunkResolution,
                renderOnto, VecI.Zero, blendPaint);
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
