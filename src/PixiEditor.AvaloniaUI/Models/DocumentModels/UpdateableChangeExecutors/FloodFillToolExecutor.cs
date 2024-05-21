using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.Models.Handlers.Tools;
using PixiEditor.AvaloniaUI.Models.Tools;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Actions.Undo;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.Palettes;
using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class FloodFillToolExecutor : UpdateableChangeExecutor
{
    private bool considerAllLayers;
    private bool drawOnMask;
    private Guid memberGuid;
    private Color color;

    public override ExecutionState Start()
    {
        var fillTool = GetHandler<IFloodFillToolHandler>();
        IColorsHandler? colorsVM = GetHandler<IColorsHandler>();
        var member = document!.SelectedStructureMember;

        if (fillTool is null || member is null || colorsVM is null)
            return ExecutionState.Error;
        drawOnMask = member is ILayerHandler layer ? layer.ShouldDrawOnMask : true;
        if (drawOnMask && !member.HasMaskBindable)
            return ExecutionState.Error;
        if (!drawOnMask && member is not ILayerHandler)
            return ExecutionState.Error;

        colorsVM.AddSwatch(new PaletteColor(color.R, color.G, color.B));
        memberGuid = member.GuidValue;
        considerAllLayers = fillTool.ConsiderAllLayers;
        color = colorsVM.PrimaryColor;
        var pos = controller!.LastPixelPosition;

        internals!.ActionAccumulator.AddActions(new FloodFill_Action(memberGuid, pos, color, considerAllLayers, drawOnMask));

        return ExecutionState.Success;
    }

    public override void OnPixelPositionChange(VecI pos)
    {
        internals!.ActionAccumulator.AddActions(new FloodFill_Action(memberGuid, pos, color, considerAllLayers, drawOnMask));
    }

    public override void OnLeftMouseButtonUp()
    {
        internals!.ActionAccumulator.AddActions(new ChangeBoundary_Action());
        onEnded!(this);
    }

    public override void ForceStop()
    {
        internals!.ActionAccumulator.AddActions(new ChangeBoundary_Action());
    }
}
