using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.Properties;
using PixiEditor.ChangeableDocument.Helpers;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;
using BlendMode = PixiEditor.ChangeableDocument.Enums.BlendMode;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public abstract class StructureNode : RenderNode, IReadOnlyStructureNode, IRenderInput
{
    public const string DefaultMemberName = "DEFAULT_MEMBER_NAME";
    public InputProperty<float> Opacity { get; }
    public InputProperty<bool> IsVisible { get; }
    public bool ClipToPreviousMember { get; set; }
    public InputProperty<BlendMode> BlendMode { get; }
    public RenderInputProperty CustomMask { get; }
    public InputProperty<bool> MaskIsVisible { get; }
    public InputProperty<Filter> Filters { get; }

    public RenderInputProperty Background { get; }
    public RenderOutputProperty FilterlessOutput { get; }
    public RenderOutputProperty RawOutput { get; }

    public ChunkyImage? EmbeddedMask { get; set; }

    protected Texture renderedMask;
    protected static readonly Paint replacePaint = new Paint() { BlendMode = DrawingApi.Core.Surfaces.BlendMode.Src };

    protected static readonly Paint clearPaint = new Paint()
    {
        BlendMode = DrawingApi.Core.Surfaces.BlendMode.Src, Color = Colors.Transparent
    };

    public virtual ShapeCorners GetTransformationCorners(KeyFrameTime frameTime)
    {
        return new ShapeCorners(GetTightBounds(frameTime).GetValueOrDefault());
    }

    public string MemberName
    {
        get => DisplayName;
        set => DisplayName = value;
    }

    protected Paint maskPaint = new Paint() { BlendMode = DrawingApi.Core.Surfaces.BlendMode.DstIn };
    protected Paint blendPaint = new Paint() { BlendMode = DrawingApi.Core.Surfaces.BlendMode.SrcOver };

    protected Paint maskPreviewPaint = new Paint()
    {
        BlendMode = DrawingApi.Core.Surfaces.BlendMode.SrcOver, ColorFilter = Nodes.Filters.AlphaGrayscaleFilter
    };

    private int maskCacheHash = 0;

    protected StructureNode()
    {
        Painter filterlessPainter = new Painter(OnFilterlessPaint);
        Painter rawPainter = new Painter(OnRawPaint);
        Background = CreateRenderInput("Background", "BACKGROUND");
        Opacity = CreateInput<float>("Opacity", "OPACITY", 1);
        IsVisible = CreateInput<bool>("IsVisible", "IS_VISIBLE", true);
        BlendMode = CreateInput("BlendMode", "BLEND_MODE", Enums.BlendMode.Normal);
        CustomMask = CreateRenderInput("Mask", "MASK");
        MaskIsVisible = CreateInput<bool>("MaskIsVisible", "MASK_IS_VISIBLE", true);
        Filters = CreateInput<Filter>(nameof(Filters), "FILTERS", null);

        FilterlessOutput = CreateRenderOutput(nameof(FilterlessOutput), "WITHOUT_FILTERS",
            () => filterlessPainter, () => Background.Value);

        RawOutput = CreateRenderOutput(nameof(RawOutput), "RAW_LAYER_OUTPUT", () => rawPainter);

        MemberName = DefaultMemberName;
    }

    protected override void OnPaint(RenderContext context, DrawingSurface renderTarget)
    {
        if (Output.Connections.Count > 0)
        {
            RenderForOutput(context, renderTarget, Output);
        }
    }

    private void OnFilterlessPaint(RenderContext context, DrawingSurface renderTarget)
    {
        RenderForOutput(context, renderTarget, FilterlessOutput);
    }
    
    private void OnRawPaint(RenderContext context, DrawingSurface renderTarget)
    {
        RenderForOutput(context, renderTarget, RawOutput);
    }
    
    public abstract VecD GetScenePosition(KeyFrameTime frameTime);
    public abstract VecD GetSceneSize(KeyFrameTime frameTime);

    private void RenderForOutput(RenderContext context, DrawingSurface renderTarget, RenderOutputProperty output)
    {
        var renderObjectContext = CreateSceneContext(context, renderTarget, output);

        int renderSaved = renderTarget.Canvas.Save();
        VecD scenePos = GetScenePosition(context.FrameTime);
        VecD sceneSize = GetSceneSize(context.FrameTime);
        renderTarget.Canvas.ClipRect(new RectD(scenePos - (sceneSize / 2f), sceneSize));

        Render(renderObjectContext);

        renderTarget?.Canvas.RestoreToCount(renderSaved);
    }

    protected SceneObjectRenderContext CreateSceneContext(RenderContext context, DrawingSurface renderTarget,
        RenderOutputProperty output)
    {
        var sceneSize = GetSceneSize(context.FrameTime);
        RectD localBounds = new RectD(0, 0, sceneSize.X, sceneSize.Y);

        SceneObjectRenderContext renderObjectContext = new SceneObjectRenderContext(output, renderTarget, localBounds,
            context.FrameTime, context.ChunkResolution, context.DocumentSize, renderTarget == context.RenderSurface,
            context.Opacity);
        renderObjectContext.FullRerender = context.FullRerender;
        return renderObjectContext;
    }

    public abstract void Render(SceneObjectRenderContext sceneContext);

    protected void ApplyMaskIfPresent(DrawingSurface surface, RenderContext context)
    {
        if (MaskIsVisible.Value)
        {
            if (CustomMask.Value != null)
            {
                int layer = surface.Canvas.SaveLayer(maskPaint);
                CustomMask.Value.Paint(context, surface);
                
                surface.Canvas.RestoreToCount(layer);
            }
            else if (EmbeddedMask != null)
            {
                if (context.FullRerender)
                {
                    EmbeddedMask.DrawMostUpToDateRegionOn(
                        new RectI(0, 0, EmbeddedMask.LatestSize.X, EmbeddedMask.LatestSize.Y),
                        ChunkResolution.Full,
                        surface, VecI.Zero, maskPaint);
                }
                else
                {
                    surface.Canvas.DrawSurface(renderedMask.DrawingSurface, 0, 0, maskPaint);
                }
            }
        }
    }

    protected override bool CacheChanged(RenderContext context)
    {
        int cacheHash = EmbeddedMask?.GetCacheHash() ?? 0;
        return base.CacheChanged(context) || maskCacheHash != cacheHash;
    }

    protected override void UpdateCache(RenderContext context)
    {
        base.UpdateCache(context);
        maskCacheHash = EmbeddedMask?.GetCacheHash() ?? 0;
    }

    public virtual void RenderChunk(VecI chunkPos, ChunkResolution resolution, KeyFrameTime frameTime)
    {
        RenderChunkyImageChunk(chunkPos, resolution, EmbeddedMask, 55, ref renderedMask);
    }

    protected void RenderChunkyImageChunk(VecI chunkPos, ChunkResolution resolution, ChunkyImage img,
        int textureId,
        ref Texture? renderSurface)
    {
        if (img is null)
        {
            return;
        }

        VecI targetSize = img.LatestSize;
        
        renderSurface = RequestTexture(textureId, targetSize, false);

        int saved = renderSurface.DrawingSurface.Canvas.Save();
        
        if (!img.DrawMostUpToDateChunkOn(
                chunkPos,
                ChunkResolution.Full,
                renderSurface.DrawingSurface,
                chunkPos * ChunkResolution.Full.PixelSize(),
                replacePaint))
        {
            var chunkSize = ChunkResolution.Full.PixelSize();
            renderSurface.DrawingSurface.Canvas.DrawRect(new RectD(chunkPos * chunkSize, new VecD(chunkSize)),
                clearPaint);
        }
        
        renderSurface.DrawingSurface.Canvas.RestoreToCount(saved);
    }

    protected void ApplyRasterClip(DrawingSurface toClip, DrawingSurface clipSource)
    {
        if (ClipToPreviousMember && Background.Value != null)
        {
            toClip.Canvas.DrawSurface(clipSource, 0, 0, maskPaint);
        }
    }

    protected bool IsEmptyMask()
    {
        return EmbeddedMask != null && MaskIsVisible.Value && !EmbeddedMask.LatestOrCommittedChunkExists();
    }

    protected bool HasOperations()
    {
        return (MaskIsVisible.Value && (EmbeddedMask != null || CustomMask.Value != null)) || ClipToPreviousMember;
    }

    protected void DrawClipSource(DrawingSurface drawOnto, IClipSource clipSource, SceneObjectRenderContext context)
    {
        blendPaint.Color = Colors.White;
        clipSource.DrawOnTexture(context, drawOnto);
    }

    public abstract RectD? GetTightBounds(KeyFrameTime frameTime);

    public override void SerializeAdditionalData(Dictionary<string, object> additionalData)
    {
        base.SerializeAdditionalData(additionalData);
        if (EmbeddedMask != null)
        {
            additionalData["embeddedMask"] = EmbeddedMask;
        }
    }

    internal override OneOf<None, IChangeInfo, List<IChangeInfo>> DeserializeAdditionalData(IReadOnlyDocument target,
        IReadOnlyDictionary<string, object> data)
    {
        base.DeserializeAdditionalData(target, data);
        bool hasMask = data.ContainsKey("embeddedMask");
        if (hasMask)
        {
            ChunkyImage? mask = (ChunkyImage?)data["embeddedMask"];

            EmbeddedMask?.Dispose();
            EmbeddedMask = mask;

            return new List<IChangeInfo> { new StructureMemberMask_ChangeInfo(Id, mask != null) };
        }

        return new None();
    }

    public override RectD? GetPreviewBounds(int frame, string elementFor = "")
    {
        if (elementFor == nameof(EmbeddedMask) && EmbeddedMask != null)
        {
            return new RectD(VecD.Zero, EmbeddedMask.LatestSize);
        }

        return null;
    }

    public override bool RenderPreview(DrawingSurface renderOn, ChunkResolution resolution, int frame,
        string elementToRenderName)
    {
        if (elementToRenderName != nameof(EmbeddedMask))
        {
            return false;
        }

        var img = EmbeddedMask;

        if (img is null)
        {
            return false;
        }

        renderOn.Canvas.DrawSurface(renderedMask.DrawingSurface, VecI.Zero, maskPreviewPaint);

        return true;
    }

    public override void Dispose()
    {
        Output.Value = null;
        base.Dispose();
        maskPaint.Dispose();
        blendPaint.Dispose();
    }
}
