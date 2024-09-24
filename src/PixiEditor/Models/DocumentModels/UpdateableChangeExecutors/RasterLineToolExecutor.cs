using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Numerics;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class RasterLineToolExecutor : LineExecutor<ILineToolHandler>
{
    protected override bool InitShapeData(IReadOnlyLineData? data)
    {
        return false;
    }

    protected override IAction DrawLine(VecD pos)
    {
        return new DrawRasterLine_Action(memberId, (VecI)startDrawingPos.Floor(), (VecI)pos.Floor(), StrokeWidth,
            StrokeColor, StrokeCap.Butt, drawOnMask, document!.AnimationHandler.ActiveFrameBindable);
    }

    protected override IAction TransformOverlayMoved(VecD start, VecD end)
    {
        return new DrawRasterLine_Action(memberId, (VecI)start, (VecI)end,
            StrokeWidth, StrokeColor, StrokeCap.Butt, drawOnMask, document!.AnimationHandler.ActiveFrameBindable);
    }

    protected override IAction SettingsChange()
    {
        return new DrawRasterLine_Action(memberId, (VecI)startDrawingPos.Floor(), (VecI)curPos.Floor(), StrokeWidth,
            StrokeColor, StrokeCap.Butt, drawOnMask, document!.AnimationHandler.ActiveFrameBindable);
    }

    protected override IAction EndDraw()
    {
        return new EndDrawRasterLine_Action();
    }
}
