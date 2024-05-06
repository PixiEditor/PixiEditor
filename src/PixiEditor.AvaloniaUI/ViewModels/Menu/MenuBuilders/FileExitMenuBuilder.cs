using System.Collections.Generic;
using Avalonia.Controls;
using PixiEditor.AvaloniaUI.Views;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.AvaloniaUI.ViewModels.Menu.MenuBuilders;

internal class FileExitMenuBuilder : MenuItemBuilder
{
    public override void ModifyMenuTree(ICollection<MenuItem> tree)
    {
        if (TryFindMenuItem(tree, "FILE", out MenuItem? fileMenuItem))
        {
            var exitMenuItem = new MenuItem
            {
                Header = new LocalizedString("EXIT"),
                Command = SystemCommands.CloseWindowCommand,
                CommandParameter = MainWindow.Current
            };

            fileMenuItem!.Items.Add(new Separator());
            fileMenuItem!.Items.Add(exitMenuItem);
        }
    }
}
