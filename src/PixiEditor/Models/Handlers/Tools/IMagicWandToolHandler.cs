using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Models.Tools;

namespace PixiEditor.Models.Handlers.Tools;

internal interface IMagicWandToolHandler : IToolHandler
{
    public SelectionMode ResultingSelectionMode { get; }
    public DocumentScope DocumentScope { get; }
    public float Tolerance { get; }
    public FloodMode FloodMode { get; }
}
