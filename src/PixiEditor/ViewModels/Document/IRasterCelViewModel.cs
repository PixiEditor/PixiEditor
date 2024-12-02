using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Document;

internal class IRasterCelViewModel : CelViewModel, IRasterCelHandler
{
    public IRasterCelViewModel(Guid targetLayerGuid, int startFrame, int duration, Guid id, DocumentViewModel doc, DocumentInternalParts internalParts) 
        : base(startFrame, duration, targetLayerGuid, id, doc, internalParts)
    {
        
    }

}
