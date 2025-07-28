using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Models.Position;

namespace PixiEditor.Models.Handlers.Tools;

internal interface ISelectToolHandler : IToolHandler
{
    public SelectionShape SelectShape { get; }
    public SelectionMode ResultingSelectionMode { get; }
}
