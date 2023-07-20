using PixiEditor.Models.Enums;

namespace PixiEditor.Models.Containers.Tools;

internal interface IColorPickerHandler : IToolHandler
{
    public DocumentScope Mode { get; set; }
    public bool PickFromReferenceLayer { get; set; }
    public bool PickFromCanvas { get; set; }
}
