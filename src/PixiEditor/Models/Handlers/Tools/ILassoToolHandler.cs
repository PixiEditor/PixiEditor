using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.Models.Handlers.Tools;

internal interface ILassoToolHandler : IToolHandler
{
    public SelectionMode? ResultingSelectionMode { get; }
}
