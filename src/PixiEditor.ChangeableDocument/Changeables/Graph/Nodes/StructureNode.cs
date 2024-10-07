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

public abstract class StructureNode : Node, IReadOnlyStructureNode, IBackgroundInput
{
    public abstract VecD ScenePosition { get; }
    public abstract VecD SceneSize { get; }

    public const string DefaultMemberName = "DEFAULT_MEMBER_NAME";
    public InputProperty<DrawingSurface?> Background { get; }
    public InputProperty<float> Opacity { get; }
    public InputProperty<bool> IsVisible { get; }
    public bool ClipToPreviousMember { get; set; }
    public InputProperty<BlendMode> BlendMode { get; }
    public InputProperty<Texture?> CustomMask { get; }
    public InputProperty<bool> MaskIsVisible { get; }
    public InputProperty<Filter> Filters { get; }
    public OutputProperty<DrawingSurface?> Output { get; }

    public OutputProperty<Texture?> FilterlessOutput { get; }

    public ChunkyImage? EmbeddedMask { get; set; }

    public virtual ShapeCorners GetTransformationCorners(KeyFrameTime frameTime)
    {
        return new ShapeCorners(GetTightBounds(frameTime).GetValueOrDefault());
    }

    public string MemberName
    {
        get => DisplayName;
        set => DisplayName = value;
    }

    private Paint maskPaint = new Paint() { BlendMode = DrawingApi.Core.Surfaces.BlendMode.DstIn };
    protected Paint blendPaint = new Paint() { BlendMode = DrawingApi.Core.Surfaces.BlendMode.SrcOver};

    private int maskCacheHash = 0;

    protected StructureNode()
    {
        Background = CreateInput<DrawingSurface?>("Background", "BACKGROUND", null);
        Opacity = CreateInput<float>("Opacity", "OPACITY", 1);
        IsVisible = CreateInput<bool>("IsVisible", "IS_VISIBLE", true);
        BlendMode = CreateInput("BlendMode", "BLEND_MODE", Enums.BlendMode.Normal);
        CustomMask = CreateInput<Texture?>("Mask", "MASK", null);
        MaskIsVisible = CreateInput<bool>("MaskIsVisible", "MASK_IS_VISIBLE", true);
        Filters = CreateInput<Filter>(nameof(Filters), "FILTERS", null);

        Output = CreateOutput<DrawingSurface?>("Output", "OUTPUT", null);
        FilterlessOutput = CreateOutput<Texture?>(nameof(FilterlessOutput), "WITHOUT_FILTERS", null);

        MemberName = DefaultMemberName;
    }

    protected override bool AffectedByChunkResolution => true;
    protected override bool AffectedByChunkToUpdate => true;

    protected override void OnExecute(RenderContext context)
    {
        RectD localBounds = new RectD(0, 0, SceneSize.X, SceneSize.Y);

        DrawingSurface sceneSurface = Background.Value ?? context.TargetSurface;
        
        int savedNum = sceneSurface.Canvas.Save();
        sceneSurface.Canvas.ClipRect(RectD.Create((VecI)ScenePosition.Floor(), (VecI)SceneSize.Ceiling()));
        sceneSurface.Canvas.Translate((float)ScenePosition.X, (float)ScenePosition.Y);
        
        SceneObjectRenderContext renderObjectContext = new SceneObjectRenderContext(sceneSurface, localBounds, 
            context.FrameTime, context.ChunkResolution, context.DocumentSize) { ChunkToUpdate = context.ChunkToUpdate };
        
        Render(renderObjectContext);
        
        sceneSurface.Canvas.RestoreToCount(savedNum);
        
        Output.Value = sceneSurface;
    }

    public abstract void Render(SceneObjectRenderContext sceneContext);

    protected void ApplyMaskIfPresent(DrawingSurface surface, RenderContext context)
    {
        if (MaskIsVisible.Value)
        {
            if (CustomMask.Value != null)
            {
                surface.Canvas.DrawSurface(CustomMask.Value.DrawingSurface, 0, 0, maskPaint);
            }
            else if (EmbeddedMask != null)
            {
                EmbeddedMask.DrawMostUpToDateChunkOn(
                    context.ChunkToUpdate.Value,
                    context.ChunkResolution,
                    surface,
                    context.ChunkToUpdate.Value * context.ChunkResolution.PixelSize(),
                    maskPaint);
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

    protected void DrawPreviousLayer(DrawingSurface drawOnto, LayerNode previousNode, SceneObjectRenderContext context)
    {
        blendPaint.Color = Colors.White;
        previousNode.DrawLayer(context, drawOnto, false);
    }

    protected void DrawSurface(DrawingSurface workingSurface, DrawingSurface source, RenderContext context,
        Filter? filter)
    {
        // Maybe clip rect will allow to avoid snapshotting? Idk if it will be faster
        /*
        RectI sourceRect = CalculateSourceRect(workingSurface.Size, source.Size, context);
        RectI targetRect = CalculateDestinationRect(context);
        using var snapshot = source.DrawingSurface.Snapshot(sourceRect);
        */
        
        blendPaint.SetFilters(filter);
        
        workingSurface.Canvas.DrawSurface(source, source.DeviceClipBounds.X, source.DeviceClipBounds.Y, blendPaint);
    }

    protected RectI CalculateSourceRect(VecI targetSize, VecI sourceSize, RenderContext context)
    {
        float divider = 1;

        if (sourceSize.X < targetSize.X || sourceSize.Y < targetSize.Y)
        {
            divider = Math.Min((float)targetSize.X / sourceSize.X, (float)targetSize.Y / sourceSize.Y);
        }

        int chunkSize = (int)Math.Round(context.ChunkResolution.PixelSize() / divider);
        VecI chunkPos = context.ChunkToUpdate.Value;

        int x = (int)(chunkPos.X * chunkSize);
        int y = (int)(chunkPos.Y * chunkSize);
        int width = (int)(chunkSize);
        int height = (int)(chunkSize);

        x = Math.Clamp(x, 0, Math.Max(sourceSize.X - width, 0));
        y = Math.Clamp(y, 0, Math.Max(sourceSize.Y - height, 0));

        return new RectI(x, y, width, height);
    }

    protected RectI CalculateDestinationRect(RenderContext context)
    {
        int chunkSize = context.ChunkResolution.PixelSize();
        VecI chunkPos = context.ChunkToUpdate.Value;

        int x = chunkPos.X * chunkSize;
        int y = chunkPos.Y * chunkSize;
        int width = chunkSize;
        int height = chunkSize;

        return new RectI(x, y, width, height);
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

    public override void Dispose()
    {
        base.Dispose();
        maskPaint.Dispose();
        blendPaint.Dispose();
    }
}
