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
    public BrushData BrushData => brushData ??= BrushToolbar.CreateBrushData();
    private BrushData? brushData;
    private Guid brushOutputGuid = Guid.Empty;
    private BrushOutputNode? outputNode;
    private VecD lastSmoothed;
    private DateTime lastTime;
    private double lastViewportZoom = 1.0;

    protected IBrushToolHandler BrushTool;
    protected IBrushToolbar BrushToolbar;
    protected IBrushToolHandler handler;
    protected IColorsHandler colorsHandler;

    protected Guid layerId;
    protected Color color;
    protected bool antiAliasing;
    protected bool firstApply = true;

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

        UpdateBrushNodes();

        if (controller.LeftMousePressed)
        {
            EnqueueDrawActions();
        }

        return ExecutionState.Success;
    }

    protected virtual void EnqueueDrawActions()
    {
        var point = GetStabilizedPoint();

        if (handler != null)
        {
            handler.LastAppliedPoint = point;
        }

        IAction? action = new LineBasedPen_Action(layerId, point, (float)ToolSize,
            antiAliasing, BrushData, drawOnMask,
            document!.AnimationHandler.ActiveFrameBindable, controller.LastPointerInfo, controller.LastKeyboardInfo,
            controller.EditorData);

        internals!.ActionAccumulator.AddActions(action);
    }

    protected VecD GetStabilizedPoint()
    {
        if (firstApply)
        {
            lastSmoothed = controller.LastPrecisePosition;
            lastTime = DateTime.Now;
            firstApply = false;
            return lastSmoothed;
        }

        if (BrushToolbar.StabilizationMode == StabilizationMode.TimeBased)
        {
            return GetStabilizedPointTimeBased();
        }

        if (BrushToolbar.StabilizationMode == StabilizationMode.CircleRope)
        {
            return GetStabilizedPointCircleRope(lastViewportZoom);
        }

        return controller.LastPrecisePosition;
    }

    protected VecD GetStabilizedPointTimeBased()
    {
        float timeConstant = (float)BrushToolbar.Stabilization / 100f;
        float elapsed = Math.Min((float)(DateTime.Now - lastTime).TotalSeconds, 0.1f);
        float alpha = elapsed / Math.Max(timeConstant + elapsed, 0.0001f);
        VecD smoothed = lastSmoothed + (controller.LastPrecisePosition - lastSmoothed) * alpha;
        lastTime = DateTime.Now;

        lastSmoothed = smoothed;
        return smoothed;
    }

    protected VecD GetStabilizedPointCircleRope(double viewportZoom)
    {
        float radius = (float)BrushToolbar.Stabilization / (float)viewportZoom;
        VecD direction = controller.LastPrecisePosition - lastSmoothed;
        float distance = (float)direction.Length;

        if (distance > radius)
        {
            direction = direction.Normalize();
            lastSmoothed += direction * (distance - radius);
        }

        return lastSmoothed;
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

    public override void OnLeftMouseButtonDown(MouseOnCanvasEventArgs args)
    {
        base.OnLeftMouseButtonDown(args);
        EnqueueDrawActions();
    }

    public override void OnPrecisePositionChange(MouseOnCanvasEventArgs args)
    {
        base.OnPrecisePositionChange(args);
        if (controller.LeftMousePressed)
        {
            lastViewportZoom = args.ViewportScale;
            EnqueueDrawActions();
        }
    }

    public override void OnConvertedKeyDown(Key key)
    {
        base.OnConvertedKeyDown(key);
        /*
        UpdateBrushOverlay(controller.LastPrecisePosition);
    */
    }

    public override void OnSettingsChanged(string name, object value)
    {
        if (name == nameof(BrushToolbar.Brush))
        {
            brushData = BrushToolbar.CreateBrushData();
            UpdateBrushNodes();
        }

        if (name is nameof(IBrushToolbar.ToolSize) or nameof(IBrushToolbar.AntiAliasing))
        {
            brushData = BrushToolbar.CreateBrushData();
        }

        /*
        ExecuteBrush();
        UpdateBrushOverlay(controller.LastPrecisePosition);
    */
    }

    public override void OnColorChanged(Color color, bool primary)
    {
        if (primary)
        {
            this.color = color;
        }
    }


    public override void OnLeftMouseButtonUp(VecD argsPositionOnCanvas)
    {
        EnqueueEndDraw();
    }

    protected virtual void EnqueueEndDraw()
    {
        firstApply = true;
        internals!.ActionAccumulator.AddFinishedActions(new EndLineBasedPen_Action());
    }

    public override void ForceStop()
    {
        EnqueueEndDraw();
    }
}
