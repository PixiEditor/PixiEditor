using PixiEditor.Models.Containers;
using PixiEditor.Models.DocumentModels;

namespace PixiEditor.Avalonia.Helpers;

internal interface IFolderHandlerFactory
{
    public IDocument Document { get; }
    public IFolderHandler CreateFolderHandler(DocumentInternalParts helper, Guid infoGuidValue);
}
