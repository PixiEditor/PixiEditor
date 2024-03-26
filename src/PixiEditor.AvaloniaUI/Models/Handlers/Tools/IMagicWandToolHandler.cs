using PixiEditor.AvaloniaUI.Models.Tools;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.AvaloniaUI.Models.Handlers.Tools;

internal interface IMagicWandToolHandler : IToolHandler
{
    public SelectionMode SelectMode { get; }
    public DocumentScope DocumentScope { get; }
}
