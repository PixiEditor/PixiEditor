using System.Collections;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.ChangeableDocument.Rendering.ContextData;
using BlendMode = PixiEditor.ChangeableDocument.Enums.BlendMode;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;

[NodeInfo(NodeId)]
public class BrushOutputNode : Node
{
    public const string NodeId = "BrushOutput";
    public const string BrushNameProperty = "BrushName";
    public const string FitToStrokeSizeProperty = "FitToStrokeSize";
    public const int PointPreviewSize = 50;
    public const int StrokePreviewSizeX = 200;
    public const int StrokePreviewSizeY = 50;

    public const string DefaultBlenderCode = @"
    vec4 main(vec4 src, vec4 dst) {
    	return src + (1 - src.a) * dst;
    }
";

    private string? lastStampBlenderCode = "";
    private string? lastImageBlenderCode = "";

    public Blender? LastStampBlender => cachedStampBlender;
    public Blender? LastImageBlender => cachedImageBlender;

    private Blender? cachedStampBlender = null;
    private Blender? cachedImageBlender = null;

    public InputProperty<string> BrushName { get; }
    public InputProperty<ShapeVectorData> VectorShape { get; }
    public InputProperty<Paintable> Stroke { get; }
    public InputProperty<Paintable> Fill { get; }
    public RenderInputProperty Content { get; }
    public InputProperty<Drawie.Backend.Core.Surfaces.BlendMode> StampBlendMode { get; }
    public InputProperty<Drawie.Backend.Core.Surfaces.BlendMode> ImageBlendMode { get; }
    public InputProperty<bool> UseCustomStampBlender { get; }
    public InputProperty<string> CustomStampBlenderCode { get; }
    public InputProperty<Matrix3X3> Transform { get; }
    public InputProperty<float> Pressure { get; }
    public InputProperty<float> Spacing { get; }
    public InputProperty<bool> FitToStrokeSize { get; }
    public InputProperty<bool> AutoPosition { get; }
    public InputProperty<bool> AllowSampleStacking { get; }
    public InputProperty<bool> AlwaysClear { get; }
    public InputProperty<bool> SnapToPixels { get; }

    public InputProperty<string> Tags { get; }

    // Indicate whether stamps from this brush can be reused when drawing with the same brush again. Optimization option.
    public InputProperty<bool> CanReuseStamps { get; }

    public InputProperty<IReadOnlyNodeGraph> Previous { get; }

    internal Texture ContentTexture;

    private TextureCache cache = new();

    private ChunkyImage? previewChunkyImage;
    private BrushEngine previewEngine = new BrushEngine() { PressureSmoothingWindowSize = 0 };

    protected override bool ExecuteOnlyOnCacheChange => true;
    public Guid PersistentId { get; private set; } = Guid.NewGuid();

    public const string PreviewSvg =
        "M0.25 99.4606C0.25 99.4606 60.5709 79.3294 101.717 99.4606C147.825 122.019 199.75 99.4606 199.75 99.4606";

    public const int YOffsetInPreview = -88;
    public const string UseCustomStampBlenderProperty = "UseCustomStampBlender";
    public const string CustomStampBlenderCodeProperty = "CustomStampBlender";
    public const string StampBlendModeProperty = "StampBlendMode";
    public const string ContentProperty = "Content";
    public const string ContentTransformProperty = "Transform";

    private VectorPath? previewVectorPath;
    private bool drawnContentTextureOnce = false;
    private Matrix3X3 lastTranform = Matrix3X3.Identity;

