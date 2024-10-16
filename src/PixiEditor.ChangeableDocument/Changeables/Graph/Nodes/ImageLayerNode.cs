using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Helpers;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("ImageLayer")]
public class ImageLayerNode : LayerNode, IReadOnlyImageNode
{
    public const string ImageFramesKey = "Frames";
    public const string ImageLayerKey = "LayerImage";
    //public RenderOutputProperty RawOutput { get; }

    public override VecD ScenePosition => layerImage.CommittedSize / 2f;
    public override VecD SceneSize => layerImage.CommittedSize;

    public bool LockTransparency { get; set; }

    private VecI startSize;
    private ChunkyImage layerImage => keyFrames[0]?.Data as ChunkyImage;

    private Texture fullResrenderedSurface;
    private int renderedSurfaceFrame = -1;

    public ImageLayerNode(VecI size)
    {
        //RawOutput = CreateRenderOutput(nameof(RawOutput), "RAW_LAYER_OUTPUT");

        if (keyFrames.Count == 0)
        {
            keyFrames.Add(new KeyFrameData(Guid.NewGuid(), 0, 0, ImageLayerKey) { Data = new ChunkyImage(size) });
        }

        this.startSize = size;
    }

    public override RectD? GetTightBounds(KeyFrameTime frameTime)
    {
        return (RectD?)GetLayerImageAtFrame(frameTime.Frame).FindTightCommittedBounds();
    }

    protected override void OnExecute(RenderContext context)
    {
        base.OnExecute(context);
        /*var rawOutputTarget = RawOutput.GetFirstRenderTarget(context);
        if (RawOutput.Connections.Count > 0 && rawOutputTarget != null)
        {
            using SceneObjectRenderContext sceneContext = CreateSceneContext(context, rawOutputTarget, RawOutput);
            
            int renderSaved = rawOutputTarget.Canvas.Save();
            rawOutputTarget.Canvas.ClipRect(new RectD(ScenePosition - (SceneSize / 2f), SceneSize));
            
            DrawLayerInScene(sceneContext, rawOutputTarget, false);
            
            rawOutputTarget.Canvas.RestoreToCount(renderSaved);
            RawOutput.Value = rawOutputTarget;
        }*/
    }
    
    protected override VecI GetTargetSize(RenderContext ctx)
    {
        return (GetFrameWithImage(ctx.FrameTime).Data as ChunkyImage).LatestSize;
    }

    protected internal override void DrawLayerInScene(SceneObjectRenderContext ctx, DrawingSurface workingSurface,
        bool useFilters = true)
    {
        int scaled = workingSurface.Canvas.Save();
        float multiplier = (float)ctx.ChunkResolution.InvertedMultiplier();
        workingSurface.Canvas.Translate(ScenePosition);
        base.DrawLayerInScene(ctx, workingSurface, useFilters);

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

        VecD topLeft = SceneSize / 2f;
        if (renderedSurfaceFrame == null || ctx.FullRerender || ctx.FrameTime.Frame != renderedSurfaceFrame)
        {
            GetLayerImageAtFrame(ctx.FrameTime.Frame).DrawMostUpToDateRegionOn(
                new RectI(0, 0, layerImage.LatestSize.X, layerImage.LatestSize.Y),
                ChunkResolution.Full,
                workingSurface, -(VecI)topLeft, paint);
        }
        else
        {
            workingSurface.Canvas.DrawSurface(fullResrenderedSurface.DrawingSurface, -(VecI)topLeft, paint);
        }

        workingSurface.Canvas.RestoreToCount(saved);
    }

    public override RectD? GetPreviewBounds(int frame, string elementFor = "")
    {
        if (elementFor == nameof(EmbeddedMask))
        {
            return base.GetPreviewBounds(frame, elementFor);
        }

        if (Guid.TryParse(elementFor, out Guid guid))
        {
            var keyFrame = keyFrames.FirstOrDefault(x => x.KeyFrameGuid == guid);

            if (keyFrame != null)
            {
                return (RectD?)GetLayerImageByKeyFrameGuid(keyFrame.KeyFrameGuid).FindTightCommittedBounds();
            }
        }

        return (RectD?)GetLayerImageAtFrame(frame).FindTightCommittedBounds();
    }

    public override bool RenderPreview(DrawingSurface renderOnto, ChunkResolution resolution, int frame,
        string elementToRenderName)
    {
        if (elementToRenderName == nameof(EmbeddedMask))
        {
            return base.RenderPreview(renderOnto, resolution, frame, elementToRenderName);
        }

        var img = GetLayerImageAtFrame(frame);

        if (Guid.TryParse(elementToRenderName, out Guid guid))
        {
            var keyFrame = keyFrames.FirstOrDefault(x => x.KeyFrameGuid == guid);

            if (keyFrame != null)
            {
                img = GetLayerImageByKeyFrameGuid(keyFrame.KeyFrameGuid);
            }
        }

        if (img is null)
        {
            return false;
        }

        if (renderedSurfaceFrame == frame)
        {
            renderOnto.Canvas.DrawSurface(fullResrenderedSurface.DrawingSurface, VecI.Zero, blendPaint);
        }
        else
        {
            img.DrawMostUpToDateRegionOn(
                new RectI(0, 0, img.LatestSize.X, img.LatestSize.Y),
                resolution,
                renderOnto, VecI.Zero, blendPaint);
        }

        return true;
    }

    private KeyFrameData GetFrameWithImage(KeyFrameTime frame)
    {
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
        var image = new ImageLayerNode(startSize) { MemberName = this.MemberName, };

        image.keyFrames.Clear();

        return image;
    }

    public override void Dispose()
    {
        base.Dispose();

        foreach (var workingSurface in workingSurfaces)
        {
            workingSurface.Value.Dispose();
        }
    }

    IReadOnlyChunkyImage IReadOnlyImageNode.GetLayerImageAtFrame(int frame) => GetLayerImageAtFrame(frame);

    IReadOnlyChunkyImage IReadOnlyImageNode.GetLayerImageByKeyFrameGuid(Guid keyFrameGuid) =>
        GetLayerImageByKeyFrameGuid(keyFrameGuid);

    void IReadOnlyImageNode.SetLayerImageAtFrame(int frame, IReadOnlyChunkyImage newLayerImage) =>
        SetLayerImageAtFrame(frame, (ChunkyImage)newLayerImage);

    void IReadOnlyImageNode.ForEveryFrame(Action<IReadOnlyChunkyImage> action) => ForEveryFrame(action);

    public override void RenderChunk(VecI chunkPos, ChunkResolution resolution, KeyFrameTime frameTime)
    {
        base.RenderChunk(chunkPos, resolution, frameTime);

        var img = GetLayerImageAtFrame(frameTime.Frame);

        RenderChunkyImageChunk(chunkPos, resolution, img, ref fullResrenderedSurface);
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
