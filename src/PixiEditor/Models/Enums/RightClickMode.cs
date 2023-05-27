using System.ComponentModel;

namespace PixiEditor.Models.Enums;

public enum RightClickMode
{
    [Description("SHOW_CONTEXT_MENU")]
    ContextMenu,
    [Description("ERASE")]
    Erase,
    [Description("USE_SECONDARY_COLOR")]
    SecondaryColor
}
