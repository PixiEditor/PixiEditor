using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Extensions.UI;
using PixiEditor.Helpers.Extensions;
using PixiEditor.UI.Common.Controls;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.UI.Common.Localization;

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

    public override void ModifyMenuTree(ICollection<NativeMenuItem> tree)
    {
        if (TryFindMenuItem(tree, "VIEW", out NativeMenuItem? viewItem))
        {
            viewItem.Menu.Items.Add(new NativeMenuItemSeparator());
            NativeMenuItem gridLinesItem = new NativeMenuItem();
            gridLinesItem.ToggleType = NativeMenuItemToggleType.CheckBox;
            Translator.SetKey(gridLinesItem, "TOGGLE_SNAPPING");

            gridLinesItem.Icon = PixiPerfectIcons.ToIcon(PixiPerfectIcons.Snapping).ToBitmap(IconDimensions);
            BindItem(gridLinesItem);
            viewItem.Menu.Items.Add(gridLinesItem);
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
    
    private void BindItem(NativeMenuItem gridLinesItem)
    {
        gridLinesItem.Bind(NativeMenuItem.IsCheckedProperty, new Binding("ViewportSubViewModel.SnappingEnabled")
        {
            Source = ViewModelMain.Current,
        });

        gridLinesItem.Bind(NativeMenuItem.IsEnabledProperty, new Binding("!!DocumentManagerSubViewModel.ActiveDocument")
        {
            Source = ViewModelMain.Current
        });
        
        gridLinesItem.Command = new RelayCommand(() =>
        {
            ViewModelMain.Current.ViewportSubViewModel.SnappingEnabled = !ViewModelMain.Current.ViewportSubViewModel.SnappingEnabled;
        });
    }
}
