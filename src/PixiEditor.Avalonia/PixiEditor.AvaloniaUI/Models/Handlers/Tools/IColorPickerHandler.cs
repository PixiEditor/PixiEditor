using PixiEditor.AvaloniaUI.Models.Tools;

namespace PixiEditor.AvaloniaUI.Models.Handlers.Tools;

internal interface IColorPickerHandler : IToolHandler
{
    public DocumentScope Mode { get; }
    public bool PickFromReferenceLayer { get; }
    public bool PickFromCanvas { get; }
}
