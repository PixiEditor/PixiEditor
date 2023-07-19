using PixiEditor.Models.Containers;
using PixiEditor.Models.DocumentModels;

namespace PixiEditor.Avalonia.Helpers;

internal interface ILayerHandlerFactory
{
    public IDocument Document { get; }
    public ILayerHandler CreateLayerHandler(DocumentInternalParts helper, Guid infoGuidValue);
}
