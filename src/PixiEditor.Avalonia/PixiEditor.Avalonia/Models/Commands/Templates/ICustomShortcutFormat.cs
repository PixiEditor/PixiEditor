using PixiEditor.Models.Commands.Templates.Parsers;

namespace PixiEditor.Models.Commands.Templates;

public interface ICustomShortcutFormat
{
    public KeysParser KeysParser { get; }
    public string[] CustomShortcutExtensions { get; }
}
