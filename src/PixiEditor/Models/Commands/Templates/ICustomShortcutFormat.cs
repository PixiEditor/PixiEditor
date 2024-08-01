using PixiEditor.Models.Commands.Templates.Providers.Parsers;

namespace PixiEditor.Models.Commands.Templates;

public interface ICustomShortcutFormat
{
    public KeysParser KeysParser { get; }
    public string[] CustomShortcutExtensions { get; }
}
