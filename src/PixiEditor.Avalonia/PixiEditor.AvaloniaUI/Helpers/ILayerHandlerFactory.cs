using PixiEditor.AvaloniaUI.Models.DocumentModels;
using PixiEditor.AvaloniaUI.Models.Handlers;

namespace PixiEditor.AvaloniaUI.Helpers;

internal interface ILayerHandlerFactory
{
    public IDocument Document { get; }
    public ILayerHandler CreateLayerHandler(DocumentInternalParts helper, Guid infoGuidValue);
}
