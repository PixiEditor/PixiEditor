using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Actions.Undo;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Models.Controllers.InputDevice;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class FloodFillToolExecutor : UpdateableChangeExecutor
{
    private bool considerAllLayers;
    private bool drawOnMask;
    private Guid memberGuid;
    private Color color;
    private float tolerance;
    private FloodFillMode fillMode;

    public override ExecutionState Start()
    {
        var fillTool = GetHandler<IFloodFillToolHandler>();
        IColorsHandler? colorsVM = GetHandler<IColorsHandler>();
        var member = document!.SelectedStructureMember;
        color = colorsVM!.PrimaryColor;

        if (fillTool is null || member is null || colorsVM is null)
            return ExecutionState.Error;
        drawOnMask = member is ILayerHandler layer ? layer.ShouldDrawOnMask : true;
        if (drawOnMask && !member.HasMaskBindable)
            return ExecutionState.Error;
        if (!drawOnMask && member is not ILayerHandler)
            return ExecutionState.Error;

        colorsVM.AddSwatch(new PaletteColor(color.R, color.G, color.B));
        memberGuid = member.Id;
        considerAllLayers = fillTool.ConsiderAllLayers;
        color = colorsVM.PrimaryColor;
        var pos = controller!.LastPixelPosition;
        tolerance = fillTool.Tolerance;
        fillMode = fillTool.FillMode;
        
        internals!.ActionAccumulator.AddActions(new FloodFill_Action(memberGuid, pos, color, considerAllLayers, tolerance, fillMode, drawOnMask, document!.AnimationHandler.ActiveFrameBindable));

        return ExecutionState.Success;
    }

    public override void OnPixelPositionChange(VecI pos, MouseOnCanvasEventArgs args)
    {
        internals!.ActionAccumulator.AddActions(new FloodFill_Action(memberGuid, pos, color, considerAllLayers, tolerance, fillMode, drawOnMask, document!.AnimationHandler.ActiveFrameBindable));
    }

    public override void OnLeftMouseButtonUp(VecD argsPositionOnCanvas)
    {
        internals!.ActionAccumulator.AddActions(new ChangeBoundary_Action());
        onEnded!(this);
    }

    public override void ForceStop()
    {
        internals!.ActionAccumulator.AddActions(new ChangeBoundary_Action());
    }
}
