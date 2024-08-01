using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;

namespace PixiEditor.Helpers;

internal interface ILayerHandlerFactory
{
    public IDocument Document { get; }
    public ILayerHandler CreateLayerHandler(DocumentInternalParts helper, Guid infoGuidValue);
}
