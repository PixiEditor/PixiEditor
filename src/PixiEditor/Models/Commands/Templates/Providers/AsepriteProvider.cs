using System.Windows.Input;

namespace PixiEditor.Models.Commands.Templates;

internal partial class ShortcutProvider
{
    public static AsepriteProvider Aseprite { get; } = new();

    internal class AsepriteProvider : ShortcutProvider, IShortcutDefaults
    {
        public AsepriteProvider() : base("Aseprite")
        {
        }

        public ShortcutCollection DefaultShortcuts { get; } = new()
        {
            { "PixiEditor.File.SaveAsNew", Key.S, ModifierKeys.Control | ModifierKeys.Alt },
            { "PixiEditor.Window.OpenSettingsWindow", Key.K, ModifierKeys.Control },
            // Tools
            { "PixiEditor.Tools.Select.EllipseToolViewModel", Key.U, ModifierKeys.Shift },
            { "PixiEditor.Tools.Select.ColorPickerToolViewModel", Key.I, ModifierKeys.None },
            { "PixiEditor.Tools.Select.RectangleToolViewModel", Key.U, ModifierKeys.None },
            { "PixiEditor.Tools.Select.SelectToolViewModel", Key.V, ModifierKeys.None },
            // Not actually in aseprite, but should be included
            { "PixiEditor.Search.Toggle", Key.OemComma, ModifierKeys.Control }
        };
    }
}
