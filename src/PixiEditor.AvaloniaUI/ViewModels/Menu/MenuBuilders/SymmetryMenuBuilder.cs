using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using PixiEditor.AvaloniaUI.Helpers.Converters;
using PixiEditor.AvaloniaUI.Views.Input;
using PixiEditor.Extensions.UI;

namespace PixiEditor.AvaloniaUI.ViewModels.Menu.MenuBuilders;

internal class SymmetryMenuBuilder : MenuItemBuilder
{
    public override void ModifyMenuTree(ICollection<MenuItem> tree)
    {
        if (TryFindMenuItem(tree, "IMAGE", out MenuItem? viewItem))
        {
            int index = viewItem!.Items.Count >= 3 ? 3 : viewItem.Items.Count - 1;
            viewItem!.Items.Insert(index, new Separator());
            ToggleableMenuItem horizontalSymmetryItem = new ToggleableMenuItem();
            Translator.SetKey(horizontalSymmetryItem, "HORIZONTAL_LINE_SYMMETRY");
            horizontalSymmetryItem.Icon = new Image()
            {
                Source = ImagePathToBitmapConverter.LoadBitmapFromRelativePath("/Images/SymmetryHorizontal.png"),
                Width = Models.Commands.XAML.Menu.IconDimensions,
                Height = Models.Commands.XAML.Menu.IconDimensions
            };

            BindItem(horizontalSymmetryItem, "DocumentManagerSubViewModel.ActiveDocument.HorizontalSymmetryAxisEnabledBindable");
            viewItem.Items.Insert(index + 1, horizontalSymmetryItem);

            ToggleableMenuItem verticalSymmetryItem = new ToggleableMenuItem();
            Translator.SetKey(verticalSymmetryItem, "VERTICAL_LINE_SYMMETRY");
            verticalSymmetryItem.Icon = new Image()
            {
                Source = ImagePathToBitmapConverter.LoadBitmapFromRelativePath("/Images/SymmetryVertical.png"),
                Width = Models.Commands.XAML.Menu.IconDimensions,
                Height = Models.Commands.XAML.Menu.IconDimensions
            };

            BindItem(verticalSymmetryItem, "DocumentManagerSubViewModel.ActiveDocument.VerticalSymmetryAxisEnabledBindable");
            viewItem.Items.Insert(index + 2, verticalSymmetryItem);
            viewItem.Items.Insert(index + 3, new Separator());
        }
    }

    private void BindItem(ToggleableMenuItem item, string checkedBindingPath)
    {
        Binding isEnabledBinding = new Binding("!!DocumentManagerSubViewModel.ActiveDocument")
        {
            Source = ViewModelMain.Current
        };

        Binding isCheckedBinding = new Binding(checkedBindingPath)
        {
            Source = ViewModelMain.Current,
            Mode = BindingMode.TwoWay
        };

        item.Bind(ToggleableMenuItem.IsCheckedProperty, isCheckedBinding);
        item.Bind(InputElement.IsEnabledProperty, isEnabledBinding);
    }
}
