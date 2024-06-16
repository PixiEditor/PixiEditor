using PixiEditor.AvaloniaUI.Models.DocumentModels;
using PixiEditor.AvaloniaUI.Models.Handlers;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

internal class RasterKeyFrameViewModel : KeyFrameViewModel, IRasterKeyFrameHandler
{
    public RasterKeyFrameViewModel(Guid targetLayerGuid, int startFrame, int duration, Guid id, DocumentViewModel doc, DocumentInternalParts internalParts) 
        : base(startFrame, duration, targetLayerGuid, id, doc, internalParts)
    {
        
    }

}
