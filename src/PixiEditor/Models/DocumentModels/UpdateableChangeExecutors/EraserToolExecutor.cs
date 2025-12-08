#nullable enable
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;
using PixiEditor.ChangeableDocument.Rendering.ContextData;
using PixiEditor.Models.Controllers.InputDevice;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal class EraserToolExecutor : BrushBasedExecutor<IEraserToolHandler>
{
    protected override void EnqueueDrawActions()
    {
        var point = GetStabilizedPoint();

        if (handler != null)
        {
            handler.LastAppliedPoint = point;
        }

        Color primaryColor = controller.EditorData.PrimaryColor.WithAlpha(0);
        EditorData data = new EditorData(primaryColor, controller.EditorData.SecondaryColor);
        var action = new LineBasedPen_Action(layerId, point, (float)ToolSize, antiAliasing,
            BrushData, drawOnMask,
            document!.AnimationHandler.ActiveFrameBindable, controller.LastPointerInfo, controller.LastKeyboardInfo, data);

        internals!.ActionAccumulator.AddActions(action);
    }
}
