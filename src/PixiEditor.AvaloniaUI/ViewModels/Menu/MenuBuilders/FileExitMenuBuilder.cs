using System.Collections.Generic;
using Avalonia.Controls;
using PixiEditor.AvaloniaUI.Views;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.UI;

namespace PixiEditor.AvaloniaUI.ViewModels.Menu.MenuBuilders;

internal class FileExitMenuBuilder : MenuItemBuilder
{
    public override void ModifyMenuTree(ICollection<MenuItem> tree)
    {
        if (TryFindMenuItem(tree, "FILE", out MenuItem? fileMenuItem))
        {
            var exitMenuItem = new MenuItem
            {
                Command = SystemCommands.CloseWindowCommand,
                CommandParameter = MainWindow.Current
            };

            Translator.SetKey(exitMenuItem, "EXIT");

            fileMenuItem!.Items.Add(new Separator());
            fileMenuItem!.Items.Add(exitMenuItem);
        }
    }
}
