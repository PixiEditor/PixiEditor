using PixiEditor.AvaloniaUI.Models.Position;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.AvaloniaUI.Models.Handlers.Tools;

internal interface ISelectToolHandler : IToolHandler
{
    public SelectionShape SelectShape { get; }
    public SelectionMode ResultingSelectionMode { get; }
}
