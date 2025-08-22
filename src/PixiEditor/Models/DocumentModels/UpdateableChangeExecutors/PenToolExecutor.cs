using Avalonia.Input;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Vector;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.ViewModels.Document.Nodes.Brushes;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class PenToolExecutor : UpdateableChangeExecutor
{
    private Guid guidValue;
    private Color color;
    public double ToolSize => penToolbar.ToolSize;
    public bool SquareBrush => penToolbar.PaintShape == PaintBrushShape.Square;
    public override bool BlocksOtherActions => controller.LeftMousePressed;

    private bool drawOnMask;
    private bool pixelPerfect;
    private bool antiAliasing;
    private float spacing = 1;
    private bool transparentErase;

    private Guid brushOutputGuid = Guid.Empty;
    private INodePropertyHandler vectorShapeInput;

    private IPenToolbar penToolbar;
    private IPenToolHandler handler;

    public override ExecutionState Start()
    {
        IStructureMemberHandler? member = document!.SelectedStructureMember;
        IColorsHandler? colorsHandler = GetHandler<IColorsHandler>();

        IPenToolHandler? penTool = GetHandler<IPenToolHandler>();
        if (colorsHandler is null || penTool is null || member is null || penTool?.Toolbar is not IPenToolbar toolbar)
            return ExecutionState.Error;
        drawOnMask = member is not ILayerHandler layer || layer.ShouldDrawOnMask;
        if (drawOnMask && !member.HasMaskBindable)
            return ExecutionState.Error;
        if (!drawOnMask && member is not ILayerHandler)
            return ExecutionState.Error;

        handler = penTool;
        penToolbar = toolbar;
        guidValue = member.Id;
        color = colorsHandler.PrimaryColor;
        pixelPerfect = penTool.PixelPerfectEnabled;
        antiAliasing = toolbar.AntiAliasing;
        spacing = toolbar.Spacing;

        if (color.A > 0)
        {
            colorsHandler.AddSwatch(new PaletteColor(color.R, color.G, color.B));
        }

        brushOutputGuid =
            document.NodeGraphHandler.AllNodes.FirstOrDefault(x => x.InternalName == "PixiEditor.BrushOutput")?.Id ??
            Guid.Empty;
        if (brushOutputGuid == Guid.Empty)
            return ExecutionState.Error;

        vectorShapeInput =
            (document.NodeGraphHandler.NodeLookup[brushOutputGuid] as BrushOutputNodeViewModel).Inputs
            .FirstOrDefault(x => x.PropertyName == "VectorShape");

        transparentErase = color.A == 0;
        vectorShapeInput.UpdateComputedValue();
        if (controller.LeftMousePressed)
        {
            IAction? action = pixelPerfect switch
            {
                false => new LineBasedPen_Action(guidValue, color, controller!.LastPixelPosition, (float)ToolSize,
                    transparentErase, antiAliasing, spacing, brushOutputGuid, drawOnMask,
                    document!.AnimationHandler.ActiveFrameBindable, controller.LastPointerInfo, controller.EditorData),
                true => new PixelPerfectPen_Action(guidValue, controller!.LastPixelPosition, color, drawOnMask,
                    document!.AnimationHandler.ActiveFrameBindable)
            };
            internals!.ActionAccumulator.AddActions(action);
        }

        handler.FinalBrushShape = (vectorShapeInput.ComputedValue as IReadOnlyShapeVectorData)?.ToPath(true);

        return ExecutionState.Success;
    }

    public override void OnLeftMouseButtonDown(MouseOnCanvasEventArgs args)
    {
        base.OnLeftMouseButtonDown(args);
        IAction? action = pixelPerfect switch
        {
            false => new LineBasedPen_Action(guidValue, color, controller!.LastPixelPosition, (float)ToolSize,
                transparentErase, antiAliasing, spacing, brushOutputGuid, drawOnMask,
                document!.AnimationHandler.ActiveFrameBindable, controller.LastPointerInfo, controller.EditorData),
            true => new PixelPerfectPen_Action(guidValue, controller!.LastPixelPosition, color, drawOnMask,
                document!.AnimationHandler.ActiveFrameBindable)
        };
        internals!.ActionAccumulator.AddActions(action);
    }

    public override void OnPrecisePositionChange(MouseOnCanvasEventArgs args)
    {
        base.OnPrecisePositionChange(args);
        if (!controller.LeftMousePressed)
        {
            vectorShapeInput.UpdateComputedValue();
        }

        handler.FinalBrushShape = (vectorShapeInput.ComputedValue as IReadOnlyShapeVectorData)?.ToPath(true);
    }

    public override void OnPixelPositionChange(VecI pos, MouseOnCanvasEventArgs args)
    {
        if (controller.LeftMousePressed)
        {
            IAction? action = pixelPerfect switch
            {
                false => new LineBasedPen_Action(guidValue, color, pos, (float)ToolSize, transparentErase, antiAliasing,
                    spacing, brushOutputGuid, drawOnMask, document!.AnimationHandler.ActiveFrameBindable,
                    controller.LastPointerInfo, controller.EditorData),
                true => new PixelPerfectPen_Action(guidValue, pos, color, drawOnMask,
                    document!.AnimationHandler.ActiveFrameBindable)
            };
            internals!.ActionAccumulator.AddActions(action);
        }
    }

    public override void OnConvertedKeyDown(Key key)
    {
        base.OnConvertedKeyDown(key);
        handler.FinalBrushShape = (vectorShapeInput.ComputedValue as IReadOnlyShapeVectorData)?.ToPath(true);
    }

    public override void OnLeftMouseButtonUp(VecD argsPositionOnCanvas)
    {
        IAction? action = pixelPerfect switch
        {
            false => new EndLineBasedPen_Action(),
            true => new EndPixelPerfectPen_Action()
        };

        internals!.ActionAccumulator.AddFinishedActions(action);
    }

    public override void ForceStop()
    {
        IAction? action = pixelPerfect switch
        {
            false => new EndLineBasedPen_Action(),
            true => new EndPixelPerfectPen_Action()
        };
        internals!.ActionAccumulator.AddFinishedActions(action);
    }
}
