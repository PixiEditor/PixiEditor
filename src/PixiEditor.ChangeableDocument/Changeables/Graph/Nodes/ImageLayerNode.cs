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
    public OutputProperty<Texture> RawOutput { get; }

    public override VecD ScenePosition => layerImage.CommittedSize / 2f;
    public override VecD SceneSize => layerImage.CommittedSize;
    
    public bool LockTransparency { get; set; }

    private VecI startSize;
    private ChunkyImage layerImage => keyFrames[0]?.Data as ChunkyImage;

    private static readonly Paint clearPaint = new()
    {
        BlendMode = DrawingApi.Core.Surfaces.BlendMode.Src,
        Color = PixiEditor.DrawingApi.Core.ColorsImpl.Colors.Transparent
    };

    // Handled by overriden CacheChanged
    protected override bool AffectedByAnimation => true;

    protected override bool AffectedByChunkResolution => true;


    private Dictionary<ChunkResolution, Texture> renderedSurfaces = new();

    public ImageLayerNode(VecI size)
    {
        RawOutput = CreateOutput<Texture>(nameof(RawOutput), "RAW_LAYER_OUTPUT", null);

        if (keyFrames.Count == 0)
        {
            keyFrames.Add(new KeyFrameData(Guid.NewGuid(), 0, 0, ImageLayerKey) { Data = new ChunkyImage(size) });
        }

        this.startSize = size;

        CreateRenderCanvases(size, renderedSurfaces);
    }
    
    public override RectD? GetTightBounds(KeyFrameTime frameTime)
    {
        return (RectD?)GetLayerImageAtFrame(frameTime.Frame).FindTightCommittedBounds();
    }

    protected override void OnExecute(RenderContext context)
    {
        base.OnExecute(context);

        /*if (RawOutput.Connections.Count > 0)
        {
            var rawWorkingSurface = TryInitWorkingSurface(GetTargetSize(context), context.ChunkResolution, 2);
            DrawLayer(context, rawWorkingSurface, true, useFilters: false);

            RawOutput.Value = rawWorkingSurface;
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
        VecD shiftToCenter = SceneSize - renderedSurfaces[ctx.ChunkResolution].Size;
        workingSurface.Canvas.Translate(ScenePosition);
        workingSurface.Canvas.Scale(multiplier, multiplier);
        workingSurface.Canvas.Translate(shiftToCenter / 2f);
        base.DrawLayerInScene(ctx, workingSurface, useFilters);

        workingSurface.Canvas.RestoreToCount(scaled);
    }

    protected override void DrawWithoutFilters(SceneObjectRenderContext ctx, DrawingSurface workingSurface,
        Paint paint)
    {
        DrawLayer(workingSurface, paint, ctx.ChunkResolution); 
    }

    // Draw with filters is a bit tricky since some filters sample data from chunks surrounding the chunk being drawn,
    // this is why we need to do intermediate drawing to a temporary surface and then apply filters to that surface
    protected override void DrawWithFilters(SceneObjectRenderContext context, DrawingSurface workingSurface,
        Paint paint)
    {
        DrawLayer(workingSurface, paint, context.ChunkResolution);
    }

    private void DrawLayer(DrawingSurface workingSurface, Paint paint, ChunkResolution resolution)
    {
        VecD topLeft = SceneSize / 2f;
        workingSurface.Canvas.DrawSurface(renderedSurfaces[resolution].DrawingSurface, -(VecI)topLeft, paint);
    }

    public override bool RenderPreview(Texture renderOn, VecI chunk, ChunkResolution resolution, int frame)
    {
        var img = GetLayerImageAtFrame(frame);

        if (img is null)
        {
            return false;
        }

        img.DrawMostUpToDateChunkOn(
            chunk,
            resolution,
            renderOn.DrawingSurface,
            chunk * resolution.PixelSize(),
            blendPaint);

        return true;
    }

    private void DrawChunk(ChunkyImage frameImage, RenderContext context, Texture tempSurface, VecI vecI,
        Paint paint)
    {
        /*VecI chunkPos = context.ChunkToUpdate.Value + vecI;
        if (frameImage.LatestOrCommittedChunkExists(chunkPos))
        {
            frameImage.DrawMostUpToDateChunkOn(
                chunkPos,
                context.ChunkResolution,
                tempSurface.DrawingSurface,
                chunkPos * context.ChunkResolution.PixelSize(),
                paint);
        }*/
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

        RenderChunkyImageChunk(chunkPos, resolution, img, renderedSurfaces);
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
