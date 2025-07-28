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

internal class ToggleHighResPreviewMenuBuilder : MenuItemBuilder
{
    public override void ModifyMenuTree(ICollection<MenuItem> tree)
    {
        if (TryFindMenuItem(tree, "VIEW", out MenuItem? viewItem))
        {
            ToggleableMenuItem snappingItem = new ToggleableMenuItem();
            Translator.SetKey(snappingItem, "TOGGLE_HIGH_RES_PREVIEW");
            snappingItem.Icon = new Image()
            {
                Source = UI.Common.Fonts.PixiPerfectIconExtensions.ToIcon(PixiPerfectIcons.Circle),
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
            Translator.SetKey(gridLinesItem, "TOGGLE_HIGH_RES_PREVIEW");

            gridLinesItem.Icon = PixiPerfectIconExtensions.ToIcon(PixiPerfectIcons.Circle).ToBitmap(IconDimensions);
            BindItem(gridLinesItem);
            viewItem.Menu.Items.Add(gridLinesItem);
        }
    }

    private void BindItem(ToggleableMenuItem gridLinesItem)
    {
        gridLinesItem.Bind(ToggleableMenuItem.IsCheckedProperty, new Binding("ViewportSubViewModel.HighResRender")
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
        gridLinesItem.Bind(NativeMenuItem.IsCheckedProperty, new Binding("ViewportSubViewModel.HighResRender")
        {
            Source = ViewModelMain.Current,
        });

        gridLinesItem.Bind(NativeMenuItem.IsEnabledProperty, new Binding("!!DocumentManagerSubViewModel.ActiveDocument")
        {
            Source = ViewModelMain.Current
        });
        
        gridLinesItem.Command = new RelayCommand(() =>
        {
            ViewModelMain.Current.ViewportSubViewModel.HighResRender = !ViewModelMain.Current.ViewportSubViewModel.HighResRender;
        });
    }
}
