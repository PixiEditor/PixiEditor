using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;

namespace PixiEditor.Helpers;

internal interface IFolderHandlerFactory
{
    public IDocument Document { get; }
    public IFolderHandler CreateFolderHandler(DocumentInternalParts helper, Guid infoGuidValue);
}
