#nullable enable
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Numerics;
using PixiEditor.Helpers;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.ChangeableDocument.Rendering.ContextData;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal class EraserToolExecutor : BrushBasedExecutor<IEraserToolHandler>
{
    protected override void EnqueueDrawActions(bool createLine, bool snapLineTo45Incremented = false)
    {
        var point = GetStabilizedPoint();
        point = ApplyPixelCenterSnapping(point);

        Color primaryColor = controller.EditorData.PrimaryColor.WithAlpha(0);
        EditorData data = new EditorData(primaryColor, controller.EditorData.SecondaryColor);
        createLine = createLine && document.LastBrushToolUseEndPositionBindable is not null;
        if (createLine && document.LastBrushToolUseEndPositionBindable is VecD lastToolUsePosition)
        {
            if (snapLineTo45Incremented)
                point = ApplyStraightLineSnappingTo45(point, lastToolUsePosition);

            VecD prevPoint = snapToCentersOfPixels ? lastToolUsePosition.Floor() + new VecD(0.5) : lastToolUsePosition;
            IAction actionStart = new LineBasedPen_Action(layerId, prevPoint, (float)ToolSize,
                antiAliasing, BrushData, drawOnMask,
                document!.AnimationHandler.ActiveFrameBindable, controller.LastPointerInfo, controller.LastKeyboardInfo,
                data);

            internals!.ActionAccumulator.AddActions(actionStart);
        }

        var action = new LineBasedPen_Action(layerId, point, (float)ToolSize, antiAliasing,
            BrushData, drawOnMask,
            document!.AnimationHandler.ActiveFrameBindable, controller.LastPointerInfo, controller.LastKeyboardInfo,
            data);

        if (handler != null)
        {
            handler.LastAppliedPoint = point;
        }
        
        internals!.ActionAccumulator.AddActions(action);
        
        if (createLine)
        {
            EnqueueEndDraw();
        }
    }
}
