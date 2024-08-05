using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.ChangeableDocument.Helpers;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public abstract class StructureNode : Node, IReadOnlyStructureNode, IBackgroundInput
{
    public InputProperty<Texture?> Background { get; }
    public InputProperty<float> Opacity { get; }
    public InputProperty<bool> IsVisible { get; }
    public InputProperty<bool> ClipToPreviousMember { get; }
    public InputProperty<BlendMode> BlendMode { get; }
    public InputProperty<ChunkyImage?> Mask { get; }
    public InputProperty<bool> MaskIsVisible { get; }
    public InputProperty<Filter> Filters { get; }

    public OutputProperty<Texture?> Output { get; }

    public OutputProperty<Texture?> FilterlessOutput { get; }

    public string MemberName { get; set; } = "New Element"; // would be good to add localization here, it is set if node is created via node graph
    
    public override string DisplayName
    {
        get => MemberName;
        set => MemberName = value;
    }

    protected Dictionary<(ChunkResolution, int), Texture> workingSurfaces = new Dictionary<(ChunkResolution, int), Texture>();
    private Paint maskPaint = new Paint() { BlendMode = DrawingApi.Core.Surfaces.BlendMode.DstIn };
    protected Paint blendPaint = new Paint();

    protected StructureNode()
    {
        Background = CreateInput<Texture?>("Background", "BACKGROUND", null);
        Opacity = CreateInput<float>("Opacity", "OPACITY", 1);
        IsVisible = CreateInput<bool>("IsVisible", "IS_VISIBLE", true);
        ClipToPreviousMember = CreateInput<bool>("ClipToMemberBelow", "CLIP_TO_MEMBER_BELOW", false);
        BlendMode = CreateInput<BlendMode>("BlendMode", "BLEND_MODE", Enums.BlendMode.Normal);
        Mask = CreateInput<ChunkyImage?>("Mask", "MASK", null);
        MaskIsVisible = CreateInput<bool>("MaskIsVisible", "MASK_IS_VISIBLE", true);
        Filters = CreateInput<Filter>(nameof(Filters), "FILTERS", null);

        Output = CreateOutput<Texture?>("Output", "OUTPUT", null);
        FilterlessOutput = CreateOutput<Texture?>(nameof(FilterlessOutput), "WITHOUT_FILTERS", null);
    }

    protected abstract override Texture? OnExecute(RenderingContext context);

    protected Texture TryInitWorkingSurface(VecI imageSize, RenderingContext context, int id)
    {
        ChunkResolution targetResolution = context.ChunkResolution;
        bool hasSurface = workingSurfaces.TryGetValue((targetResolution, id), out Texture workingSurface);
        VecI targetSize = (VecI)(imageSize * targetResolution.Multiplier());

        if (!hasSurface || workingSurface.Size != targetSize || workingSurface.IsDisposed)
        {
            workingSurfaces[(targetResolution, id)] = new Texture(targetSize);
            workingSurface = workingSurfaces[(targetResolution, id)];
        }

        return workingSurface;
    }

    protected void ApplyMaskIfPresent(Texture surface, RenderingContext context)
    {
        if (Mask.Value != null && MaskIsVisible.Value)
        {
            Mask.Value.DrawMostUpToDateChunkOn(
                context.ChunkToUpdate,
                context.ChunkResolution,
                surface.Surface,
                context.ChunkToUpdate * context.ChunkResolution.PixelSize(),
                maskPaint);
        }
    }

    protected void ApplyRasterClip(Texture surface, RenderingContext context)
    {
        if (ClipToPreviousMember.Value && Background.Value != null)
        {
            RectI? clippingRect = null;
            VecI chunkStart = context.ChunkToUpdate * context.ChunkResolution.PixelSize();
            VecI targetSize = new VecI(context.ChunkResolution.PixelSize());
            clippingRect = new RectI(chunkStart, targetSize);

            //TODO: Implement this
            // OperationHelper.ClampAlpha(surface.DrawingSurface, Background.Value, clippingRect);
        }
    }


    protected bool IsEmptyMask()
    {
        return Mask.Value != null && MaskIsVisible.Value && !Mask.Value.LatestOrCommittedChunkExists();
    }

    protected bool HasOperations()
    {
        return (MaskIsVisible.Value && Mask.Value != null) || ClipToPreviousMember.Value;
    }

    protected void DrawBackground(Texture workingSurface, RenderingContext context)
    {
        blendPaint.Color = Colors.White;
        DrawSurface(workingSurface, Background.Value, context, null); 
    }

    protected void DrawSurface(Texture workingSurface, Texture source, RenderingContext context, Filter? filter)
    {
        // Maybe clip rect will allow to avoid snapshotting? Idk if it will be faster
        RectI sourceRect = CalculateSourceRect(source, workingSurface.Size, context);
        RectI targetRect = CalculateDestinationRect(context);
        using var snapshot = source.Surface.Snapshot(sourceRect);

        blendPaint.SetFilters(filter);
        workingSurface.Surface.Canvas.DrawImage(snapshot, targetRect.X, targetRect.Y, blendPaint);
    }

    protected RectI CalculateSourceRect(Texture image, VecI targetSize, RenderingContext context)
    {
        float multiplierToFit = image.Size.X / (float)targetSize.X;
        int chunkSize = context.ChunkResolution.PixelSize();
        VecI chunkPos = context.ChunkToUpdate;

        int x = (int)(chunkPos.X * chunkSize * multiplierToFit);
        int y = (int)(chunkPos.Y * chunkSize * multiplierToFit);
        int width = (int)(chunkSize * multiplierToFit);
        int height = (int)(chunkSize * multiplierToFit);

        return new RectI(x, y, width, height);
    }

    protected RectI CalculateDestinationRect(RenderingContext context)
    {
        int chunkSize = context.ChunkResolution.PixelSize();
        VecI chunkPos = context.ChunkToUpdate;

        int x = chunkPos.X * chunkSize;
        int y = chunkPos.Y * chunkSize;
        int width = chunkSize;
        int height = chunkSize;

        return new RectI(x, y, width, height);
    }

    public abstract RectI? GetTightBounds(KeyFrameTime frameTime);

    public override void Dispose()
    {
        base.Dispose();
        maskPaint.Dispose();
        blendPaint.Dispose();
    }
}