    public BrushOutputNode()
    {
        BrushName = CreateInput<string>(BrushNameProperty, "NAME", "Unnamed");
        VectorShape = CreateInput<ShapeVectorData>("VectorShape", "SHAPE", null);
        Stroke = CreateInput<Paintable>("Stroke", "STROKE", null);
        Fill = CreateInput<Paintable>("Fill", "FILL", null);
        Content = CreateRenderInput(ContentProperty, "CONTENT");
        Transform = CreateInput<Matrix3X3>(ContentTransformProperty, "CONTENT_TRANSFORM", Matrix3X3.Identity);
        ImageBlendMode = CreateInput<Drawie.Backend.Core.Surfaces.BlendMode>("BlendMode", "BLEND_MODE",
            Drawie.Backend.Core.Surfaces.BlendMode.SrcOver);
        StampBlendMode = CreateInput<Drawie.Backend.Core.Surfaces.BlendMode>(StampBlendModeProperty, "STAMP_BLEND_MODE",
            Drawie.Backend.Core.Surfaces.BlendMode.SrcOver);

        UseCustomStampBlender = CreateInput<bool>(UseCustomStampBlenderProperty, "USE_CUSTOM_STAMP_BLENDER", false);

        CustomStampBlenderCode =
            CreateInput<string>(CustomStampBlenderCodeProperty, "CUSTOM_STAMP_BLENDER_CODE", DefaultBlenderCode)
                .WithRules(validator => validator.Custom(ValidateBlenderCode));
        CanReuseStamps = CreateInput<bool>("CanReuseStamps", "CAN_REUSE_STAMPS", false);

        Pressure = CreateInput<float>("Pressure", "PRESSURE", 1f);
        Spacing = CreateInput<float>("Spacing", "SPACING", 0);
        FitToStrokeSize = CreateInput<bool>(FitToStrokeSizeProperty, "FIT_TO_STROKE_SIZE", true);
        AutoPosition = CreateInput<bool>("AutoPosition", "AUTO_POSITION", true);
        AllowSampleStacking = CreateInput<bool>("AllowSampleStacking", "ALLOW_SAMPLE_STACKING", false);
        AlwaysClear = CreateInput<bool>("AlwaysClear", "ALWAYS_CLEAR", false);
        SnapToPixels = CreateInput<bool>("SnapToPixels", "SNAP_TO_PIXELS", false);
        Tags = CreateInput<string>("Tags", "TAGS", "");
        Previous = CreateInput<IReadOnlyNodeGraph>("Previous", "PREVIOUS", null);
    }

    private ValidatorResult ValidateBlenderCode(object? value)
    {
        if (value is string code)
        {
            Blender? blender = Blender.CreateFromString(code, out string? error);
            if (blender != null)
            {
                blender.Dispose();
                return new ValidatorResult(true, null);
            }

            return new ValidatorResult(false, error);
        }

        return new ValidatorResult(false, "Blender code must be a string.");
    }

    protected override void OnExecute(RenderContext context)
    {
        if (Content.Value != null)
        {
            if (context.RenderOutputSize.LongestAxis > 0)
            {
                if (!CanReuseStamps.Value || ContentTexture == null || ContentTexture.Size != context.RenderOutputSize ||
                    !drawnContentTextureOnce || Transform.Value != lastTranform)
                {
                    ContentTexture = cache.RequestTexture(0, context.RenderOutputSize, context.ProcessingColorSpace);
                    ContentTexture.DrawingSurface.Canvas.Save();
                    ContentTexture.DrawingSurface.Canvas.SetMatrix(Transform.Value);
                    Content.Value.Paint(context, ContentTexture.DrawingSurface.Canvas);
                    ContentTexture.DrawingSurface.Canvas.Restore();
                    drawnContentTextureOnce = true;
                    lastTranform = Transform.Value;
                }
            }
        }

        if (UseCustomStampBlender.Value)
        {
            if (CustomStampBlenderCode.Value != lastStampBlenderCode || cachedStampBlender == null)
            {
                cachedStampBlender?.Dispose();
                cachedStampBlender = Blender.CreateFromString(CustomStampBlenderCode.Value, out _);
                lastStampBlenderCode = CustomStampBlenderCode.Value;
            }
        }
        else
        {
            cachedStampBlender?.Dispose();
            cachedStampBlender = null;
            lastStampBlenderCode = "";
        }

        RenderPreviews(context.GetPreviewTexturesForNode(Id), context);
    }

    internal override void SerializeAdditionalDataInternal(IReadOnlyDocument target, Dictionary<string, object> additionalData)
    {
        base.SerializeAdditionalDataInternal(target, additionalData);
        additionalData["PersistentId"] = PersistentId;
    }

    public void ResetContentTexture()
    {
        drawnContentTextureOnce = false;
    }

