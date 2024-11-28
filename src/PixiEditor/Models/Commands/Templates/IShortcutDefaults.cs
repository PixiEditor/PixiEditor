using System.Collections.Generic;

namespace PixiEditor.Models.Commands.Templates;

internal interface IShortcutDefaults
{
    List<Shortcut> DefaultShortcuts { get; }
}
