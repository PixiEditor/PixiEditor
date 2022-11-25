using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

#nullable enable
internal class ColorPickerToolExecutor : UpdateableChangeExecutor
{
    private bool includeReference;
    private bool includeCanvas;
    private DocumentScope scope;
    private ColorsViewModel? colorsViewModel;

    public override ExecutionState Start()
    {
        colorsViewModel = ViewModelMain.Current?.ColorsSubViewModel;
        ColorPickerToolViewModel? tool = ViewModelMain.Current?.ToolsSubViewModel.GetTool<ColorPickerToolViewModel>();

        if (colorsViewModel is null || tool is null)
            return ExecutionState.Error;

        scope = tool.Mode;
        includeReference = tool.PickFromReferenceLayer && document!.ReferenceLayerViewModel.ReferenceBitmap is not null;
        includeCanvas = tool.PickFromCanvas;
        
        colorsViewModel.PrimaryColor = document.PickColor(controller.LastPrecisePosition, scope, includeReference, includeCanvas);
        return ExecutionState.Success;
    }

    public override void OnPrecisePositionChange(VecD pos)
    {
        if (!includeReference)
            return;
        colorsViewModel.PrimaryColor = document.PickColor(pos, scope, includeReference, includeCanvas);
    }

    public override void OnPixelPositionChange(VecI pos)
    {
        colorsViewModel.PrimaryColor = document.PickColor(pos, scope, includeReference, includeCanvas);
    }

    public override void OnLeftMouseButtonUp()
    {
        onEnded?.Invoke(this);
    }

    public override void ForceStop()
    {
        
    }
}
