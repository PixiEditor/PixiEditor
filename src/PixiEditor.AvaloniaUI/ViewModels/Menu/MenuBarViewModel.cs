using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.AvaloniaUI.Models.Commands;
using PixiEditor.AvaloniaUI.Models.Commands.XAML;
using PixiEditor.Extensions.Common.Localization;
using Command = PixiEditor.AvaloniaUI.Models.Commands.Commands.Command;

namespace PixiEditor.AvaloniaUI.ViewModels.Menu;

internal class MenuBarViewModel : PixiObservableObject
{
    public ObservableCollection<MenuItem> MenuEntries { get; set; } = new();

    private Dictionary<string, MenuTreeItem> menuItems = new();

    public void Init(IServiceProvider serviceProvider, CommandController controller)
    {
        MenuItemBuilder[] builders = serviceProvider.GetServices<MenuItemBuilder>().ToArray();
        foreach (var command in controller.Commands.OrderBy(x => x.MenuItemOrder).ThenBy(x => x.InternalName))
        {
           if(string.IsNullOrEmpty(command.MenuItemPath)) continue;

           BuildMenuEntry(command);
        }

        BuildMenu(builders);
    }

    private void BuildMenu(MenuItemBuilder[] builders)
    {
        BuildSimpleItems(menuItems);
        foreach (var builder in builders)
        {
            builder.ModifyMenuTree(MenuEntries);
        }
    }

    private void BuildSimpleItems(Dictionary<string, MenuTreeItem> root, MenuItem? parent = null)
    {
        string? lastSubCommand = null;

        foreach (var item in root)
        {
            MenuItem menuItem = new()
            {
                Header = new LocalizedString(item.Key),
            };

            if (item.Value.Items.Count == 0)
            {
                Models.Commands.XAML.Menu.SetCommand(menuItem, item.Value.Command.InternalName);

                string internalName = item.Value.Command.InternalName;
                internalName = internalName.Substring(0, internalName.LastIndexOf('.'));

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
                BuildSimpleItems(item.Value.Items, menuItem);
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
