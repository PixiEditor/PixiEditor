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
        return new DrawRasterLine_Action(memberId, ToPixelPos(startDrawingPos), ToPixelPos(pos),
            (float)StrokeWidth,
            StrokePaintable, StrokeCap.Butt, toolbar.AntiAliasing, drawOnMask,
            document!.AnimationHandler.ActiveFrameBindable);
    }

    protected override IAction TransformOverlayMoved(VecD start, VecD end)
    {
        return new DrawRasterLine_Action(memberId, ToPixelPos(start), ToPixelPos(end),
            (float)StrokeWidth, StrokePaintable, StrokeCap.Butt, toolbar.AntiAliasing, drawOnMask,
            document!.AnimationHandler.ActiveFrameBindable);
    }

    protected override IAction[] SettingsChange(string name, object value)
    {
        return
        [
            new DrawRasterLine_Action(memberId, ToPixelPos(startDrawingPos), ToPixelPos(curPos),
                (float)StrokeWidth,
                StrokePaintable, StrokeCap.Butt, toolbar.AntiAliasing, drawOnMask,
                document!.AnimationHandler.ActiveFrameBindable)
        ];
    }

    private VecD ToPixelPos(VecD pos)
    {
        return pos;
    }

    protected override IAction EndDraw()
    {
        return new EndDrawRasterLine_Action();
    }
}
