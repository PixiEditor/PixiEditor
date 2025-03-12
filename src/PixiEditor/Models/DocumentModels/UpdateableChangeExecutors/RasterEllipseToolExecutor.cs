using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.Models.Handlers;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class RasterEllipseToolExecutor : DrawableShapeToolExecutor<IRasterEllipseToolHandler>
{
    private void DrawEllipseOrCircle(VecD curPos, double rotationRad, bool firstDraw)
    {
        RectI rect;
        VecI startPos = (VecI)Snap(startDrawingPos, curPos).Floor();
        if (firstDraw)
            rect = new RectI((VecI)curPos, VecI.Zero);
        else
            rect = RectI.FromTwoPixels(startPos, (VecI)curPos);

        lastRect = (RectD)rect;
        lastRadians = rotationRad;

        internals!.ActionAccumulator.AddActions(new DrawRasterEllipse_Action(memberId, rect, rotationRad, StrokePaintable, FillPaintable, (float)StrokeWidth, toolbar.AntiAliasing, drawOnMask, document!.AnimationHandler.ActiveFrameBindable));
    }

    public override ExecutorType Type => ExecutorType.ToolLinked;
    protected override DocumentTransformMode TransformMode => DocumentTransformMode.Scale_Rotate_NoShear_NoPerspective;
    protected override bool UseGlobalUndo => false;
    protected override bool ShowApplyButton => true;
    protected override void DrawShape(VecD currentPos, double rotationRad, bool firstDraw) => DrawEllipseOrCircle(currentPos, rotationRad, firstDraw);
    protected override IAction SettingsChangedAction()
    {
        return new DrawRasterEllipse_Action(memberId, (RectI)lastRect, lastRadians, StrokePaintable, FillPaintable, (float)StrokeWidth, toolbar.AntiAliasing, drawOnMask, document!.AnimationHandler.ActiveFrameBindable);
    }

    protected override IAction TransformMovedAction(ShapeData data, ShapeCorners corners)
    {
        RectI rect = (RectI)RectD.FromCenterAndSize(data.Center, data.Size);
        double radians = corners.RectRotation;
        
        lastRect = (RectD)rect;
        lastRadians = radians;
        
        return new DrawRasterEllipse_Action(memberId, (RectI)lastRect, lastRadians, StrokePaintable,
            FillPaintable, (float)StrokeWidth, toolbar.AntiAliasing, drawOnMask, document!.AnimationHandler.ActiveFrameBindable);
    }

    protected override bool CanEditShape(IStructureMemberHandler layer)
    {
        return true;
    }

    protected override IAction EndDrawAction() => new EndDrawRasterEllipse_Action();
}
