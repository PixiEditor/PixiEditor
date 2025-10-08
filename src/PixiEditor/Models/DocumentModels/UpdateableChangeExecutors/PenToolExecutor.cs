using Avalonia.Input;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
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
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.Models.BrushEngine;
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
    public float Spacing => penToolbar.Spacing;
    public BrushData BrushData => brushData ??= GetBrushFromToolbar(penToolbar);
    public override bool BlocksOtherActions => controller.LeftMousePressed;

    private bool drawOnMask;
    private bool pixelPerfect;
    private bool antiAliasing;
    private bool transparentErase;

    private InputProperty<ShapeVectorData> vectorShapeInput;

    private IPenToolbar penToolbar;
    private IPenToolHandler handler;

    private BrushData? brushData;
    private Guid brushOutputGuid = Guid.Empty;

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

        if (color.A > 0)
        {
            colorsHandler.AddSwatch(new PaletteColor(color.R, color.G, color.B));
        }

        if (toolbar.Brush == null)
            return ExecutionState.Error;

        UpdateBrushNodes();

        transparentErase = color.A == 0;
        if (controller.LeftMousePressed)
        {
            IAction? action = pixelPerfect switch
            {
                false => new LineBasedPen_Action(guidValue, color, controller!.LastPixelPosition, (float)ToolSize,
                    transparentErase, antiAliasing, Spacing, BrushData, drawOnMask,
                    document!.AnimationHandler.ActiveFrameBindable, controller.LastPointerInfo, controller.EditorData),
                true => new PixelPerfectPen_Action(guidValue, controller!.LastPixelPosition, color, drawOnMask,
                    document!.AnimationHandler.ActiveFrameBindable)
            };
            internals!.ActionAccumulator.AddActions(action);
        }

        handler.FinalBrushShape = vectorShapeInput?.Value?.ToPath(true);

        return ExecutionState.Success;
    }

    private void UpdateBrushNodes()
    {
        BrushOutputNode brushOutputNode = BrushData.BrushGraph?.AllNodes
            .FirstOrDefault(x => x is BrushOutputNode) as BrushOutputNode;

        if (brushOutputNode is null)
            return;

        brushOutputGuid = brushOutputNode.Id;

        vectorShapeInput =
            brushOutputNode.InputProperties
                .FirstOrDefault(x => x.InternalPropertyName == "VectorShape") as InputProperty<ShapeVectorData>;
    }

    private BrushData GetBrushFromToolbar(IPenToolbar toolbar)
    {
        var pipe = toolbar.Brush.Document.ShareGraph();
        var data = new BrushData(pipe.TryAccessData());
        pipe.Dispose();
        return data;
    }

    public override void OnSettingsChanged(string name, object value)
    {
        if (name == nameof(penToolbar.Brush))
        {
            brushData = GetBrushFromToolbar(penToolbar);
            UpdateBrushNodes();
        }
    }

    public override void OnLeftMouseButtonDown(MouseOnCanvasEventArgs args)
    {
        base.OnLeftMouseButtonDown(args);
        IAction? action = pixelPerfect switch
        {
            false => new LineBasedPen_Action(guidValue, color, controller!.LastPixelPosition, (float)ToolSize,
                transparentErase, antiAliasing, Spacing, BrushData, drawOnMask,
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
            var outputNode = BrushData.BrushGraph.AllNodes.FirstOrDefault(x => x.Id == brushOutputGuid) as BrushOutputNode;
            using Texture surf = new(VecI.One);
            brushData.Value.BrushGraph.Execute(
                outputNode,
                new RenderContext(surf.DrawingSurface, document.AnimationHandler.ActiveFrameTime, ChunkResolution.Full,
                    surf.Size, document.SizeBindable, document.ProcessingColorSpace, SamplingOptions.Default,
                    internals.Tracker.Document.Blackboard)
                {
                    PointerInfo = controller.LastPointerInfo,
                    EditorData = controller.EditorData,
                });
        }

        handler.FinalBrushShape = vectorShapeInput?.Value?.ToPath(true);
    }

    public override void OnPixelPositionChange(VecI pos, MouseOnCanvasEventArgs args)
    {
        if (controller.LeftMousePressed)
        {
            IAction? action = pixelPerfect switch
            {
                false => new LineBasedPen_Action(guidValue, color, pos, (float)ToolSize, transparentErase, antiAliasing,
                    Spacing, BrushData, drawOnMask, document!.AnimationHandler.ActiveFrameBindable,
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
        handler.FinalBrushShape = vectorShapeInput?.Value?.ToPath(true);
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
