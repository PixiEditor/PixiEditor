using PixiEditor.AvaloniaUI.Models.DocumentModels;
using PixiEditor.AvaloniaUI.Models.Handlers;

namespace PixiEditor.AvaloniaUI.Helpers;

internal interface IFolderHandlerFactory
{
    public IDocument Document { get; }
    public IFolderHandler CreateFolderHandler(DocumentInternalParts helper, Guid infoGuidValue);
}
