using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;
using PixiEditor.Models.Handlers;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal class VectorEllipseToolExecutor : DrawableShapeToolExecutor<IVectorEllipseToolHandler>
{
    public override ExecutorType Type => ExecutorType.ToolLinked;
    protected override DocumentTransformMode TransformMode => DocumentTransformMode.Scale_Rotate_Shear_NoPerspective;

    private VecD firstRadius;
    private VecD firstCenter;

    private Matrix3X3 lastMatrix = Matrix3X3.Identity;

    protected override bool AlignToPixels => false;

    protected override bool InitShapeData(IReadOnlyShapeVectorData data)
    {
        if (data is not EllipseVectorData ellipseData)
            return false;

        firstCenter = ellipseData.Center;
        firstRadius = ellipseData.Radius;
        lastMatrix = ellipseData.TransformationMatrix;

        return true;
    }

    protected override bool CanEditShape(IStructureMemberHandler layer)
    {
        IVectorLayerHandler vectorLayer = layer as IVectorLayerHandler;
        if (vectorLayer is null)
            return false;

        var shapeData = vectorLayer.GetShapeData(document.AnimationHandler.ActiveFrameTime);
        return shapeData is EllipseVectorData;
    }

    protected override bool UseGlobalUndo => true;
    protected override bool ShowApplyButton => false;

    protected override void DrawShape(VecD curPos, double rotationRad, bool firstDraw)
    {
        RectD rect;
        VecD startPos = Snap(startDrawingPos, curPos);
        if (!firstDraw)
            rect = RectD.FromTwoPoints(startPos, curPos);
        else
            rect = new RectD(curPos, VecD.Zero);

        firstCenter = rect.Center;
        firstRadius = rect.Size / 2f;

        EllipseVectorData data = new EllipseVectorData(firstCenter, firstRadius)
        {
            Stroke = StrokePaintable, FillPaintable = FillPaintable, StrokeWidth = (float)StrokeWidth,
        };

        lastRect = rect;

        internals!.ActionAccumulator.AddActions(new SetShapeGeometry_Action(memberId, data));
    }

    protected override IAction SettingsChangedAction()
    {
        return new SetShapeGeometry_Action(memberId,
            new EllipseVectorData(firstCenter, firstRadius)
            {
                Stroke = StrokePaintable,
                FillPaintable = FillPaintable,
                StrokeWidth = (float)StrokeWidth,
                TransformationMatrix = lastMatrix
            });
    }

    protected override IAction TransformMovedAction(ShapeData data, ShapeCorners corners)
    {
        RectD rect = RectD.FromCenterAndSize(data.Center, data.Size);
        RectD firstRect = RectD.FromCenterAndSize(firstCenter, firstRadius * 2);

        Matrix3X3 matrix = Matrix3X3.Identity;
        if (corners.IsRect)
        {
            firstCenter = corners.RectCenter;
            firstRadius = corners.RectSize / 2f;

            if (corners.RectRotation != 0)
                matrix = Matrix3X3.CreateRotation((float)corners.RectRotation, (float)firstCenter.X,
                    (float)firstCenter.Y);
        }
        else
        {
            matrix = OperationHelper.CreateMatrixFromPoints(corners, firstRadius * 2);
            matrix = matrix.Concat(
                Matrix3X3.CreateTranslation(-(float)firstRect.TopLeft.X, -(float)firstRect.TopLeft.Y));
        }

        EllipseVectorData ellipseData = new EllipseVectorData(firstCenter, firstRadius)
        {
            Stroke = StrokePaintable,
            FillPaintable = FillPaintable,
            StrokeWidth = (float)StrokeWidth,
            TransformationMatrix = matrix
        };

        lastRect = rect;
        lastMatrix = matrix;

        return new SetShapeGeometry_Action(memberId, ellipseData);
    }

    protected override IAction EndDrawAction()
    {
        return new EndSetShapeGeometry_Action();
    }

    public override bool IsFeatureEnabled(IExecutorFeature feature)
    {
        if(feature is IMidChangeUndoableExecutor) return false;
        return base.IsFeatureEnabled(feature);
    }
}
