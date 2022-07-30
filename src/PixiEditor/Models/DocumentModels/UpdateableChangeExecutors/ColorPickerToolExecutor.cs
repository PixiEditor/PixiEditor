using PixiEditor.Models.Enums;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal class ColorPickerToolExecutor : UpdateableChangeExecutor
{
    public override ExecutionState Start()
    {
        ColorsViewModel colorsViewModel = ViewModelMain.Current?.ColorsSubViewModel;
        
        if(document is null || controller is null)
        {
            return ExecutionState.Error;
        }
        
        colorsViewModel.PrimaryColor = document.PickColor(controller.LastPixelPosition, false);
        return ExecutionState.Success;
    }

    public override void OnLeftMouseButtonUp()
    {
        onEnded?.Invoke(this);
    }

    public override void ForceStop()
    {
        
    }
}
