using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiEditor.Models.Handlers.Tools;
using Drawie.Numerics;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class RasterLineToolExecutor : LineExecutor<ILineToolHandler>
{
    protected override bool UseGlobalUndo => false;
    protected override bool ShowApplyButton => true;

    protected override bool InitShapeData(IReadOnlyLineData? data)
    {
        return false;
    }

    protected override IAction DrawLine(VecD pos)
    {
        VecD dir = GetSignedDirection(startDrawingPos, pos);
        VecD oppositeDir = new VecD(-dir.X, -dir.Y);
        return new DrawRasterLine_Action(memberId, ToPixelPos(startDrawingPos, oppositeDir), ToPixelPos(pos, dir), (float)StrokeWidth,
            StrokePaintable, StrokeCap.Butt, toolbar.AntiAliasing, drawOnMask, document!.AnimationHandler.ActiveFrameBindable);
    }

    protected override IAction TransformOverlayMoved(VecD start, VecD end)
    {
        VecD dir = GetSignedDirection(start, end);
        VecD oppositeDir = new VecD(-dir.X, -dir.Y);
        return new DrawRasterLine_Action(memberId, ToPixelPos(start, oppositeDir), ToPixelPos(end, dir), 
            (float)StrokeWidth, StrokePaintable, StrokeCap.Butt, toolbar.AntiAliasing, drawOnMask, document!.AnimationHandler.ActiveFrameBindable);
    }

    protected override IAction[] SettingsChange(string name, object value)
    {
        VecD dir = GetSignedDirection(startDrawingPos, curPos);
        VecD oppositeDir = new VecD(-dir.X, -dir.Y);
        return [new DrawRasterLine_Action(memberId, ToPixelPos(startDrawingPos, oppositeDir), ToPixelPos(curPos, dir), (float)StrokeWidth,
            StrokePaintable, StrokeCap.Butt, toolbar.AntiAliasing, drawOnMask, document!.AnimationHandler.ActiveFrameBindable)];
    }

    private VecI ToPixelPos(VecD pos, VecD dir)
    {
        if (StrokeWidth > 1) return (VecI)pos.Round();
        
        double xAdjustment = dir.X > 0 ? 0.5 : -0.5;
        double yAdjustment = dir.Y > 0 ? 0.5 : -0.5;
        
        VecD adjustment = new VecD(xAdjustment, yAdjustment);

        
        VecI finalPos = (VecI)(pos - adjustment);

        return finalPos;
    }
    
    private VecD GetSignedDirection(VecD start, VecD end)
    {
        return new VecD(Math.Sign(end.X - start.X), Math.Sign(end.Y - start.Y));
    }

    protected override IAction EndDraw()
    {
        return new EndDrawRasterLine_Action();
    }
}
