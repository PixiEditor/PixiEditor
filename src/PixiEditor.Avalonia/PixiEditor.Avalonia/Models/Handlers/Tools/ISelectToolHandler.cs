using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Models.Enums;

namespace PixiEditor.Models.Containers.Tools;

internal interface ISelectToolHandler : IToolHandler
{
    public SelectionShape SelectShape { get; }
    public SelectionMode ResultingSelectionMode { get; }
}
