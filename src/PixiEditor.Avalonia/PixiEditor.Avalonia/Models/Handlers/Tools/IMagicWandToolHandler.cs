using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Models.Enums;

namespace PixiEditor.Models.Containers.Tools;

internal interface IMagicWandToolHandler : IToolHandler
{
    public SelectionMode SelectMode { get; }
    public DocumentScope DocumentScope { get; }
}
