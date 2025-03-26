using ChunkyImageLib.Operations;
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
    public OutputProperty<ShapeVectorData> Shape { get; }

    public Matrix3X3 TransformationMatrix
    {
        get => ShapeData?.TransformationMatrix ?? Matrix3X3.Identity;
        set
        {
            if (ShapeData == null)
            {
                return;
            }

            ShapeData.TransformationMatrix = value;
        }
    }

    public ShapeVectorData? ShapeData
    {
        get => Shape.Value;
        set => Shape.Value = value;
    }

    IReadOnlyShapeVectorData IReadOnlyVectorNode.ShapeData => ShapeData;


    public override VecD GetScenePosition(KeyFrameTime time) => ShapeData?.TransformedAABB.Center ?? VecD.Zero;
    public override VecD GetSceneSize(KeyFrameTime time) => ShapeData?.TransformedAABB.Size ?? VecD.Zero;

    public VectorLayerNode()
    {
        AllowHighDpiRendering = true;
        Shape = CreateOutput<ShapeVectorData>("Shape", "SHAPE", null);
    }

    protected override void DrawWithoutFilters(SceneObjectRenderContext ctx, DrawingSurface workingSurface,
        Paint paint)
    {
        if (ShapeData == null)
        {
            return;
        }

        Rasterize(workingSurface, paint);
    }

    protected override void DrawWithFilters(SceneObjectRenderContext ctx, DrawingSurface workingSurface, Paint paint)
    {
        if (ShapeData == null)
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
            return ShapeData?.TransformedVisualAABB;
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

        if (ShapeData == null)
        {
            return false;
        }

        using var paint = new Paint();

        VecI tightBoundsSize = (VecI)ShapeData.TransformedVisualAABB.Size;

        VecI translation = new VecI(
            (int)Math.Max(ShapeData.TransformedAABB.TopLeft.X, 0),
            (int)Math.Max(ShapeData.TransformedAABB.TopLeft.Y, 0));

        VecI size = tightBoundsSize + translation;

        if (size.X == 0 || size.Y == 0)
        {
            return false;
        }

        Matrix3X3 matrix = ShapeData.TransformationMatrix;

        if (!context.ProcessingColorSpace.IsSrgb)
        {
            int saved = renderOn.Canvas.Save();
            Texture tex = Texture.ForProcessing(renderOn, ColorSpace.CreateSrgb());
            renderOn.Canvas.SetMatrix(Matrix3X3.Identity);
            Rasterize(tex.DrawingSurface, paint);
            renderOn.Canvas.DrawSurface(tex.DrawingSurface, 0, 0);
            renderOn.Canvas.RestoreToCount(saved);
        }
        else
        {
            Rasterize(renderOn, paint);
        }

        return true;
    }

    public override void SerializeAdditionalData(Dictionary<string, object> additionalData)
    {
        base.SerializeAdditionalData(additionalData);
        additionalData["ShapeData"] = ShapeData;
    }

    internal override void DeserializeAdditionalData(IReadOnlyDocument target,
        IReadOnlyDictionary<string, object> data, List<IChangeInfo> infos)
    {
        base.DeserializeAdditionalData(target, data, infos);
        ShapeData = (ShapeVectorData)data["ShapeData"];

        if (ShapeData == null)
        {
            return;
        }

        var affected = new AffectedArea(OperationHelper.FindChunksTouchingRectangle(
            (RectI)ShapeData.TransformedAABB, ChunkyImage.FullChunkSize));

        infos.Add(new VectorShape_ChangeInfo(Id, affected));
    }

    protected override int GetContentCacheHash()
    {
        return HashCode.Combine(base.GetContentCacheHash(), ShapeData?.GetCacheHash() ?? 0);
    }

    public override RectD? GetTightBounds(KeyFrameTime frameTime)
    {
        return ShapeData?.TransformedVisualAABB ?? null;
    }

    public override ShapeCorners GetTransformationCorners(KeyFrameTime frameTime)
    {
        return ShapeData?.TransformationCorners ?? new ShapeCorners();
    }

    public void Rasterize(DrawingSurface surface, Paint paint)
    {
        int layer = surface.Canvas.SaveLayer(paint);
        ShapeData?.RasterizeTransformed(surface.Canvas);

        surface.Canvas.RestoreToCount(layer);
    }

    public override Node CreateCopy()
    {
        return new VectorLayerNode()
        {
            ShapeData = (ShapeVectorData?)ShapeData?.Clone(),
            ClipToPreviousMember = this.ClipToPreviousMember,
            EmbeddedMask = this.EmbeddedMask?.CloneFromCommitted(),
            AllowHighDpiRendering = this.AllowHighDpiRendering
        };
    }

    public void Resize(VecD multiplier)
    {
        if (ShapeData == null)
        {
            return;
        }

        if(ShapeData is IScalable resizable)
        {
            resizable.Resize(multiplier);
        }
        else
        {
            ShapeData.TransformationMatrix =
                ShapeData.TransformationMatrix.PostConcat(Matrix3X3.CreateScale((float)multiplier.X,
                    (float)multiplier.Y));
        }
    }
}
