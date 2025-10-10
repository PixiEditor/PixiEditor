using Avalonia.Input;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.Models.BrushEngine;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal class BrushBasedExecutor<T> : UpdateableChangeExecutor where T : IBrushToolHandler
{
    public BrushData BrushData => brushData ??= GetBrushFromToolbar(BrushToolbar);
    private BrushData? brushData;
    private Guid brushOutputGuid = Guid.Empty;

    protected IBrushToolHandler BrushTool;
    protected IBrushToolbar BrushToolbar;
    protected IBrushToolHandler handler;
    protected IColorsHandler colorsHandler;

    protected Guid layerId;
    protected Color color;
    protected bool antiAliasing;

    private InputProperty<ShapeVectorData> vectorShapeInput;

    protected bool drawOnMask;
    public double ToolSize => BrushToolbar.ToolSize;
    public float Spacing => BrushToolbar.Spacing;

    public override bool BlocksOtherActions => controller.LeftMousePressed;

    public override ExecutionState Start()
    {
        IStructureMemberHandler? member = document!.SelectedStructureMember;
        IColorsHandler? colorsHandler = GetHandler<IColorsHandler>();

        BrushTool = GetHandler<T>();
        if (colorsHandler is null || BrushTool is null || member is null || BrushTool?.Toolbar is not IBrushToolbar toolbar)
            return ExecutionState.Error;
        drawOnMask = member is not ILayerHandler layer || layer.ShouldDrawOnMask;
        if (drawOnMask && !member.HasMaskBindable)
            return ExecutionState.Error;
        if (!drawOnMask && member is not ILayerHandler)
            return ExecutionState.Error;

        handler = BrushTool;
        BrushToolbar = toolbar;
        layerId = member.Id;
        color = colorsHandler.PrimaryColor;
        antiAliasing = toolbar.AntiAliasing;
        this.colorsHandler = colorsHandler;

        UpdateBrushNodes();

        if (controller.LeftMousePressed)
        {
            EnqueueDrawActions();
        }

        handler.FinalBrushShape = vectorShapeInput?.Value?.ToPath(true);

        return ExecutionState.Success;
    }

    protected virtual void EnqueueDrawActions()
    {
        IAction? action = new LineBasedPen_Action(layerId, controller!.LastPixelPosition, (float)ToolSize,
            antiAliasing, Spacing, BrushData, drawOnMask,
            document!.AnimationHandler.ActiveFrameBindable, controller.LastPointerInfo, controller.EditorData);

        internals!.ActionAccumulator.AddActions(action);
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

    private BrushData GetBrushFromToolbar(IBrushToolbar toolbar)
    {
        Brush? brush = toolbar.Brush;
        if (brush == null)
        {
            return new BrushData();
        }

        var pipe = toolbar.Brush.Document.ShareGraph();
        var data = new BrushData(pipe.TryAccessData());
        pipe.Dispose();
        return data;
    }

    public override void OnLeftMouseButtonDown(MouseOnCanvasEventArgs args)
    {
        base.OnLeftMouseButtonDown(args);
        EnqueueDrawActions();
    }

    public override void OnPrecisePositionChange(MouseOnCanvasEventArgs args)
    {
        base.OnPrecisePositionChange(args);
        if (!controller.LeftMousePressed)
        {
            var outputNode =
                BrushData.BrushGraph.AllNodes.FirstOrDefault(x => x.Id == brushOutputGuid) as BrushOutputNode;
            using Texture surf = new(VecI.One);
            brushData.Value.BrushGraph.Execute(
                outputNode,
                new RenderContext(surf.DrawingSurface, document.AnimationHandler.ActiveFrameTime, ChunkResolution.Full,
                    surf.Size, document.SizeBindable, document.ProcessingColorSpace, SamplingOptions.Default,
                    BrushData.BrushGraph)
                {
                    PointerInfo = controller.LastPointerInfo, EditorData = controller.EditorData,
                });
        }

        handler.FinalBrushShape = vectorShapeInput?.Value?.ToPath(true);
    }

    public override void OnPixelPositionChange(VecI pos, MouseOnCanvasEventArgs args)
    {
        if (controller.LeftMousePressed)
        {
            EnqueueDrawActions();
        }
    }

    public override void OnConvertedKeyDown(Key key)
    {
        base.OnConvertedKeyDown(key);
        handler.FinalBrushShape = vectorShapeInput?.Value?.ToPath(true);
    }

    public override void OnSettingsChanged(string name, object value)
    {
        if (name == nameof(BrushToolbar.Brush))
        {
            brushData = GetBrushFromToolbar(BrushToolbar);
            UpdateBrushNodes();
        }
    }


    public override void OnLeftMouseButtonUp(VecD argsPositionOnCanvas)
    {
        EnqueueEndDraw();
    }

    protected virtual void EnqueueEndDraw()
    {
        internals!.ActionAccumulator.AddFinishedActions(new EndLineBasedPen_Action());
    }

    public override void ForceStop()
    {
        EnqueueEndDraw();
    }
}
