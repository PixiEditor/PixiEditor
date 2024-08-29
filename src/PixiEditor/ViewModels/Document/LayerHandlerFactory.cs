using PixiEditor.Helpers;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;
using PixiEditor.ViewModels.Document.Nodes;

namespace PixiEditor.ViewModels.Document;

internal class LayerHandlerFactory : ILayerHandlerFactory
{
    public DocumentViewModel Document { get; }
    IDocument ILayerHandlerFactory.Document => Document;

    public LayerHandlerFactory(DocumentViewModel document)
    {
        Document = document;
    }

    public ILayerHandler CreateLayerHandler(DocumentInternalParts helper, Guid infoGuidValue)
    {
        return new LayerViewModel(Document, helper, infoGuidValue);
    }
}
