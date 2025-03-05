using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.Models.Handlers;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class RasterRectangleToolExecutor : DrawableShapeToolExecutor<IRasterRectangleToolHandler>
{
    private ShapeData lastData;
    public override ExecutorType Type => ExecutorType.ToolLinked;

    private void DrawRectangle(VecD curPos, double rotationRad, bool firstDraw)
    {
        RectI rect;
        VecI startPos = (VecI)Snap(startDrawingPos, curPos).Floor();
        if (firstDraw)
            rect = new RectI((VecI)curPos, VecI.Zero);
        else
            rect = RectI.FromTwoPixels(startPos, (VecI)curPos);
        
        lastRect = (RectD)rect;
        lastRadians = rotationRad;

        lastData = new ShapeData(rect.Center, rect.Size, rotationRad, (float)StrokeWidth, StrokePaintable, FillPaintable)
        {
            AntiAliasing = toolbar.AntiAliasing
        };

        internals!.ActionAccumulator.AddActions(new DrawRasterRectangle_Action(memberId, lastData, drawOnMask,
            document!.AnimationHandler.ActiveFrameBindable));
    }

    protected override bool UseGlobalUndo => false;
    protected override bool ShowApplyButton => true;

    protected override void DrawShape(VecD currentPos, double rotationRad, bool first) =>
        DrawRectangle(currentPos, rotationRad, first);

    protected override IAction SettingsChangedAction()
    {
        lastData = new ShapeData(lastData.Center, lastData.Size, lastRadians, (float)StrokeWidth, StrokePaintable, FillPaintable)
        {
            AntiAliasing = toolbar.AntiAliasing
        };
        return new DrawRasterRectangle_Action(memberId, lastData, drawOnMask,
            document!.AnimationHandler.ActiveFrameBindable);
    }

    protected override IAction TransformMovedAction(ShapeData data, ShapeCorners corners)
    {
        lastData = data;

        lastRadians = corners.RectRotation;

        return new DrawRasterRectangle_Action(memberId, data, drawOnMask,
            document!.AnimationHandler.ActiveFrameBindable);
    }

    protected override bool CanEditShape(IStructureMemberHandler layer)
    {
        return true;
    }

    protected override IAction EndDrawAction() => new EndDrawRasterRectangle_Action();
}
