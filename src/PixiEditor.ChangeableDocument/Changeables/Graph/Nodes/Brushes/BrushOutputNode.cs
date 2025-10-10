using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.ChangeableDocument.Rendering.ContextData;
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

    private string previewSvg =
        "M0.25 99.4606C0.25 99.4606 60.5709 79.3294 101.717 99.4606C147.825 122.019 199.75 99.4606 199.75 99.4606";

    private VectorPath? previewVectorPath;

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

            var bounds = new RectD(0, 0, 200, 200);

            RenderContext adjusted =
                PreviewUtility.CreatePreviewContext(ctx, new VecD(1), bounds.Size, preview.Texture.Size);

            adjusted.RenderSurface = preview.Texture.DrawingSurface;
            RenderPreview(preview.Texture.DrawingSurface, adjusted);
            preview.Texture.DrawingSurface.Canvas.RestoreToCount(saved);
        }
    }

    private void RenderPreview(DrawingSurface surface, RenderContext context)
    {
        if (previewChunkyImage == null)
        {
            previewChunkyImage = new ChunkyImage(new VecI(200, 200), context.ProcessingColorSpace);
        }

        if (previewVectorPath == null)
        {
            previewVectorPath = VectorPath.FromSvgPath(previewSvg);
        }

        RectI rect;

        BrushEngine engine = new BrushEngine();
        previewChunkyImage.EnqueueClear();

        float pressure;
        int maxSize = 50;
        float offset = 0;

        int[] sizes = new int[] { 10, 25, 50 };
        VecD pos;
        for (var i = 0; i < sizes.Length; i++)
        {
            var size = sizes[i];
            int x = 25 + i * 60;
            pos = new VecI(x, maxSize);

            engine.ExecuteBrush(previewChunkyImage,
                new BrushData(context.Graph) { StrokeWidth = size, AntiAliasing = true, Spacing = 0 },
                (VecI)pos, context.FrameTime, context.ProcessingColorSpace, context.DesiredSamplingOptions,
                new PointerInfo(pos, 1, 0, VecD.Zero, new VecD(0, 1)),
                new EditorData(Colors.White, Colors.Black));
        }

        while (offset <= previewChunkyImage.CommittedSize.X)
        {
            pressure = (float)Math.Sin((offset / previewChunkyImage.CommittedSize.X) * Math.PI);
            var vec4D = previewVectorPath.GetPositionAndTangentAtDistance(offset, false);
            pos = vec4D.XY;
            pos = new VecD(pos.X, pos.Y + maxSize / 2f);

            engine.ExecuteBrush(previewChunkyImage,
                new BrushData(context.Graph) { StrokeWidth = maxSize, AntiAliasing = true, Spacing = 15 },
                (VecI)pos, context.FrameTime, context.ProcessingColorSpace, context.DesiredSamplingOptions,
                new PointerInfo(pos, pressure, 0, VecD.Zero, vec4D.ZW),
                new EditorData(Colors.White, Colors.Black));

            offset += 1;
        }

        previewChunkyImage.CommitChanges();
        previewChunkyImage.DrawCommittedChunkOn(
            VecI.Zero, ChunkResolution.Full, surface, VecD.Zero);
    }


    public override Node CreateCopy()
    {
        return new BrushOutputNode();
    }
}
