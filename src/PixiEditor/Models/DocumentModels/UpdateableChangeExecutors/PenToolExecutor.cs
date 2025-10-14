using Avalonia.Input;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.Models.BrushEngine;
using PixiEditor.Models.Controllers.InputDevice;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class PenToolExecutor : BrushBasedExecutor<IPenToolHandler>
{
    private bool pixelPerfect;

    public override ExecutionState Start()
    {
        if (base.Start() == ExecutionState.Error)
            return ExecutionState.Error;

        var penTool = GetHandler<IPenToolHandler>();
        pixelPerfect = penTool.PixelPerfectEnabled;

        if (color.A > 0)
        {
            colorsHandler.AddSwatch(new PaletteColor(color.R, color.G, color.B));
        }

        return ExecutionState.Success;
    }

    protected override void EnqueueDrawActions()
    {
        IAction? action = pixelPerfect switch
        {
            false => new LineBasedPen_Action(layerId, controller!.LastPixelPosition, (float)ToolSize,
                antiAliasing, Spacing, BrushData, drawOnMask,
                document!.AnimationHandler.ActiveFrameBindable, controller.LastPointerInfo, controller.LastKeyboardInfo, controller.EditorData),
            true => new PixelPerfectPen_Action(layerId, controller!.LastPixelPosition, color, drawOnMask,
                document!.AnimationHandler.ActiveFrameBindable)
        };

        internals!.ActionAccumulator.AddActions(action);
    }

    protected override void EnqueueEndDraw()
    {
        IAction? action = pixelPerfect switch
        {
            false => new EndLineBasedPen_Action(),
            true => new EndPixelPerfectPen_Action()
        };

        internals!.ActionAccumulator.AddFinishedActions(action);
    }
}
