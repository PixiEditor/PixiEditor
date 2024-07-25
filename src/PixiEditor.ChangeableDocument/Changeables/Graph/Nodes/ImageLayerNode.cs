using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Helpers;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("ImageLayer")]
public class ImageLayerNode : LayerNode, IReadOnlyImageNode
{
    public const string ImageFramesKey = "Frames";
    public const string ImageLayerKey = "LayerImage";

    public InputProperty<bool> LockTransparency { get; }

    private VecI size;
    private ChunkyImage layerImage => keyFrames[0]?.Data as ChunkyImage;

    private static readonly Paint clearPaint = new()
    {
        BlendMode = DrawingApi.Core.Surfaces.BlendMode.Src,
        Color = PixiEditor.DrawingApi.Core.ColorsImpl.Colors.Transparent
    };

    // Handled by overriden CacheChanged
    protected override bool AffectedByAnimation => true;

    protected override bool AffectedByChunkResolution => true;

    protected override bool AffectedByChunkToUpdate => true;

    public ImageLayerNode(VecI size)
    {
        LockTransparency = CreateInput<bool>("LockTransparency", "LOCK_TRANSPARENCY", false);

        if (keyFrames.Count == 0)
        {
            keyFrames.Add(new KeyFrameData(Guid.NewGuid(), 0, 0, ImageLayerKey) { Data = new ChunkyImage(size) });
        }

        this.size = size;
    }

    public override RectI? GetTightBounds(KeyFrameTime frameTime)
    {
        return GetLayerImageAtFrame(frameTime.Frame).FindTightCommittedBounds();
    }

    protected override Surface? OnExecute(RenderingContext context)
    {
        if (!IsVisible.Value || Opacity.Value <= 0 || IsEmptyMask())
        {
            Output.Value = Background.Value;
            return Output.Value;
        }

        var frameImage = GetFrameWithImage(context.FrameTime);

        blendPaint.Color = new Color(255, 255, 255, 255);
        blendPaint.BlendMode = DrawingApi.Core.Surfaces.BlendMode.Src;

        var renderedSurface = RenderImage(frameImage.Data as ChunkyImage, context);

        Output.Value = renderedSurface;

        return Output.Value;
    }

    private Surface RenderImage(ChunkyImage frameImage, RenderingContext context)
    {
        var workingSurface = TryInitWorkingSurface(frameImage.LatestSize, context);

        if (!HasOperations())
        {
            bool canClear = Background.Value == null;
            if (Background.Value != null)
            {
                DrawBackground(workingSurface, context);
                blendPaint.BlendMode = RenderingContext.GetDrawingBlendMode(BlendMode.Value);
            }

            DrawLayer(frameImage, context, workingSurface, canClear);
            Output.Value = workingSurface;
            return workingSurface;
        }

        DrawLayer(frameImage, context, workingSurface, true);

        // shit gets downhill with mask on big canvases, TODO: optimize
        ApplyMaskIfPresent(workingSurface, context);
        ApplyRasterClip(workingSurface, context);

        if (Background.Value != null)
        {
            Surface tempSurface = new Surface(workingSurface.Size);
            DrawBackground(tempSurface, context);
            blendPaint.BlendMode = RenderingContext.GetDrawingBlendMode(BlendMode.Value);
            tempSurface.DrawingSurface.Canvas.DrawSurface(workingSurface.DrawingSurface, 0, 0, blendPaint);

            Output.Value = tempSurface;
            return tempSurface;
        }

        Output.Value = workingSurface;
        return workingSurface;
    }

    private void DrawLayer(ChunkyImage frameImage, RenderingContext context, Surface workingSurface, bool shouldClear)
    {
        blendPaint.Color = blendPaint.Color.WithAlpha((byte)Math.Round(Opacity.Value * 255)); 
        
        blendPaint.SetFilters(Filters.Value);
        
        if (!frameImage.DrawMostUpToDateChunkOn(
                context.ChunkToUpdate,
                context.ChunkResolution,
                workingSurface.DrawingSurface,
                context.ChunkToUpdate * context.ChunkResolution.PixelSize(),
                blendPaint) && shouldClear)
        {
            workingSurface.DrawingSurface.Canvas.DrawRect(CalculateDestinationRect(context), clearPaint);
        }
    }

    private KeyFrameData GetFrameWithImage(KeyFrameTime frame)
    {
        var imageFrame = keyFrames.LastOrDefault(x => x.IsInFrame(frame.Frame));
        if (imageFrame?.Data is not ChunkyImage)
        {
            return keyFrames[0];
        }

        var frameImage = imageFrame;
        return frameImage;
    }

    protected override bool CacheChanged(RenderingContext context)
    {
        var frame = GetFrameWithImage(context.FrameTime);
        return base.CacheChanged(context) || frame?.RequiresUpdate == true;
    }

    protected override void UpdateCache(RenderingContext context)
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
        return new ImageLayerNode(size)
        {
            MemberName = MemberName,
            keyFrames = new List<KeyFrameData>()
            {
                // we are only copying the layer image, keyframes probably shouldn't be copied since they are controlled by AnimationData
                new KeyFrameData(Guid.NewGuid(), 0, 0, ImageLayerKey) { Data = layerImage.CloneFromCommitted() }
            }
        };
    }


    IReadOnlyChunkyImage IReadOnlyImageNode.GetLayerImageAtFrame(int frame) => GetLayerImageAtFrame(frame);

    IReadOnlyChunkyImage IReadOnlyImageNode.GetLayerImageByKeyFrameGuid(Guid keyFrameGuid) =>
        GetLayerImageByKeyFrameGuid(keyFrameGuid);

    void IReadOnlyImageNode.SetLayerImageAtFrame(int frame, IReadOnlyChunkyImage newLayerImage) =>
        SetLayerImageAtFrame(frame, (ChunkyImage)newLayerImage);

    void IReadOnlyImageNode.ForEveryFrame(Action<IReadOnlyChunkyImage> action) => ForEveryFrame(action);

    bool ITransparencyLockable.LockTransparency
    {
        get => LockTransparency.Value; // TODO: I wonder if it should be NonOverridenValue
        set => LockTransparency.NonOverridenValue = value;
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
