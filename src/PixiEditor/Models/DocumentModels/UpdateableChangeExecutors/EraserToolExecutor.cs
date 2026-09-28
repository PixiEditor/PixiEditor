#nullable enable
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using Drawie.Backend.Core.ColorsImpl;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.ChangeableDocument.Rendering.ContextData;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal class EraserToolExecutor : BrushBasedExecutor<IEraserToolHandler>
{
    protected override void EnqueueDrawActions(bool createLine)
    {
        var point = GetStabilizedPoint();
        Color primaryColor = controller.EditorData.PrimaryColor.WithAlpha(0);
        EditorData data = new EditorData(primaryColor, controller.EditorData.SecondaryColor);
        if (createLine)
        {
            IAction? actionStart = new LineBasedPen_Action(layerId, handler.LastAppliedPoint, (float)ToolSize,
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
