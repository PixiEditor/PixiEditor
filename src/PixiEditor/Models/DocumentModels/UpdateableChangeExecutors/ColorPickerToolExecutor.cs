using Drawie.Backend.Core.Numerics;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.Models.Controllers.InputDevice;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

#nullable enable
internal class ColorPickerToolExecutor : UpdateableChangeExecutor
{
    private bool includeReference;
    private bool includeCanvas;
    private DocumentScope scope;
    private IColorsHandler? colorsViewModel;
    private IWindowHandler? windowHandler;
    private Guid? sceneTextureKey;

    public override ExecutionState Start()
    {
        colorsViewModel = GetHandler<IColorsHandler>();
        IColorPickerHandler? tool = GetHandler<IColorPickerHandler>();

        if (colorsViewModel is null || tool is null)
            return ExecutionState.Error;

        scope = tool.Mode;
        includeReference = tool.PickFromReferenceLayer && document!.ReferenceLayerHandler.ReferenceTexture is not null && document!.ReferenceLayerHandler.IsVisible;
        includeCanvas = tool.PickFromCanvas;

        windowHandler = GetHandler<IWindowHandler>();

        IViewport? viewport = windowHandler.ActiveWindow as IViewport;
        string? customOutput = viewport?.RenderOutputName;
        customOutput = customOutput == "DEFAULT" ? null : customOutput;
        sceneTextureKey = viewport?.SceneTextureKey;

        colorsViewModel.PrimaryColor = document.PickColor(controller.LastPrecisePosition, scope, includeReference, includeCanvas, document.AnimationHandler.ActiveFrameBindable, document.ReferenceLayerHandler.IsTopMost, customOutput, sceneTextureKey);
        return ExecutionState.Success;
    }

    public override void OnPrecisePositionChange(MouseOnCanvasEventArgs args)
    {
        if (!includeReference)
            return;

        string? customOutput = (windowHandler?.ActiveWindow as IViewport)?.RenderOutputName;
        customOutput = customOutput == "DEFAULT" ? null : customOutput;

        colorsViewModel.PrimaryColor = document.PickColor(args.Point.PositionOnCanvas, scope, includeReference, includeCanvas, document.AnimationHandler.ActiveFrameBindable, document.ReferenceLayerHandler.IsTopMost, customOutput, sceneTextureKey);
    }

    public override void OnPixelPositionChange(VecI pos, MouseOnCanvasEventArgs args)
    {
        string? customOutput = (windowHandler?.ActiveWindow as IViewport)?.RenderOutputName;
        customOutput = customOutput == "DEFAULT" ? null : customOutput;

        colorsViewModel.PrimaryColor = document.PickColor(pos, scope, includeReference, includeCanvas, document.AnimationHandler.ActiveFrameBindable, document.ReferenceLayerHandler.IsTopMost, customOutput, sceneTextureKey);
    }

    public override void OnLeftMouseButtonUp(VecD argsPositionOnCanvas)
    {
        onEnded?.Invoke(this);
    }

    public override void ForceStop()
    {
        
    }
}
