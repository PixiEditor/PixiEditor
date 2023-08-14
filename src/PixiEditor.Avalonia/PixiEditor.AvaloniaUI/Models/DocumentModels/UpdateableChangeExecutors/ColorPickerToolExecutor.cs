using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.Models.Handlers.Tools;
using PixiEditor.AvaloniaUI.Models.Tools;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Models.DocumentModels.UpdateableChangeExecutors;

#nullable enable
internal class ColorPickerToolExecutor : UpdateableChangeExecutor
{
    private bool includeReference;
    private bool includeCanvas;
    private DocumentScope scope;
    private IColorsHandler? colorsViewModel;

    public override ExecutionState Start()
    {
        colorsViewModel = GetHandler<IColorsHandler>();
        IColorPickerHandler? tool = GetHandler<IColorPickerHandler>();

        if (colorsViewModel is null || tool is null)
            return ExecutionState.Error;

        scope = tool.Mode;
        includeReference = tool.PickFromReferenceLayer && document!.ReferenceLayerHandler.ReferenceBitmap is not null;
        includeCanvas = tool.PickFromCanvas;
        
        colorsViewModel.PrimaryColor = document.PickColor(controller.LastPrecisePosition, scope, includeReference, includeCanvas, document.ReferenceLayerHandler.IsTopMost);
        return ExecutionState.Success;
    }

    public override void OnPrecisePositionChange(VecD pos)
    {
        if (!includeReference)
            return;
        colorsViewModel.PrimaryColor = document.PickColor(pos, scope, includeReference, includeCanvas, document.ReferenceLayerHandler.IsTopMost);
    }

    public override void OnPixelPositionChange(VecI pos)
    {
        colorsViewModel.PrimaryColor = document.PickColor(pos, scope, includeReference, includeCanvas, document.ReferenceLayerHandler.IsTopMost);
    }

    public override void OnLeftMouseButtonUp()
    {
        onEnded?.Invoke(this);
    }

    public override void ForceStop()
    {
        
    }
}
