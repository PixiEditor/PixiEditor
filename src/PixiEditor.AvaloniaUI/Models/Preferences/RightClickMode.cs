using System.ComponentModel;

namespace PixiEditor.AvaloniaUI.Models.Preferences;

public enum RightClickMode
{
    [Description("USE_SECONDARY_COLOR")]
    SecondaryColor,
    [Description("SHOW_CONTEXT_MENU")]
    ContextMenu,
    [Description("ERASE")]
    Erase
}
