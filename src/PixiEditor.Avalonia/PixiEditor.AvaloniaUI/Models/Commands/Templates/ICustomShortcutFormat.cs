using PixiEditor.AvaloniaUI.Models.Commands.Templates.Providers.Parsers;

namespace PixiEditor.AvaloniaUI.Models.Commands.Templates;

public interface ICustomShortcutFormat
{
    public KeysParser KeysParser { get; }
    public string[] CustomShortcutExtensions { get; }
}
