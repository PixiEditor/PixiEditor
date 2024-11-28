using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using PixiEditor.Extensions.UI;
using PixiEditor.UI.Common.Controls;
using PixiEditor.UI.Common.Fonts;

namespace PixiEditor.ViewModels.Menu.MenuBuilders;

internal class ToggleSnappingMenuBuilder : MenuItemBuilder
{
    public override void ModifyMenuTree(ICollection<MenuItem> tree)
    {
        if (TryFindMenuItem(tree, "VIEW", out MenuItem? viewItem))
        {
            ToggleableMenuItem snappingItem = new ToggleableMenuItem();
            Translator.SetKey(snappingItem, "TOGGLE_SNAPPING");
            snappingItem.Icon = new Image()
            {
                Source = PixiPerfectIcons.ToIcon(PixiPerfectIcons.Snapping),
                Width = Models.Commands.XAML.Menu.IconDimensions,
                Height = Models.Commands.XAML.Menu.IconDimensions
            };

            BindItem(snappingItem);
            viewItem.Items.Add(snappingItem);
        }
    }

    private void BindItem(ToggleableMenuItem gridLinesItem)
    {
        gridLinesItem.Bind(ToggleableMenuItem.IsCheckedProperty, new Binding("ViewportSubViewModel.SnappingEnabled")
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
