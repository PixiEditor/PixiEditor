using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.AvaloniaUI.Models.Handlers.Tools;

internal interface ILassoToolHandler : IToolHandler
{
    public SelectionMode? ResultingSelectionMode { get; }
}
