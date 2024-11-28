using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Document;

internal class RasterKeyFrameViewModel : KeyFrameViewModel, IRasterKeyFrameHandler
{
    public RasterKeyFrameViewModel(Guid targetLayerGuid, int startFrame, int duration, Guid id, DocumentViewModel doc, DocumentInternalParts internalParts) 
        : base(startFrame, duration, targetLayerGuid, id, doc, internalParts)
    {
        
    }

}
