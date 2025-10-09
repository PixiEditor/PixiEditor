using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;
using BlendMode = PixiEditor.ChangeableDocument.Enums.BlendMode;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;

[NodeInfo(NodeId)]
public class BrushOutputNode : Node
{
    public const string NodeId = "BrushOutput";
    public const string BrushNameProperty = "BrushName";

    public InputProperty<string> BrushName { get; }
    public InputProperty<ShapeVectorData> VectorShape { get; }
    public InputProperty<Paintable> Stroke { get; }
    public InputProperty<Paintable> Fill { get; }
    public RenderInputProperty Content { get; }
    public InputProperty<BlendMode> BlendMode { get; }
    public InputProperty<Matrix3X3> Transform { get; }
    public InputProperty<float> Pressure { get; }
    public InputProperty<bool> FitToStrokeSize { get; }
    public InputProperty<bool> AutoPosition { get; }
    public InputProperty<bool> AllowSampleStacking { get; }

    internal Texture ContentTexture;

    private TextureCache cache = new();

    private ChunkyImage? previewChunkyImage;

    protected override bool ExecuteOnlyOnCacheChange => true;

    public BrushOutputNode()
    {
        BrushName = CreateInput<string>(BrushNameProperty, "NAME", "Unnamed");
        VectorShape = CreateInput<ShapeVectorData>("VectorShape", "SHAPE", null);
        Stroke = CreateInput<Paintable>("Stroke", "STROKE", null);
        Fill = CreateInput<Paintable>("Fill", "FILL", null);
        Content = CreateRenderInput("Content", "CONTENT");
        Transform = CreateInput<Matrix3X3>("Transform", "TRANSFORM", Matrix3X3.Identity);
        BlendMode = CreateInput<BlendMode>("BlendMode", "BLEND_MODE", Enums.BlendMode.Normal);

        Pressure = CreateInput<float>("Pressure", "PRESSURE", 1f);
        FitToStrokeSize = CreateInput<bool>("FitToStrokeSize", "FIT_TO_STROKE_SIZE", true);
        AutoPosition = CreateInput<bool>("AutoPosition", "AUTO_POSITION", true);
        AllowSampleStacking = CreateInput<bool>("AllowSampleStacking", "ALLOW_SAMPLE_STACKING", false);
    }

    protected override void OnExecute(RenderContext context)
    {
        if (Content.Value != null)
        {
            ContentTexture = cache.RequestTexture(0, context.RenderOutputSize, context.ProcessingColorSpace);
            ContentTexture.DrawingSurface.Canvas.Save();
            ContentTexture.DrawingSurface.Canvas.SetMatrix(Transform.Value);
            Content.Value.Paint(context, ContentTexture.DrawingSurface);
            ContentTexture.DrawingSurface.Canvas.Restore();
        }

        RenderPreviews(context.GetPreviewTexturesForNode(Id), context);
    }

    private void RenderPreviews(List<PreviewRenderRequest>? previews, RenderContext ctx)
    {
        var previewToRender = previews;
        if (previewToRender == null || previewToRender.Count == 0)
            return;

        foreach (var preview in previewToRender)
        {
            if (preview.Texture == null)
                continue;

            int saved = preview.Texture.DrawingSurface.Canvas.Save();
            preview.Texture.DrawingSurface.Canvas.Clear();

            var bounds = new RectD(0, 0, 300, 100);

            VecD scaling = PreviewUtility.CalculateUniformScaling(bounds.Size, preview.Texture.Size);
            VecD offset = PreviewUtility.CalculateCenteringOffset(bounds.Size, preview.Texture.Size, scaling);
            RenderContext adjusted =
                PreviewUtility.CreatePreviewContext(ctx, scaling, bounds.Size, preview.Texture.Size);

            preview.Texture.DrawingSurface.Canvas.Translate((float)offset.X, (float)offset.Y);
            preview.Texture.DrawingSurface.Canvas.Scale((float)scaling.X, (float)scaling.Y);
            preview.Texture.DrawingSurface.Canvas.Translate((float)-bounds.X, (float)-bounds.Y);

            adjusted.RenderSurface = preview.Texture.DrawingSurface;
            RenderPreview(preview.Texture.DrawingSurface, adjusted);
            preview.Texture.DrawingSurface.Canvas.RestoreToCount(saved);
        }
    }

    private void RenderPreview(DrawingSurface surface, RenderContext context)
    {
        if (previewChunkyImage == null)
        {
            previewChunkyImage = new ChunkyImage(new VecI(300, 100), context.ProcessingColorSpace);
        }

        RectI rect = new(0, 0, 300, 100);

        BrushEngine.PaintBrush(previewChunkyImage,
            AutoPosition.Value,
            VectorShape.Value,
            rect,
            FitToStrokeSize.Value,
            Pressure.Value,
            Content.Value,
            ContentTexture,
            RenderContext.GetDrawingBlendMode(BlendMode.Value),
            true,
            Fill.Value,
            Stroke.Value);

        previewChunkyImage.DrawCachedMostUpToDateChunkOn(
            VecI.Zero, ChunkResolution.Full, surface, VecD.Zero);
    }


    public override Node CreateCopy()
    {
        return new BrushOutputNode();
    }
}
