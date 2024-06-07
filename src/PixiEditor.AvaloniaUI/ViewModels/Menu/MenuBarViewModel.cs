using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.AvaloniaUI.Models.Commands;
using PixiEditor.AvaloniaUI.Models.Commands.XAML;
using PixiEditor.AvaloniaUI.ViewModels.SubViewModels.AdditionalContent;
using PixiEditor.Extensions.Common.Localization;
using Command = PixiEditor.AvaloniaUI.Models.Commands.Commands.Command;

namespace PixiEditor.AvaloniaUI.ViewModels.Menu;

internal class MenuBarViewModel : PixiObservableObject
{
    public AdditionalContentViewModel AdditionalContentSubViewModel { get; set; }
    public ObservableCollection<MenuItem> MenuEntries { get; set; } = new();

    private Dictionary<string, MenuTreeItem> menuItems = new();

    private readonly Dictionary<string, int> menuOrderMultiplier = new Dictionary<string, int>()
    {
        { "FILE", 100 },
        { "EDIT", 200 },
        { "SELECT", 300 },
        { "IMAGE", 400 },
        { "VIEW", 500 },
        { "HELP", 600 },
        { "DEBUG", 1000 },
    };

    public MenuBarViewModel(AdditionalContentViewModel? additionalContentSubViewModel)
    {
        AdditionalContentSubViewModel = additionalContentSubViewModel;
    }

    public void Init(IServiceProvider serviceProvider, CommandController controller)
    {
        MenuItemBuilder[] builders = serviceProvider.GetServices<MenuItemBuilder>().ToArray();

        var commandsWithMenuItems = controller.Commands.Where(x => !string.IsNullOrEmpty(x.MenuItemPath) && IsValid(x.MenuItemPath)).ToArray();

        foreach (var command in commandsWithMenuItems.OrderBy(GetCategoryMultiplier).ThenBy(x => x.MenuItemOrder).ThenBy(x => x.InternalName))
        {
           if(string.IsNullOrEmpty(command.MenuItemPath)) continue;

           BuildMenuEntry(command);
        }

        BuildMenu(controller, builders);
    }

    private int GetCategoryMultiplier(Command command)
    {
        string category = command.MenuItemPath!.Split('/')[0];
        return menuOrderMultiplier.GetValueOrDefault(category, 9999);
    }

    private bool IsValid(string argMenuItemPath)
    {
        return argMenuItemPath.Split('/').Length > 1;
    }

    private void BuildMenu(CommandController controller, MenuItemBuilder[] builders)
    {
        BuildSimpleItems(controller, menuItems);
        foreach (var builder in builders)
        {
            builder.ModifyMenuTree(MenuEntries);
        }
    }

    private void BuildSimpleItems(CommandController controller, Dictionary<string, MenuTreeItem> root, MenuItem? parent = null)
    {
        string? lastSubCommand = null;

        foreach (var item in root)
        {
            MenuItem menuItem = new()
            {
                Header = new LocalizedString(item.Key),
            };

            CommandGroup? group = controller.CommandGroups.FirstOrDefault(x => x.IsVisibleProperty != null && x.Commands.Contains(item.Value.Command));

            if (group != null)
            {
                menuItem.Bind(Visual.IsVisibleProperty, new Binding(group.IsVisibleProperty)
                {
                    Source = ViewModelMain.Current,
                });
            }

            if (item.Value.Items.Count == 0)
            {
                Models.Commands.XAML.Menu.SetCommand(menuItem, item.Value.Command.InternalName);

                string internalName = item.Value.Command.InternalName;
                internalName = internalName[..internalName.LastIndexOf('.')];

                if (lastSubCommand != null && lastSubCommand != internalName)
                {
                    parent?.Items.Add(new Separator());
                }

                if (parent != null)
                {
                    parent.Items.Add(menuItem);
                }
                else
                {
                    MenuEntries.Add(menuItem);
                }

                lastSubCommand = internalName;
            }
            else
            {
                if (parent != null)
                {
                    parent.Items.Add(menuItem);
                }
                else
                {
                    MenuEntries.Add(menuItem);
                }
                BuildSimpleItems(controller, item.Value.Items, menuItem);
            }
        }
    }

    private void BuildMenuEntry(Command command)
    {
        string[] path = command.MenuItemPath!.Split('/');
        MenuTreeItem current = null;

        for (int i = 0; i < path.Length; i++)
        {
            if (current == null)
            {
                if (!menuItems.ContainsKey(path[i]))
                {
                    menuItems.Add(path[i], new MenuTreeItem(path[i], command));
                }
                current = menuItems[path[i]];
            }
            else
            {
                if (!current.Items.ContainsKey(path[i]))
                {
                    current.Items.Add(path[i], new MenuTreeItem(path[i], command));
                }
                current = current.Items[path[i]];
            }
        }
    }
}
