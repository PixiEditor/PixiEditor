using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class BrightnessToolExecutor : UpdateableChangeExecutor
{
    private Guid guidValue;
    private bool repeat;
    private float correctionFactor;
    private int toolSize;

    public override ExecutionState Start()
    {
        ViewModelMain? vm = ViewModelMain.Current;
        StructureMemberViewModel? member = document!.SelectedStructureMember;
        BrightnessToolViewModel? tool = vm?.ToolsSubViewModel.GetTool<BrightnessToolViewModel>();
        if (vm is null || tool is null || member is null)
            return ExecutionState.Error;
        if (member is not LayerViewModel layer || layer.ShouldDrawOnMask)
            return ExecutionState.Error;

        guidValue = member.GuidValue;
        repeat = tool.BrightnessMode == BrightnessMode.Repeat;
        toolSize = tool.ToolSize;
        correctionFactor = tool.Darken || tool.UsedWith == MouseButton.Right ? -tool.CorrectionFactor : tool.CorrectionFactor;

        ChangeBrightness_Action action = new(guidValue, controller!.LastPixelPosition, correctionFactor, toolSize, repeat);
        internals!.ActionAccumulator.AddActions(action);

        return ExecutionState.Success;
    }

    public override void OnPixelPositionChange(VecI pos)
    {
        ChangeBrightness_Action action = new(guidValue, pos, correctionFactor, toolSize, repeat);
        internals!.ActionAccumulator.AddActions(action);
    }

    public override void OnLeftMouseButtonUp()
    {
        internals!.ActionAccumulator.AddFinishedActions(new EndChangeBrightness_Action());
        onEnded?.Invoke(this);
    }

    public override void ForceStop()
    {
        internals!.ActionAccumulator.AddFinishedActions(new EndChangeBrightness_Action());
    }
}
