using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using PixiEditor.AvaloniaUI.Helpers.Converters;
using PixiEditor.AvaloniaUI.Views.Input;
using PixiEditor.Extensions.UI;
using PixiEditor.UI.Common.Fonts;

namespace PixiEditor.AvaloniaUI.ViewModels.Menu.MenuBuilders;

internal class ToggleGridLinesMenuBuilder : MenuItemBuilder
{
    public override void ModifyMenuTree(ICollection<MenuItem> tree)
    {
        if (TryFindMenuItem(tree, "VIEW", out MenuItem? viewItem))
        {
            viewItem!.Items.Add(new Separator());
            ToggleableMenuItem gridLinesItem = new ToggleableMenuItem();
            Translator.SetKey(gridLinesItem, "TOGGLE_GRIDLINES");
            gridLinesItem.Icon = new Image()
            {
                Source = PixiPerfectIcons.ToIcon(PixiPerfectIcons.Grid),
                Width = Models.Commands.XAML.Menu.IconDimensions,
                Height = Models.Commands.XAML.Menu.IconDimensions
            };

            BindItem(gridLinesItem);
            viewItem.Items.Add(gridLinesItem);
        }
    }

    private void BindItem(ToggleableMenuItem gridLinesItem)
    {
        gridLinesItem.Bind(ToggleableMenuItem.IsCheckedProperty, new Binding("ViewportSubViewModel.GridLinesEnabled")
        {
            Source = ViewModelMain.Current,
            Mode = BindingMode.TwoWay
        });

        gridLinesItem.Bind(InputElement.IsEnabledProperty, new Binding("!!DocumentManagerSubViewModel.ActiveDocument")
        {
            Source = ViewModelMain.Current
        });
    }
}
