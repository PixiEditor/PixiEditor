using System.Windows.Input;

namespace PixiEditor.Models.Commands.Templates;

internal partial class ShortcutProvider
{
    public static AsepriteProvider Aseprite { get; } = new();

    internal class AsepriteProvider : ShortcutProvider, IShortcutDefaults
    {
        public AsepriteProvider() : base("Aseprite")
        {
            LogoPath = "/Images/TemplateLogos/Aseprite.png";
        }

        public List<Shortcut> DefaultShortcuts { get; } = new()
        {
            new Shortcut(Key.S, ModifierKeys.Control | ModifierKeys.Alt, "PixiEditor.File.SaveAsNew"),
            new Shortcut(Key.K, ModifierKeys.Control, "PixiEditor.Window.OpenSettingsWindow"),
            // Tools
            new Shortcut(Key.U, ModifierKeys.Shift, "PixiEditor.Tools.Select.EllipseToolViewModel"),
            new Shortcut(Key.I, ModifierKeys.None, "PixiEditor.Tools.Select.ColorPickerToolViewModel"),
            new Shortcut(Key.U, ModifierKeys.None, "PixiEditor.Tools.Select.RectangleToolViewModel"),
            new Shortcut(Key.V, ModifierKeys.None, "PixiEditor.Tools.Select.SelectToolViewModel"),
            // Not actually in aseprite, but should be included
            new Shortcut(Key.OemComma, ModifierKeys.Control, "PixiEditor.Search.Toggle")
        };
    }
}
