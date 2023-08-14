using System.Collections.Generic;

namespace PixiEditor.AvaloniaUI.Models.Commands.Templates;

internal interface IShortcutDefaults
{
    List<Shortcut> DefaultShortcuts { get; }
}
