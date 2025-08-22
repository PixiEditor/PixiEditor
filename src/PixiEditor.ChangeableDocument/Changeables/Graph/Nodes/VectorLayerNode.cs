﻿using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.Vectors;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("VectorLayer")]
public class VectorLayerNode : LayerNode, ITransformableObject, IReadOnlyVectorNode, IRasterizable, IScalable
{
    public InputProperty<ShapeVectorData> InputVector { get; }
    public OutputProperty<ShapeVectorData> Shape { get; }
    public OutputProperty<Matrix3X3> Matrix { get; }

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

    public ShapeVectorData? EmbeddedShapeData
    {
        get => Shape.Value;
        set => Shape.Value = value;
    }

    public ShapeVectorData? RenderableShapeData
    {
        get => InputVector.Value ?? EmbeddedShapeData;
    }

    IReadOnlyShapeVectorData IReadOnlyVectorNode.ShapeData => RenderableShapeData;


    public override VecD GetScenePosition(KeyFrameTime time) =>
        RenderableShapeData?.TransformedAABB.Center ?? VecD.Zero;

    public override VecD GetSceneSize(KeyFrameTime time) => RenderableShapeData?.TransformedAABB.Size ?? VecD.Zero;

    public VectorLayerNode()
    {
        AllowHighDpiRendering = true;
        InputVector = CreateInput<ShapeVectorData>("Input", "INPUT", null);
        Shape = CreateOutput<ShapeVectorData>("Shape", "SHAPE", null);
        Matrix = CreateOutput<Matrix3X3>("Matrix", "MATRIX", Matrix3X3.Identity);
    }

    protected override void OnExecute(RenderContext context)
    {
        base.OnExecute(context);
        Matrix.Value = TransformationMatrix;
    }

    protected override void DrawWithoutFilters(SceneObjectRenderContext ctx, DrawingSurface workingSurface,
        Paint paint)
    {
        if (RenderableShapeData == null)
        {
            return;
        }

        Rasterize(workingSurface, paint);
    }

    protected override void DrawWithFilters(SceneObjectRenderContext ctx, DrawingSurface workingSurface, Paint paint)
    {
        if (RenderableShapeData == null)
        {
            return;
        }

        Rasterize(workingSurface, paint);
    }

    public override RectD? GetPreviewBounds(int frame, string elementFor = "")
    {
        if (elementFor == nameof(EmbeddedMask))
        {
            base.GetPreviewBounds(frame, elementFor);
        }
        else
        {
            return RenderableShapeData?.TransformedVisualAABB;
        }

        return null;
    }

    public override bool RenderPreview(DrawingSurface renderOn, RenderContext context,
        string elementToRenderName)
    {
        if (elementToRenderName == nameof(EmbeddedMask))
        {
            return base.RenderPreview(renderOn, context, elementToRenderName);
        }

        if (RenderableShapeData == null)
        {
            return false;
        }

        using var paint = new Paint();

        VecI tightBoundsSize = (VecI)RenderableShapeData.TransformedVisualAABB.Size;

        VecI translation = new VecI(
            (int)Math.Max(RenderableShapeData.TransformedAABB.TopLeft.X, 0),
            (int)Math.Max(RenderableShapeData.TransformedAABB.TopLeft.Y, 0));

        VecI size = tightBoundsSize + translation;

        if (size.X == 0 || size.Y == 0)
        {
            return false;
        }


        int savedCount = renderOn.Canvas.Save();
        renderOn.Canvas.Scale((float)context.ChunkResolution.Multiplier());
        Rasterize(renderOn, paint);
        renderOn.Canvas.RestoreToCount(savedCount);

        return true;
    }

    public override RectD? GetApproxBounds(KeyFrameTime frameTime)
    {
        return GetTightBounds(frameTime);
    }

    public override void SerializeAdditionalData(Dictionary<string, object> additionalData)
    {
        base.SerializeAdditionalData(additionalData);
        additionalData["ShapeData"] = EmbeddedShapeData;
    }

    internal override void DeserializeAdditionalData(IReadOnlyDocument target,
        IReadOnlyDictionary<string, object> data, List<IChangeInfo> infos)
    {
        base.DeserializeAdditionalData(target, data, infos);
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

    protected override int GetContentCacheHash()
    {
        return HashCode.Combine(
            base.GetContentCacheHash(),
            EmbeddedShapeData?.GetCacheHash() ?? 0,
            RenderableShapeData?.GetCacheHash() ?? 0);
    }

    public override RectD? GetTightBounds(KeyFrameTime frameTime)
    {
        return RenderableShapeData?.TransformedVisualAABB ?? null;
    }

    public override ShapeCorners GetTransformationCorners(KeyFrameTime frameTime)
    {
        return RenderableShapeData?.TransformationCorners ?? new ShapeCorners();
    }

    public void Rasterize(DrawingSurface surface, Paint paint)
    {
        int layer;
        // TODO: This can be further optimized by passing opacity, blend mode and filters directly to the rasterization method
        if (paint != null && (paint.Color.A < 255 || paint.ColorFilter != null || paint.ImageFilter != null ||
                              paint.Shader != null ||
                              paint.BlendMode != Drawie.Backend.Core.Surfaces.BlendMode.SrcOver))
        {
            layer = surface.Canvas.SaveLayer(paint);
        }
        else
        {
            layer = surface.Canvas.Save();
        }

        RenderableShapeData?.RasterizeTransformed(surface.Canvas);

        surface.Canvas.RestoreToCount(layer);
    }

    public override Node CreateCopy()
    {
        return new VectorLayerNode()
        {
            EmbeddedShapeData = (ShapeVectorData?)EmbeddedShapeData?.Clone(),
            ClipToPreviousMember = this.ClipToPreviousMember,
            EmbeddedMask = this.EmbeddedMask?.CloneFromCommitted(),
            AllowHighDpiRendering = this.AllowHighDpiRendering
        };
    }

    public void Resize(VecD multiplier)
    {
        if (EmbeddedShapeData == null)
        {
            return;
        }

        if (EmbeddedShapeData is IScalable resizable)
        {
            resizable.Resize(multiplier);
        }
        else
        {
            EmbeddedShapeData.TransformationMatrix =
                EmbeddedShapeData.TransformationMatrix.PostConcat(Matrix3X3.CreateScale((float)multiplier.X,
                    (float)multiplier.Y));
        }
    }
}
