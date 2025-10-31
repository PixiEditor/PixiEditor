using System.Diagnostics;
using Avalonia.Input;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
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

internal class BrushBasedExecutor<T> : BrushBasedExecutor where T : IBrushToolHandler
{
    public override ExecutionState Start()
    {
        BrushTool = GetHandler<T>();
        return base.Start();
    }
}

internal class BrushBasedExecutor : UpdateableChangeExecutor
{
    public BrushData BrushData => brushData ??= GetBrushFromToolbar(BrushToolbar);
    private BrushData? brushData;
    private Guid brushOutputGuid = Guid.Empty;
    private BrushOutputNode? outputNode;
    private ChunkyImage previewImage = null!;
    private ChangeableDocument.Changeables.Brushes.BrushEngine engine = new();
    private VecD lastSmoothed;
    private Stopwatch stopwatch = new Stopwatch();

    protected IBrushToolHandler BrushTool;
    protected IBrushToolbar BrushToolbar;
    protected IBrushToolHandler handler;
    protected IColorsHandler colorsHandler;

    protected Guid layerId;
    protected Color color;
    protected bool antiAliasing;
    private bool firstApply = true;

    protected bool drawOnMask;
    public double ToolSize => BrushToolbar.ToolSize;

    public override bool BlocksOtherActions => controller.LeftMousePressed;

    public BrushBasedExecutor(IBrushToolHandler handler)
    {
        BrushTool = handler;
    }

    public BrushBasedExecutor()
    {
    }

    public override ExecutionState Start()
    {
        IStructureMemberHandler? member = document!.SelectedStructureMember;
        IColorsHandler? colorsHandler = GetHandler<IColorsHandler>();

        if (colorsHandler is null || BrushTool is null || member is null ||
            BrushTool?.Toolbar is not IBrushToolbar toolbar)
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

        previewImage = new ChunkyImage(new VecI(1), ColorSpace.CreateSrgb());

        UpdateBrushNodes();

        if (controller.LeftMousePressed)
        {
            EnqueueDrawActions();
        }

        UpdateBrushOverlay(controller.LastPrecisePosition);

        return ExecutionState.Success;
    }

    protected virtual void EnqueueDrawActions()
    {
        IAction? action = new LineBasedPen_Action(layerId, GetStabilizedPoint(), (float)ToolSize,
            antiAliasing, BrushData, drawOnMask,
            document!.AnimationHandler.ActiveFrameBindable, controller.LastPointerInfo, controller.LastKeyboardInfo,
            controller.EditorData);

        internals!.ActionAccumulator.AddActions(action);
    }

    protected VecD GetStabilizedPoint()
    {
        float timeConstant = 0.01f;
        float elapsed = (float)stopwatch.Elapsed.TotalSeconds;
        float alpha = elapsed / Math.Max(timeConstant + elapsed, 0.0001f);
        VecD smoothed = lastSmoothed + (controller.LastPrecisePosition - lastSmoothed) * alpha;
        stopwatch.Restart();
        if (firstApply)
        {
            smoothed = controller.LastPrecisePosition;
        }

        lastSmoothed = smoothed;
        //firstApply = false;
        return smoothed;
    }


    private void UpdateBrushNodes()
    {
        BrushOutputNode brushOutputNode = BrushData.BrushGraph?.AllNodes
            .FirstOrDefault(x => x is BrushOutputNode) as BrushOutputNode;

        if (brushOutputNode is null)
            return;

        brushOutputGuid = brushOutputNode.Id;

        outputNode =
            BrushData.BrushGraph.AllNodes.FirstOrDefault(x => x.Id == brushOutputGuid) as BrushOutputNode;
    }

    private BrushData GetBrushFromToolbar(IBrushToolbar toolbar)
    {
        Brush? brush = toolbar.Brush;
        if (brush == null)
        {
            return new BrushData();
        }

        var pipe = toolbar.Brush.Document.ShareGraph();
        var data = new BrushData(pipe.TryAccessData())
        {
            AntiAliasing = toolbar.AntiAliasing, StrokeWidth = (float)toolbar.ToolSize
        };
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
            ExecuteBrush();
        }
        else
        {
            EnqueueDrawActions();
        }

        UpdateBrushOverlay(args.PositionOnCanvas);
    }

    private void UpdateBrushOverlay(VecD pos)
    {
        if (!brushData.HasValue || brushData.Value.BrushGraph == null) return;

        handler.FinalBrushShape = engine.EvaluateShape(pos, brushData.Value);
    }

    private void ExecuteBrush()
    {
        engine.ExecuteBrush(previewImage, BrushData, controller.LastPrecisePosition,
            document.AnimationHandler.ActiveFrameTime,
            ColorSpace.CreateSrgb(), SamplingOptions.Default, controller.LastPointerInfo, controller.LastKeyboardInfo,
            controller.EditorData);
    }

    public override void OnConvertedKeyDown(Key key)
    {
        base.OnConvertedKeyDown(key);
        UpdateBrushOverlay(controller.LastPrecisePosition);
    }

    public override void OnSettingsChanged(string name, object value)
    {
        if (name == nameof(BrushToolbar.Brush))
        {
            brushData = GetBrushFromToolbar(BrushToolbar);
            UpdateBrushNodes();
        }

        if (name is nameof(IBrushToolbar.ToolSize) or nameof(IBrushToolbar.AntiAliasing))
        {
            brushData = GetBrushFromToolbar(BrushToolbar);
        }

        ExecuteBrush();
        UpdateBrushOverlay(controller.LastPrecisePosition);
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
        engine.Dispose();
    }
}
