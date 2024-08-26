using System.Collections.Generic;
using PixiEditor.Models.Commands.Commands;

namespace PixiEditor.ViewModels.Menu;

internal class MenuTreeItem
{
    public string HeaderKey { get; set; }
    public Command Command { get; set; }
    public Dictionary<string, MenuTreeItem> Items { get; set; } = new();

    public MenuTreeItem(string headerKey, Command command)
    {
        HeaderKey = headerKey;
        Command = command;
    }
}
