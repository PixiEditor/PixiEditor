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

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal class VectorRectangleToolExecutor : DrawableShapeToolExecutor<IVectorRectangleToolHandler>
{
    public override ExecutorType Type => ExecutorType.ToolLinked;
    protected override DocumentTransformMode TransformMode => DocumentTransformMode.Scale_Rotate_Shear_NoPerspective;

    private VecD firstSize;
    private VecD firstCenter;

    private Matrix3X3 lastMatrix = Matrix3X3.Identity;

    protected override bool InitShapeData(IReadOnlyShapeVectorData data)
    {
        if (data is not RectangleVectorData rectData)
            return false;
        
        firstCenter = rectData.Center;
        firstSize = rectData.Size;
        lastMatrix = rectData.TransformationMatrix;
        return true;
    }

    protected override void DrawShape(VecI curPos, double rotationRad, bool firstDraw)
    {
        RectI rect;
        VecI startPos = (VecI)Snap(startDrawingPos, curPos).Floor();
        if (firstDraw)
            rect = new RectI(curPos, VecI.Zero);
        /*else if (toolViewModel!.DrawSquare)
            rect = GetSquaredCoordinates(startPos, curPos);
        else*/
            rect = RectI.FromTwoPixels(startPos, curPos);

        firstCenter = rect.Center;
        firstSize = rect.Size;

        RectangleVectorData data = new RectangleVectorData(firstCenter, firstSize)
        {
            StrokeColor = StrokeColor, FillColor = FillColor, StrokeWidth = StrokeWidth,
        };

        lastRect = rect;

        internals!.ActionAccumulator.AddActions(new SetShapeGeometry_Action(memberId, data));
    }

    protected override IAction SettingsChangedAction()
    {
        return new SetShapeGeometry_Action(memberId,
            new RectangleVectorData(firstCenter, firstSize)
            {
                StrokeColor = StrokeColor, FillColor = FillColor, StrokeWidth = StrokeWidth,
                TransformationMatrix = lastMatrix
            });
    }

    protected override IAction TransformMovedAction(ShapeData data, ShapeCorners corners)
    {
        if (firstCenter == default || firstSize == default)
        {
            firstCenter = data.Center;
            firstSize = data.Size;
        }

        RectD firstRect = RectD.FromCenterAndSize(firstCenter, firstSize);
        Matrix3X3 matrix = OperationHelper.CreateMatrixFromPoints(corners, firstSize);
        matrix = matrix.Concat(Matrix3X3.CreateTranslation(-(float)firstRect.TopLeft.X, -(float)firstRect.TopLeft.Y));

        RectangleVectorData newData = new RectangleVectorData(firstCenter, firstSize)
        {
            StrokeColor = data.StrokeColor, FillColor = data.FillColor, StrokeWidth = data.StrokeWidth,
            TransformationMatrix = matrix
        };

        lastMatrix = matrix;

        return new SetShapeGeometry_Action(memberId, newData);
    }

    protected override IAction EndDrawAction()
    {
        return new EndSetShapeGeometry_Action();
    }
}
