using PixiEditor.AvaloniaUI.Models.Handlers;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

public class RasterClipViewModel : AnimationClipViewModel, IRasterClipHandler
{
    public Guid TargetLayerGuid { get; }
    
    public RasterClipViewModel(Guid targetLayerGuid, int startFrame, int duration) : base(startFrame, duration)
    {
        TargetLayerGuid = targetLayerGuid;
    }
}
