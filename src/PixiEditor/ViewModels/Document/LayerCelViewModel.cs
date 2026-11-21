using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Document;

internal class LayerCelViewModel : CelViewModel, ILayerCelHandler
{
    public LayerCelViewModel(Guid targetLayerGuid, int startFrame, int duration, Guid id, DocumentViewModel doc, DocumentInternalParts internalParts)
        : base(startFrame, duration, targetLayerGuid, id, doc, internalParts)
    {
        
    }

}
