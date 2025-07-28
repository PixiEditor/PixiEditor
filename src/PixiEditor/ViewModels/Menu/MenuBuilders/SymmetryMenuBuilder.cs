using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Helpers.Converters;
using PixiEditor.Views.Input;
using PixiEditor.Extensions.UI;
using PixiEditor.Helpers.Extensions;
using PixiEditor.UI.Common.Controls;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.ViewModels.Menu.MenuBuilders;

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
                Source = PixiPerfectIconExtensions.ToIcon(PixiPerfectIcons.XSymmetry),
                Width = Models.Commands.XAML.Menu.IconDimensions,
                Height = Models.Commands.XAML.Menu.IconDimensions
            };

            BindItem(horizontalSymmetryItem, "DocumentManagerSubViewModel.ActiveDocument.HorizontalSymmetryAxisEnabledBindable");
            viewItem.Items.Insert(index + 1, horizontalSymmetryItem);

            ToggleableMenuItem verticalSymmetryItem = new ToggleableMenuItem();
            Translator.SetKey(verticalSymmetryItem, "VERTICAL_LINE_SYMMETRY");
            verticalSymmetryItem.Icon = new Image()
            {
                Source = PixiPerfectIconExtensions.ToIcon(PixiPerfectIcons.YSymmetry),
                Width = Models.Commands.XAML.Menu.IconDimensions,
                Height = Models.Commands.XAML.Menu.IconDimensions
            };

            BindItem(verticalSymmetryItem, "DocumentManagerSubViewModel.ActiveDocument.VerticalSymmetryAxisEnabledBindable");
            viewItem.Items.Insert(index + 2, verticalSymmetryItem);
            viewItem.Items.Insert(index + 3, new Separator());
        }
    }

    public override void ModifyMenuTree(ICollection<NativeMenuItem> tree)
    {
        if(TryFindMenuItem(tree, "IMAGE", out NativeMenuItem? viewItem))
        {
            int index = viewItem!.Menu.Items.Count >= 3 ? 3 : viewItem.Menu.Items.Count - 1;
            viewItem!.Menu.Items.Insert(index, new NativeMenuItemSeparator());
            NativeMenuItem horizontalSymmetryItem = new NativeMenuItem();
            horizontalSymmetryItem.ToggleType = NativeMenuItemToggleType.CheckBox;
            
            PixelSize iconDimensions = new PixelSize((int)Models.Commands.XAML.Menu.IconDimensions, (int)Models.Commands.XAML.Menu.IconDimensions);
            
            Translator.SetKey(horizontalSymmetryItem, "HORIZONTAL_LINE_SYMMETRY");
            horizontalSymmetryItem.Icon = PixiPerfectIconExtensions.ToIcon(PixiPerfectIcons.XSymmetry)
                .ToBitmap(iconDimensions);

            BindItem(horizontalSymmetryItem, "DocumentManagerSubViewModel.ActiveDocument.HorizontalSymmetryAxisEnabledBindable",
                () =>
                {
                    var activeDocument = ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument;
                    if (activeDocument != null)
                    {
                        activeDocument.HorizontalSymmetryAxisEnabledBindable =
                            !activeDocument.HorizontalSymmetryAxisEnabledBindable;
                    }
                });
            viewItem.Menu.Items.Insert(index + 1, horizontalSymmetryItem);

            NativeMenuItem verticalSymmetryItem = new NativeMenuItem();
            Translator.SetKey(verticalSymmetryItem, "VERTICAL_LINE_SYMMETRY");
            verticalSymmetryItem.ToggleType = NativeMenuItemToggleType.CheckBox;
            verticalSymmetryItem.Icon = PixiPerfectIconExtensions.ToIcon(PixiPerfectIcons.YSymmetry)
                .ToBitmap(iconDimensions);

            BindItem(verticalSymmetryItem, "DocumentManagerSubViewModel.ActiveDocument.VerticalSymmetryAxisEnabledBindable",
                () =>
                {
                    var activeDocument = ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument;
                    if (activeDocument != null)
                    {
                        activeDocument.VerticalSymmetryAxisEnabledBindable =
                            !activeDocument.VerticalSymmetryAxisEnabledBindable;
                    }
                });
            viewItem.Menu.Items.Insert(index + 2, verticalSymmetryItem);
            viewItem.Menu.Items.Insert(index + 3, new NativeMenuItemSeparator());
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
    
    private void BindItem(NativeMenuItem item, string checkedBindingPath, Action updateAction)
    {
        Binding isEnabledBinding = new Binding("!!DocumentManagerSubViewModel.ActiveDocument")
        {
            Source = ViewModelMain.Current
        };

        Binding isCheckedBinding = new Binding(checkedBindingPath)
        {
            Source = ViewModelMain.Current,
        };

        item.Command = new RelayCommand(updateAction);
        
        item.Bind(NativeMenuItem.IsCheckedProperty, isCheckedBinding);
        item.Bind(NativeMenuItem.IsEnabledProperty, isEnabledBinding);
    }
}
