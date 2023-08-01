using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.Models.Containers.Tools;

internal interface ILassoToolHandler : IToolHandler
{
    public SelectionMode? ResultingSelectionMode { get; }
}
