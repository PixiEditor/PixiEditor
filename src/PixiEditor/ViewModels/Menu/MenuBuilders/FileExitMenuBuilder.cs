using System.Collections.Generic;
using Avalonia.Controls;
using PixiEditor.Extensions.UI;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.UI.Common.Localization;
using PixiEditor.Views;

namespace PixiEditor.ViewModels.Menu.MenuBuilders;

internal class FileExitMenuBuilder : MenuItemBuilder
{
    public override void ModifyMenuTree(ICollection<MenuItem> tree)
    {
        if (TryFindMenuItem(tree, "FILE", out MenuItem? fileMenuItem))
        {
            var exitMenuItem = new MenuItem
            {
                Command = SystemCommands.CloseWindowCommand,
                CommandParameter = MainWindow.Current,
                Icon = new Image()
                {
                    Source = PixiPerfectIcons.ToIcon(PixiPerfectIcons.Exit),
                    Width = Models.Commands.XAML.Menu.IconDimensions,
                    Height = Models.Commands.XAML.Menu.IconDimensions
                }
            };

            Translator.SetKey(exitMenuItem, "EXIT");

            fileMenuItem!.Items.Add(new Separator());
            fileMenuItem!.Items.Add(exitMenuItem);
        }
    }

    public override void ModifyMenuTree(ICollection<NativeMenuItem> tree)
    {
        return; // macOS has default exit button
    }
}
