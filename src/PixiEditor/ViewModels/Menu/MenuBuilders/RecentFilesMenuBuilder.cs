using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Styling;
using PixiEditor.Extensions.UI;
using PixiEditor.Models.UserData;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.SubViewModels;

namespace PixiEditor.ViewModels.Menu.MenuBuilders;

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
            var recentFilesMenuItem = new MenuItem();

            Translator.SetKey(recentFilesMenuItem, "RECENT");

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

    public override void ModifyMenuTree(ICollection<NativeMenuItem> tree)
    {
        if(TryFindMenuItem(tree, "FILE", out NativeMenuItem? fileMenuItem))
        {
            var recentFilesMenuItem = new NativeMenuItem();
            recentFilesMenuItem.Menu = new NativeMenu();

            Translator.SetKey(recentFilesMenuItem, "RECENT");
            Models.Commands.XAML.NativeMenu.SetLocalizationKeyHeader(recentFilesMenuItem, "RECENT");

            foreach (var recent in fileViewModel.RecentlyOpened)
            {
                recentFilesMenuItem.Menu.Add(new NativeMenuItem()
                {
                    Header = recent.FilePath,
                    Command = (ICommand)new Models.Commands.XAML.Command("PixiEditor.File.OpenRecent") { UseProvided = true }.ProvideValue(null),
                    CommandParameter = recent.FilePath
                });
            }

            recentFilesMenuItem.IsEnabled = fileViewModel.HasRecent;
            fileMenuItem!.Menu.Items.Add(recentFilesMenuItem);
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
                Classes = { "pixi-icon" },
                FontSize = 16,
                Content = PixiPerfectIcons.Exit,
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