    internal override void DeserializeAdditionalDataInternal(IReadOnlyDocument target, IReadOnlyDictionary<string, object> data,
        List<IChangeInfo> infos)
    {
        base.DeserializeAdditionalDataInternal(target, data, infos);
        if (data.TryGetValue("PersistentId", out var persistentIdObj))
        {
            if (persistentIdObj is Guid persistentId)
            {
                PersistentId = persistentId;
            }
            else if (persistentIdObj is string persistentIdStr && Guid.TryParse(persistentIdStr, out Guid parsedGuid))
            {
                PersistentId = parsedGuid;
            }
        }
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

            adjusted.RenderSurface = preview.Texture.DrawingSurface.Canvas;
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

        RectI rect;

        previewChunkyImage.EnqueueClear();
        previewChunkyImage.CommitChanges();

        int maxSize = 50;
        float offset = 0;

        int[] sizes = new int[] { 10, 25, 50 };
        const int spacing = 10;
        const int marginEdges = 30;
        VecD pos = VecD.Zero;
        previewEngine.ResetState();

        for (var i = 0; i < sizes.Length; i++)
        {
            var size = sizes[i];
            int x = marginEdges + (int)(i * (size + spacing + (maxSize - size) / 2f));
            pos = new VecI(x, maxSize);

            previewEngine.ExecuteBrush(previewChunkyImage,
                new BrushData(context.Graph, Id) { StrokeWidth = size, AntiAliasing = true },
                (VecI)pos, context.FrameTime, context.ProcessingColorSpace, context.DesiredSamplingOptions,
                new PointerInfo(pos, 1, 0, VecD.Zero, new VecD(0, 1), true, false),
                new KeyboardInfo(),
                new EditorData(Colors.White, Colors.Black));
        }
        previewChunkyImage.CommitChanges();

        DrawStrokePreview(previewChunkyImage, context, maxSize);

        previewChunkyImage.CommitChanges();
        previewChunkyImage.DrawCommittedChunkOn(
            VecI.Zero, ChunkResolution.Full, surface.Canvas, VecD.Zero);
    }

    public void DrawStrokePreview(ChunkyImage target, RenderContext context, int maxSize, VecD shift = default)
    {
        if (previewVectorPath == null)
        {
            previewVectorPath = VectorPath.FromSvgPath(PreviewSvg);
        }

        float offset = 0;
        float pressure;
        VecD pos;
        List<RecordedPoint> points = new();
        previewEngine.ResetState();

        while (offset <= target.CommittedSize.X)
        {
            pressure = (float)Math.Sin((offset / target.CommittedSize.X) * Math.PI);
            var vec4D = previewVectorPath.GetPositionAndTangentAtDistance(offset, false);
            pos = vec4D.XY;
            pos = new VecD(pos.X, pos.Y + maxSize / 2f) + shift;

            points.Add(new RecordedPoint((VecI)pos, new PointerInfo(pos, pressure, 0, VecD.Zero, vec4D.ZW, true, false),
                new KeyboardInfo(), new EditorData(Colors.White, Colors.Black)));

            previewEngine.ExecuteBrush(target,
                new BrushData(context.Graph, Id) { StrokeWidth = maxSize, AntiAliasing = true }, points,
                context.FrameTime,
                context.ProcessingColorSpace, context.DesiredSamplingOptions);
            offset += 1;
        }
    }

    public IEnumerable<float> DrawStrokePreviewEnumerable(ChunkyImage target, RenderContext context, int maxSize,
        VecD shift = default)
    {
        if (previewVectorPath == null)
        {
            previewVectorPath = VectorPath.FromSvgPath(PreviewSvg);
        }

        List<RecordedPoint> points = new();

        float offset = 0;
        float pressure;
        VecD pos;
        previewEngine.ResetState();

        while (offset <= target.CommittedSize.X)
        {
            pressure = (float)Math.Sin((offset / target.CommittedSize.X) * Math.PI);
            var vec4D = previewVectorPath.GetPositionAndTangentAtDistance(offset, false);
            pos = vec4D.XY;
            pos = new VecD(pos.X, pos.Y + maxSize / 2f) + shift;
            points.Add(new RecordedPoint((VecI)pos, new PointerInfo(pos, pressure, 0, VecD.Zero, vec4D.ZW, true, false),
                new KeyboardInfo(), new EditorData(Colors.White, Colors.Black)));

            previewEngine.ExecuteBrush(target,
                new BrushData(context.Graph, Id) { StrokeWidth = maxSize, AntiAliasing = true },
                points, context.FrameTime, context.ProcessingColorSpace, context.DesiredSamplingOptions);
            offset += 1;
            yield return offset;
        }
    }

    public void DrawPointPreview(ChunkyImage img, RenderContext context, int size, VecD pos)
    {
        previewEngine.ResetState();
        previewEngine.ExecuteBrush(img,
            new BrushData(context.Graph, Id) { StrokeWidth = size, AntiAliasing = true },
            pos, context.FrameTime, context.ProcessingColorSpace, context.DesiredSamplingOptions,
            new PointerInfo(pos, 1, 0, VecD.Zero, new VecD(0, 1), true, false),
            new KeyboardInfo(),
            new EditorData(Colors.White, Colors.Black));
    }

    public override Node CreateCopy()
    {
        return new BrushOutputNode();
    }

    public override void Dispose()
    {
        previewEngine.Dispose();
        previewChunkyImage?.Dispose();
        previewChunkyImage = null;

        previewVectorPath?.Dispose();
        previewVectorPath = null;
        cache.Dispose();
        base.Dispose();
    }
}
