using PixiEditor.Models.Enums;

namespace PixiEditor.Models.Containers.Tools;

internal interface IColorPickerHandler : IToolHandler
{
    public DocumentScope Mode { get; }
    public bool PickFromReferenceLayer { get; }
    public bool PickFromCanvas { get; }
}
