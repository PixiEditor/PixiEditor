using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Models.DocumentModels;
using PixiEditor.AvaloniaUI.Models.Handlers;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

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
