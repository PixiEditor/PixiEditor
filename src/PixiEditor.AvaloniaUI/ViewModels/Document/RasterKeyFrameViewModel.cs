using PixiEditor.AvaloniaUI.Models.Handlers;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

public class RasterKeyFrameViewModel : KeyFrameViewModel, IRasterKeyFrameHandler
{
    public RasterKeyFrameViewModel(Guid targetLayerGuid, int startFrame, int duration, Guid id) : base(startFrame, duration, targetLayerGuid, id)
    {
        
    }

}
