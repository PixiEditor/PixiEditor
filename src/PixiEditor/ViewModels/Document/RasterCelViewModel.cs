using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Document;

internal class RasterCelViewModel : CelViewModel, IRasterCelHandler
{
    public RasterCelViewModel(Guid targetLayerGuid, int startFrame, int duration, Guid id, DocumentViewModel doc, DocumentInternalParts internalParts)
        : base(startFrame, duration, targetLayerGuid, id, doc, internalParts)
    {
        
    }

}
