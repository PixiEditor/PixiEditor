using PixiEditor.AvaloniaUI.Models.Handlers;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

public class RasterKeyFrameViewModel : KeyFrameViewModel, IRasterKeyFrameHandler
{
    public Guid TargetLayerGuid { get; }
    
    public RasterKeyFrameViewModel(Guid targetLayerGuid, int startFrame, int duration) : base(startFrame, duration)
    {
        TargetLayerGuid = targetLayerGuid;
    }
}
