using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Styling;
using PixiEditor.AvaloniaUI.Models.UserData;
using PixiEditor.AvaloniaUI.ViewModels.SubViewModels;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.AvaloniaUI.ViewModels.Menu;

internal class RecentFilesMenuBuilder : MenuItemBuilder
{
    private readonly FileViewModel fileViewModel;

    public RecentFilesMenuBuilder(FileViewModel fileViewModel)
    {
        this.fileViewModel = fileViewModel;
    }

    public override void ModifyMenuTree(ICollection<MenuItem> tree)
    {
        if(TryFindMenuItem(tree, "FILE", out MenuItem? fileMenuItem))
        {
            var recentFilesMenuItem = new MenuItem
            {
                Header = new LocalizedString("RECENT")
            };

            Style style = new Style((selector => selector.OfType<MenuItem>()))
            {
                Setters =
                {
                    new Setter(MenuItem.CommandProperty, new Models.Commands.XAML.Command("PixiEditor.File.OpenRecent") { UseProvided = true }.ProvideValue(null)),
                    new Setter(MenuItem.CommandParameterProperty, new Binding() { Path = "FilePath"} )
                }
            };

            recentFilesMenuItem.ItemsSource = fileViewModel.RecentlyOpened;
            recentFilesMenuItem.Styles.Add(style);
            recentFilesMenuItem.IsEnabled = fileViewModel.HasRecent;
            recentFilesMenuItem.ItemTemplate = BuildItemTemplate();

            fileMenuItem!.Items.Add(recentFilesMenuItem);
        }
    }

    private IDataTemplate? BuildItemTemplate()
    {
        return new FuncDataTemplate<RecentlyOpenedDocument>((document, _) =>
        {
            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(),
                    new ColumnDefinition(new GridLength(1, GridUnitType.Auto))
                }
            };

            var filePathTextBlock = new TextBlock
            {
                [!TextBlock.TextProperty] = new Binding("FilePath")
            };

            var removeButton = new Button
            {
                Content = "",
                FontFamily = "{DynamicResource Feather}"
            };

            Style style = new Style((selector => selector.OfType<Button>()))
            {
                Setters =
                {
                    new Setter(Button.CommandProperty, new Models.Commands.XAML.Command("PixiEditor.File.RemoveRecent") { UseProvided = true }.ProvideValue(null)),
                    new Setter(Button.CommandParameterProperty, new Binding("FilePath"))
                }
            };

            removeButton.Styles.Add(style);

            grid.Children.Add(filePathTextBlock);
            grid.Children.Add(removeButton);
            Grid.SetColumn(removeButton, 1);

            return grid;
        });
    }
}
