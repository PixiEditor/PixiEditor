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
        point = ApplyPixelCenterSnappingAndStraightLineSnapping(point, createLine && snapLineTo45Incremented);
        
        Color primaryColor = controller.EditorData.PrimaryColor.WithAlpha(0);
        EditorData data = new EditorData(primaryColor, controller.EditorData.SecondaryColor);
        if (createLine)
        {
            VecD prevPoint = snapToCentersOfPixels ? handler.LastAppliedPoint.Floor() + new VecD(0.5) : handler.LastAppliedPoint;
            IAction? actionStart = new LineBasedPen_Action(layerId, prevPoint, (float)ToolSize,
                antiAliasing, BrushData, drawOnMask,
                document!.AnimationHandler.ActiveFrameBindable, controller.LastPointerInfo, controller.LastKeyboardInfo,
                data);

            internals!.ActionAccumulator.AddActions(actionStart);
        }

        if (handler != null)
        {
            handler.LastAppliedPoint = point;
        }

        var action = new LineBasedPen_Action(layerId, point, (float)ToolSize, antiAliasing,
            BrushData, drawOnMask,
            document!.AnimationHandler.ActiveFrameBindable, controller.LastPointerInfo, controller.LastKeyboardInfo,
            data);

        internals!.ActionAccumulator.AddActions(action);
    }
}
