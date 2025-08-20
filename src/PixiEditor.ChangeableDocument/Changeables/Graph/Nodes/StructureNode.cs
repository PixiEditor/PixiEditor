using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.Properties;
using PixiEditor.ChangeableDocument.Helpers;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using BlendMode = PixiEditor.ChangeableDocument.Enums.BlendMode;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public abstract class StructureNode : RenderNode, IReadOnlyStructureNode, IRenderInput
{
    public const string DefaultMemberName = "DEFAULT_MEMBER_NAME";
    public const string IsVisiblePropertyName = "IsVisible";
    public const string OpacityPropertyName = "Opacity";
    public const string BlendModePropertyName = "BlendMode";
    public const string ClipToPreviousMemberPropertyName = "ClipToPreviousMember";
    public const string MaskIsVisiblePropertyName = "MaskIsVisible";
    public const string MaskPropertyName = "Mask";
    public const string FiltersPropertyName = "Filters";
    public const string FilterlessOutputPropertyName = "FilterlessOutput";
    public const string RawOutputPropertyName = "RawOutput";

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

    public OutputProperty<VecD> TightSize { get; }
    public OutputProperty<VecD> CanvasPosition { get; }
    public OutputProperty<VecD> CenterPosition { get; }

    public ChunkyImage? EmbeddedMask { get; set; }

    protected Texture renderedMask;

    protected static readonly Paint replacePaint =
        new Paint() { BlendMode = Drawie.Backend.Core.Surfaces.BlendMode.Src };

    protected static readonly Paint clearPaint = new Paint()
    {
        BlendMode = Drawie.Backend.Core.Surfaces.BlendMode.Src, Color = Colors.Transparent
    };

    protected static readonly Paint clipPaint = new Paint()
    {
        BlendMode = Drawie.Backend.Core.Surfaces.BlendMode.DstIn
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

    protected Paint maskPaint = new Paint()
    {
        BlendMode = Drawie.Backend.Core.Surfaces.BlendMode.DstIn, ColorFilter = Nodes.Filters.MaskFilter
    };

    protected Paint blendPaint = new Paint() { BlendMode = Drawie.Backend.Core.Surfaces.BlendMode.SrcOver };

    protected Paint maskPreviewPaint = new Paint()
    {
        BlendMode = Drawie.Backend.Core.Surfaces.BlendMode.SrcOver,
        ColorFilter = ColorFilter.CreateCompose(Nodes.Filters.AlphaGrayscaleFilter, Nodes.Filters.MaskFilter)
    };

    protected StructureNode()
    {
        Painter filterlessPainter = new Painter(OnFilterlessPaint);
        Painter rawPainter = new Painter(OnRawPaint);

        Background = CreateRenderInput("Background", "BACKGROUND");
        Opacity = CreateInput<float>(OpacityPropertyName, "OPACITY", 1);
        IsVisible = CreateInput<bool>(IsVisiblePropertyName, "IS_VISIBLE", true);
        BlendMode = CreateInput(BlendModePropertyName, "BLEND_MODE", Enums.BlendMode.Normal);
        CustomMask = CreateRenderInput(MaskPropertyName, "MASK");
        MaskIsVisible = CreateInput<bool>(MaskIsVisiblePropertyName, "MASK_IS_VISIBLE", true);
        Filters = CreateInput<Filter>(FiltersPropertyName, "FILTERS", null);

        FilterlessOutput = CreateRenderOutput(FilterlessOutputPropertyName, "WITHOUT_FILTERS",
            () => filterlessPainter, () => Background.Value);

        RawOutput = CreateRenderOutput(RawOutputPropertyName, "RAW_LAYER_OUTPUT", () => rawPainter);

        CanvasPosition = CreateOutput<VecD>("CanvasPosition", "CANVAS_POSITION", VecD.Zero);
        CenterPosition = CreateOutput<VecD>("CenterPosition", "CENTER_POSITION", VecD.Zero);
        TightSize = CreateOutput<VecD>("Size", "SIZE", VecD.Zero);

        MemberName = DefaultMemberName;
    }

    protected override void OnExecute(RenderContext context)
    {
        base.OnExecute(context);

        if (TightSize.Connections.Count > 0)
        {
            TightSize.Value = GetTightBounds(context.FrameTime)?.Size ?? VecD.Zero;
        }

        if (CanvasPosition.Connections.Count > 0)
        {
            CanvasPosition.Value = GetTightBounds(context.FrameTime)?.TopLeft ?? VecD.Zero;
        }

        if (CenterPosition.Connections.Count > 0)
        {
            CenterPosition.Value = GetTightBounds(context.FrameTime)?.Center ?? VecD.Zero;
        }
    }

    protected override void Paint(RenderContext context, DrawingSurface surface)
    {
        OnPaint(context, surface);
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

    public void RenderForOutput(RenderContext context, DrawingSurface renderTarget, RenderOutputProperty output)
    {
        if (IsDisposed)
        {
            return;
        }

        var renderObjectContext = CreateSceneContext(context, renderTarget, output);

        int renderSaved = renderTarget.Canvas.Save();
        VecD scenePos = GetScenePosition(context.FrameTime);
        VecD sceneSize = GetSceneSize(context.FrameTime);
        //renderTarget.Canvas.ClipRect(new RectD(scenePos - (sceneSize / 2f), sceneSize));

        Render(renderObjectContext);

        renderTarget?.Canvas.RestoreToCount(renderSaved);
    }

    protected SceneObjectRenderContext CreateSceneContext(RenderContext context, DrawingSurface renderTarget,
        RenderOutputProperty output)
    {
        var sceneSize = GetSceneSize(context.FrameTime);
        RectD localBounds = new RectD(0, 0, sceneSize.X, sceneSize.Y);

        SceneObjectRenderContext renderObjectContext = new SceneObjectRenderContext(output, renderTarget, localBounds,
            context.FrameTime, context.ChunkResolution, context.RenderOutputSize, context.DocumentSize, renderTarget == context.RenderSurface,
            context.ProcessingColorSpace, context.DesiredSamplingOptions, context.Opacity);
        renderObjectContext.FullRerender = context.FullRerender;
        return renderObjectContext;
    }

    public abstract void Render(SceneObjectRenderContext sceneContext);

    protected void ApplyMaskIfPresent(DrawingSurface surface, RenderContext context, ChunkResolution renderResolution)
    {
        if (MaskIsVisible.Value)
        {
            if (CustomMask.Value != null)
            {
                int layer = surface.Canvas.SaveLayer(maskPaint);
                surface.Canvas.Scale((float)renderResolution.Multiplier());
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
                else if (renderedMask != null)
                {
                    int saved = surface.Canvas.Save();
                    surface.Canvas.Scale((float)renderResolution.Multiplier());
                    surface.Canvas.DrawSurface(renderedMask.DrawingSurface, 0, 0, maskPaint);
                    surface.Canvas.RestoreToCount(saved);
                }
            }
        }
    }

    protected override int GetContentCacheHash()
    {
        return HashCode.Combine(base.GetContentCacheHash(), EmbeddedMask?.GetCacheHash() ?? 0,
            ClipToPreviousMember ? 1 : 0);
    }

    public virtual void RenderChunk(VecI chunkPos, ChunkResolution resolution, KeyFrameTime frameTime,
        ColorSpace processingColorSpace)
    {
        RenderChunkyImageChunk(chunkPos, resolution, EmbeddedMask, 55, processingColorSpace, ref renderedMask);
    }

    protected void RenderChunkyImageChunk(VecI chunkPos, ChunkResolution resolution, ChunkyImage img,
        int textureId, ColorSpace processingColorSpace,
        ref Texture? renderSurface)
    {
        if (img is null)
        {
            return;
        }

        VecI targetSize = img.LatestSize;

        if (targetSize.X <= 0 || targetSize.Y <= 0)
        {
            return;
        }

        using var ctx = DrawingBackendApi.Current.RenderingDispatcher.EnsureContext();
        renderSurface = RequestTexture(textureId, targetSize, processingColorSpace, false);

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
            toClip.Canvas.DrawSurface(clipSource, 0, 0, clipPaint);
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
        clipSource.DrawClipSource(context, drawOnto);
    }

    public abstract RectD? GetTightBounds(KeyFrameTime frameTime);
    public abstract RectD? GetApproxBounds(KeyFrameTime frameTime);

    public override void SerializeAdditionalData(Dictionary<string, object> additionalData)
    {
        base.SerializeAdditionalData(additionalData);
        if (EmbeddedMask != null)
        {
            additionalData["embeddedMask"] = EmbeddedMask;
        }

        if (ClipToPreviousMember)
        {
            additionalData["clipToPreviousMember"] = ClipToPreviousMember;
        }
    }

    internal override void DeserializeAdditionalData(IReadOnlyDocument target,
        IReadOnlyDictionary<string, object> data, List<IChangeInfo> infos)
    {
        base.DeserializeAdditionalData(target, data, infos);

        bool hasMask = data.ContainsKey("embeddedMask");
        if (hasMask)
        {
            ChunkyImage? mask = (ChunkyImage?)data["embeddedMask"];

            EmbeddedMask?.Dispose();
            EmbeddedMask = mask;

            infos.Add(new StructureMemberMask_ChangeInfo(Id, mask != null));
        }

        if (data.TryGetValue("clipToPreviousMember", out var clip))
        {
            ClipToPreviousMember = (bool)clip;
            infos.Add(new StructureMemberClipToMemberBelow_ChangeInfo(Id, ClipToPreviousMember));
        }
    }

    public override RectD? GetPreviewBounds(int frame, string elementFor = "")
    {
        if (elementFor == nameof(EmbeddedMask) && EmbeddedMask != null)
        {
            return new RectD(VecD.Zero, EmbeddedMask.LatestSize);
        }

        return null;
    }

    public override bool RenderPreview(DrawingSurface renderOn, RenderContext context,
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
        base.Dispose();
        renderedMask?.Dispose();
        EmbeddedMask?.Dispose();
        Output.Value = null;
        maskPaint.Dispose();
        blendPaint.Dispose();
        maskPreviewPaint.Dispose();
    }
}
