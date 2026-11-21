using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.Vectors;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Numerics;


namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("VectorLayer")]
public class VectorLayerNode : LayerNode, ITransformableObject, IReadOnlyVectorNode, IRasterizable, IScalable,
    IQuickColorChangeable, ICelLayer
{
    public const string VectorLayerKey = "LayerVector";
    public InputProperty<ShapeVectorData> InputVector { get; }
    public OutputProperty<ShapeVectorData> Shape { get; }
    public OutputProperty<Matrix3X3> Matrix { get; }

    public string CelDataKey => VectorLayerKey;
    public object? CreateEmptyCelData(VecI documentSize) => null;
    public object? CloneCelDataAtFrame(int frame) => GetShapeAtFrame(frame)?.Clone();
    public object? CloneCelData(KeyFrameData cel) => (cel.Data as ShapeVectorData)?.Clone();
    public bool FallbackAnimationToLayerImage { get; set; } = false;
    public Matrix3X3 TransformationMatrix
    {
        get => RenderableShapeData?.TransformationMatrix ?? Matrix3X3.Identity;
        set
        {
            if (RenderableShapeData == null)
            {
                return;
            }

            RenderableShapeData.TransformationMatrix = value;
        }
    }

    public Matrix3X3 GetTransformationMatrix(int frame)
    {
        return GetRenderableShapeData(frame)?.TransformationMatrix ?? Matrix3X3.Identity;
    }

    public void SetTransformationMatrix(int frame, Matrix3X3 matrix)
    {
        var shape = GetRenderableShapeData(frame);
        if (shape == null)
        {
            return;
        }

        shape.TransformationMatrix = matrix;
    }

    public Matrix3X3[] GetAllTransformationMatrices()
    {
        return keyFrames.Select(x => (x.Data as ShapeVectorData)?.TransformationMatrix ?? Matrix3X3.Identity).ToArray();
    }

    public void SetAllTransformationMatrices(Matrix3X3[] matrices)
    {
        for (int i = 0; i < keyFrames.Count && i < matrices.Length; i++)
        {
            if (keyFrames[i].Data is ShapeVectorData shape)
            {
                shape.TransformationMatrix = matrices[i];
            }
        }
    }

    public ShapeVectorData? EmbeddedShapeData
    {
        get => (ShapeVectorData)keyFrames[0].Data;
        set => SetCelShape(keyFrames[0], value);
    }

    public ShapeVectorData? RenderableShapeData
    {
        get => InputVector.Value ?? EmbeddedShapeData;
    }

    public ShapeVectorData? GetRenderableShapeData(KeyFrameTime frame)
    {
        return InputVector.Value ?? GetShapeAtFrame(frame.Frame);
    }

    IReadOnlyShapeVectorData IReadOnlyVectorNode.ShapeData => RenderableShapeData;

    IReadOnlyShapeVectorData? IReadOnlyVectorNode.GetShapeData(KeyFrameTime frame) => GetRenderableShapeData(frame);


    public override VecD GetScenePosition(KeyFrameTime time) =>
        GetRenderableShapeData(time)?.TransformedAABB.Center ?? VecD.Zero;

    public override VecD GetSceneSize(KeyFrameTime time) => GetRenderableShapeData(time)?.TransformedAABB.Size ?? VecD.Zero;

    public VectorLayerNode()
    {
        AllowHighDpiRendering = true;
        InputVector = CreateInput<ShapeVectorData>("Input", "INPUT", null);
        Shape = CreateOutput<ShapeVectorData>("Shape", "SHAPE", null);
        Matrix = CreateOutput<Matrix3X3>("Matrix", "MATRIX", Matrix3X3.Identity);
        if (keyFrames.Count == 0)
        {
            keyFrames.Add(
                new KeyFrameData(Guid.NewGuid(), 0, 0, VectorLayerKey) { Data = null });
        }
    }

    protected override void OnExecute(RenderContext context)
    {
        Shape.Value = GetShapeAtFrame(context.FrameTime.Frame);
        base.OnExecute(context);
        Matrix.Value = GetRenderableShapeData(context.FrameTime)?.TransformationMatrix ?? Matrix3X3.Identity;
    }

    protected override bool MustRenderInSrgb(SceneObjectRenderContext ctx)
    {
        return GetShapeAtFrame(ctx.FrameTime.Frame) is { FillPaintable: GradientPaintable } or { Stroke: GradientPaintable };
    }

    protected override void DrawWithoutFilters(SceneObjectRenderContext ctx, Canvas workingSurface,
        Paint paint)
    {
        if (GetRenderableShapeData(ctx.FrameTime.Frame) == null)
        {
            return;
        }

        if (!ctx.IterativeRender || ctx.AffectedArea.GlobalArea.HasValue)
        {
            Rasterize(workingSurface, paint, ctx.FrameTime.Frame);
        }
    }

    protected override void DrawWithFilters(SceneObjectRenderContext ctx, Canvas workingSurface, Paint paint)
    {
        if (GetRenderableShapeData(ctx.FrameTime.Frame) == null)
        {
            return;
        }

        if (!ctx.IterativeRender || ctx.AffectedArea.GlobalArea.HasValue)
        {
            Rasterize(workingSurface, paint, ctx.FrameTime.Frame);
        }
    }


    protected override bool ShouldRenderPreview(string elementToRenderName)
    {
        if (elementToRenderName == nameof(EmbeddedMask))
        {
            return base.ShouldRenderPreview(elementToRenderName);
        }

        return true;
    }

    public override void RenderPreview(DrawingSurface renderOn, RenderContext context, string elementToRenderName)
    {
        if (elementToRenderName == nameof(EmbeddedMask))
        {
            base.RenderPreview(renderOn, context, elementToRenderName);
            return;
        }
        var shape = GetPreviewShape(context.FrameTime, elementToRenderName);

        if (shape == null)
        {
            return;
        }

        using var paint = new Paint();

        VecI tightBoundsSize = (VecI)shape.TransformedVisualAABB.Size;

        VecI translation = new VecI(
            (int)Math.Max(shape.TransformedAABB.TopLeft.X, 0),
            (int)Math.Max(shape.TransformedAABB.TopLeft.Y, 0));

        VecI size = tightBoundsSize + translation;

        if (size.X == 0 || size.Y == 0)
        {
            return;
        }

        RasterizeShape(renderOn.Canvas, paint, shape);
    }

    private KeyFrameData? GetFrameWithShape(KeyFrameTime frame)
    {
        if (keyFrames.Count == 1 || frame.Frame == 0)
        {
            return keyFrames[0];
        }

        var shapeFrame = keyFrames.OrderBy(x => x.StartFrame).LastOrDefault(x => x.IsInFrame(frame.Frame));
        if (shapeFrame == null)
        {
            if (FallbackAnimationToLayerImage)
            {
                return keyFrames[0];
            }
            return keyFrames.ElementAtOrDefault(1).IsVisible ? null : keyFrames[0];
        }
        return shapeFrame;
    }

    private ShapeVectorData? GetPreviewShape(KeyFrameTime frame, string elementToRenderName)
    {
        if (Guid.TryParse(elementToRenderName, out Guid guid))
        {
            if (guid == Id)
            {
                return EmbeddedShapeData;
            }

            var keyFrame = keyFrames.FirstOrDefault(x => x.KeyFrameGuid == guid);
            if (keyFrame != null)
            {
                return keyFrame.Data as ShapeVectorData;
            }
        }
        return GetRenderableShapeData(frame);
    }

    public override RectD? GetPreviewBounds(RenderContext ctx, string elementToRenderName)
    {
        if (elementToRenderName == nameof(EmbeddedMask))
        {
            return base.GetPreviewBounds(ctx, elementToRenderName);
        }

        return GetPreviewShape(ctx.FrameTime, elementToRenderName)?.TransformedVisualAABB;
    }

    public override RectD? GetApproxBounds(KeyFrameTime frameTime)
    {
        return GetTightBounds(frameTime);
    }

    internal override void SerializeAdditionalDataInternal(IReadOnlyDocument target,
        Dictionary<string, object> additionalData)
    {
        base.SerializeAdditionalDataInternal(target, additionalData);
        additionalData["ShapeData"] = EmbeddedShapeData;
    }

    internal override void DeserializeAdditionalDataInternal(IReadOnlyDocument target,
        IReadOnlyDictionary<string, object> data, List<IChangeInfo> infos)
    {
        base.DeserializeAdditionalDataInternal(target, data, infos);
        EmbeddedShapeData = data["ShapeData"] as ShapeVectorData;

        if (EmbeddedShapeData == null)
        {
            Console.WriteLine("Failed to deserialize shape data");
            return;
        }

        var affected = new AffectedArea(OperationHelper.FindChunksTouchingRectangle(
            (RectI)EmbeddedShapeData.TransformedAABB, ChunkyImage.FullChunkSize));

        infos.Add(new VectorShape_ChangeInfo(Id, affected));
    }

    public ShapeVectorData? GetShapeAtFrame(int frame)
    {
        return GetFrameWithShape(frame)?.Data as ShapeVectorData;
    }

    public bool CanEditShapeAtFrame(int frame)
    {
        return GetFrameWithShape(frame) != null;
    }

    public void SetShapeAtFrame(int frame, ShapeVectorData? shape)
    {
        var shapeFrame = GetFrameWithShape(frame);
        if (shapeFrame != null)
        {
            SetCelShape(shapeFrame, shape);
        }
    }

    public void SetOnlyShapeAtFrame(int frame, ShapeVectorData? shape)
    {
        var targ = GetFrameWithShape(frame);
        foreach (var keyFrame in keyFrames)
        {
            SetCelShape(keyFrame, (keyFrame == targ)? shape : null);
        }
    }

    private void SetCelShape(KeyFrameData keyFrame, ShapeVectorData? shape)
    {
        if (keyFrame.Data != null && ReferenceEquals(Shape.Value, keyFrame.Data))
        {
            Shape.Value = shape;
        }

        keyFrame.Data = shape;
    }

    protected override int GetContentCacheHash()
    {
        return HashCode.Combine(
            base.GetContentCacheHash(),
            EmbeddedShapeData?.GetCacheHash() ?? 0,
            RenderableShapeData?.GetCacheHash() ?? 0);
    }

    public override RectD? GetTightBounds(KeyFrameTime frameTime)
    {
        return GetRenderableShapeData(frameTime)?.TransformedVisualAABB ?? null;
    }

    public override bool SupportsIterativeRendering => true;

    public override ShapeCorners GetTransformationCorners(KeyFrameTime frameTime)
    {
        return GetRenderableShapeData(frameTime)?.TransformationCorners ?? new ShapeCorners();
    }

    public void Rasterize(Canvas surface, Paint paint, int frame)
    {
        RasterizeShape(surface, paint, GetRenderableShapeData(frame));
    }

    public void RasterizeKeyFrame(Canvas surface, Paint paint, KeyFrameData keyframe)
    {
        RasterizeShape(surface, paint, InputVector.Value ?? keyframe.Data as ShapeVectorData);
    }
    private static void RasterizeShape(Canvas surface, Paint paint, ShapeVectorData? shape)
    {
        int layer;
        // TODO: This can be further optimized by passing opacity, blend mode and filters directly to the rasterization method
        if (paint is { IsOpaqueStandardNonBlendingPaint: false })
        {
            layer = surface.SaveLayer(paint);
        }
        else
        {
            layer = surface.Save();
        }

        shape?.RasterizeTransformed(surface);

        surface.RestoreToCount(layer);
    }

    public override Node CreateCopy()
    {
        var copy = new VectorLayerNode()
        {
            ClipToPreviousMember = this.ClipToPreviousMember,
            EmbeddedMask = this.EmbeddedMask?.CloneFromCommitted(),
            AllowHighDpiRendering = this.AllowHighDpiRendering,
            IsLocked = this.IsLocked,
            FallbackAnimationToLayerImage = this.FallbackAnimationToLayerImage,
        };
        copy.keyFrames.Clear();
        return copy;
    }

    public void ForEveryFrame(Action<ShapeVectorData> action)
    {
        foreach (var frame in keyFrames)
        {
            if (frame.Data is ShapeVectorData shape)
            {
                action(shape);
            }
        }
    }
    public void Resize(VecD multiplier)
    {
        ForEveryFrame(shape =>
        {
            if (shape is IScalable resizable)
            {
                resizable.Resize(multiplier);
            } else
            {
                shape.TransformationMatrix = shape.TransformationMatrix.PostConcat(
                    Matrix3X3.CreateScale((float)multiplier.X, (float)multiplier.Y));
            }
        });
    }

    public IChangeInfo[] ChangeColor(params Color?[] colors) => ChangeColor(0, colors);

    public Color?[] GetColors() => GetColors(0);

    public IChangeInfo[] ChangeColor(int frame, params Color?[] colors)
    {
        if (colors == null) return null;

        var shapeData = GetShapeAtFrame(frame);

        if (shapeData is null) return null;

        if (shapeData is TextVectorData textVectorData)
        {
            if (!UpdateTextVectorData(textVectorData, colors))
            {
                return null;
            }
        }
        else
        {
            if (colors.Length == 1)
            {
                var color = colors[0];
                bool fillChange = shapeData is
                                      { Fill: true, FillPaintable: ColorPaintable { AnythingVisible: true } cp } &&
                                  cp.Color != color;
                bool strokeChange =
                    shapeData is { StrokeWidth: > 0, Stroke: ColorPaintable { AnythingVisible: true } cp2 } &&
                    cp2.Color != color;

                if (!fillChange && !strokeChange) return null;

                if (fillChange)
                {
                    shapeData.FillPaintable = color == null ? null : new ColorPaintable(color.Value);
                }

                if (strokeChange)
                {
                    shapeData.Stroke = color == null ? null : new ColorPaintable(color.Value);
                }
            }
            else if (colors.Length == 2)
            {
                shapeData.FillPaintable = colors[0] == null ? null : new ColorPaintable(colors[0].Value);
                shapeData.Stroke = colors[1] == null ? null : new ColorPaintable(colors[1].Value);
            }
        }

        return
        [
            new VectorShape_ChangeInfo(Id,
                new AffectedArea(OperationHelper.FindChunksTouchingRectangle((RectI)shapeData.TransformedAABB,
                    ChunkyImage.FullChunkSize)))
        ];
    }

    private bool UpdateTextVectorData(TextVectorData textVectorData, Color?[] colors)
    {
        bool anySucceeded = colors.Length == 2;
        foreach (var textInline in textVectorData.Text.Inlines)
        {
            if (colors.Length == 1)
            {
                var color = colors[0];
                bool fillChange = textInline is
                                      { Fill: true, FillPaintable: ColorPaintable { AnythingVisible: true } cp } &&
                                  cp.Color != color;
                bool strokeChange =
                    textInline is { StrokeWidth: > 0, StrokePaintable: ColorPaintable { AnythingVisible: true } cp2 } &&
                    cp2.Color != color;

                if (!fillChange && !strokeChange)
                {
                    continue;
                }

                anySucceeded = true;

                if (fillChange)
                {
                    textInline.FillPaintable = color == null ? null : new ColorPaintable(color.Value);
                }

                if (strokeChange)
                {
                    textInline.StrokePaintable = color == null ? null : new ColorPaintable(color.Value);
                }
            }
            else if (colors.Length == 2)
            {
                textInline.FillPaintable = colors[0] == null ? null : new ColorPaintable(colors[0].Value);
                textInline.StrokePaintable = colors[1] == null ? null : new ColorPaintable(colors[1].Value);
            }
        }

        return anySucceeded;
    }

    public Color?[] GetColors(int frame)
    {
        var shapeData = GetShapeAtFrame(frame);

        if (shapeData == null) return null;

        Color? fillColor = shapeData.FillPaintable is ColorPaintable cp ? cp.Color : null;
        Color? strokeColor = shapeData.Stroke is ColorPaintable cp2 ? cp2.Color : null;

        return [fillColor, strokeColor];
    }
}
