using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Models.Commands.XAML;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.UI;
using PixiEditor.Helpers.UI;
using PixiEditor.Models.Commands;
using PixiEditor.OperatingSystem;
using PixiEditor.ViewModels.SubViewModels;
using PixiEditor.ViewModels.SubViewModels.AdditionalContent;
using Command = PixiEditor.Models.Commands.Commands.Command;
using Commands_Command = PixiEditor.Models.Commands.Commands.Command;
using NativeMenu = Avalonia.Controls.NativeMenu;

namespace PixiEditor.ViewModels.Menu;

internal class MenuBarViewModel : PixiObservableObject
{
    private AdditionalContentViewModel additionalContentViewModel;
    private UpdateViewModel updateViewModel;
    private UserViewModel userViewModel;
    private ExecutionTrigger _openPixiEditorMenuTrigger;

    public AdditionalContentViewModel AdditionalContentSubViewModel
    {
        get => additionalContentViewModel;
        set => SetProperty(ref additionalContentViewModel, value);
    }

    public UpdateViewModel UpdateViewModel
    {
        get => updateViewModel;
        set => SetProperty(ref updateViewModel, value);
    }

    public UserViewModel UserViewModel
    {
        get => userViewModel;
        set => SetProperty(ref userViewModel, value);
    }

    public ObservableCollection<MenuItem>? MenuEntries { get; set; }
    public NativeMenu? NativeMenu { get; private set; }

    public ExecutionTrigger OpenPixiEditorMenuTrigger
    {
        get => _openPixiEditorMenuTrigger;
        set => SetProperty(ref _openPixiEditorMenuTrigger, value);
    }

    private Dictionary<string, MenuTreeItem> menuItems = new();
    private List<NativeMenuItem> nativeMenuItems;


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

    public MenuBarViewModel(AdditionalContentViewModel? additionalContentSubViewModel, UpdateViewModel? updateViewModel,
        UserViewModel? userViewModel)
    {
        AdditionalContentSubViewModel = additionalContentSubViewModel;
        UpdateViewModel = updateViewModel;
        UserViewModel = userViewModel;
        OpenPixiEditorMenuTrigger = new ExecutionTrigger();
    }

    public void Init(IServiceProvider serviceProvider, CommandController controller)
    {
        MenuItemBuilder[] builders = serviceProvider.GetServices<MenuItemBuilder>().ToArray();

        var commandsWithMenuItems = controller.Commands
            .Where(x => !string.IsNullOrEmpty(x.MenuItemPath) && IsValid(x.MenuItemPath)).ToArray();

        foreach (var command in commandsWithMenuItems.OrderBy(GetCategoryMultiplier).ThenBy(x => x.MenuItemOrder)
                     .ThenBy(x => x.InternalName))
        {
            if (string.IsNullOrEmpty(command.MenuItemPath)) continue;

            BuildMenuEntry(command);
        }

        BuildMenu(controller, builders);

        if (!UpdateViewModel.IsUpdateAvailable)
        {
            UpdateViewModel.PropertyChanged += UpdateViewModelChanged;
        }
        else
        {
            Dispatcher.UIThread.Post(() =>
            {
                OpenPixiEditorMenuTrigger.Execute(this);
            });
        }
    }

    private void UpdateViewModelChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(UpdateViewModel.IsUpdateAvailable))
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (UpdateViewModel.IsUpdateAvailable)
                {
                    UpdateViewModel.PropertyChanged -= UpdateViewModelChanged;
                    OpenPixiEditorMenuTrigger.Execute(this);
                }
            });
        }
    }

    private int GetCategoryMultiplier(Commands_Command command)
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
        if (IOperatingSystem.Current.IsMacOs)
        {
            BuildBasicNativeMenuItems(controller, menuItems);
            foreach (var builder in builders)
            {
                builder.ModifyMenuTree(nativeMenuItems);
            }

            NativeMenu = [];
            foreach (var item in nativeMenuItems)
            {
                NativeMenu.Items.Add(item);
            }
        }
        else
        {
            BuildSimpleItems(controller, menuItems);
            foreach (var builder in builders)
            {
                builder.ModifyMenuTree(MenuEntries);
            }
        }
    }

    private void BuildSimpleItems(CommandController controller, Dictionary<string, MenuTreeItem> root,
        MenuItem? parent = null)
    {
        string? lastSubCommand = null;
        MenuEntries ??= new ObservableCollection<MenuItem>();

        foreach (var item in root)
        {
            MenuItem menuItem = new();

            var headerBinding = new Binding(".") { Source = item.Key, Mode = BindingMode.OneWay, };

            menuItem.Bind(Translator.KeyProperty, headerBinding);

            CommandGroup? group = controller.CommandGroups.FirstOrDefault(x =>
                x.IsVisibleProperty != null && x.Commands.Contains(item.Value.Command));

            if (group != null)
            {
                menuItem.Bind(Visual.IsVisibleProperty,
                    new Binding(group.IsVisibleProperty) { Source = ViewModelMain.Current, });
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

    private void BuildBasicNativeMenuItems(CommandController controller, Dictionary<string, MenuTreeItem> root,
        NativeMenu? parent = null)
    {
        string? lastSubCommand = null;

        foreach (var item in root)
        {
            NativeMenuItem menuItem = new();

            nativeMenuItems ??= new List<NativeMenuItem>();
            var headerBinding = new Binding(".") { Source = item.Key, Mode = BindingMode.OneWay, };

            menuItem.Bind(Translator.KeyProperty, headerBinding);
            menuItem.Bind(PixiEditor.Models.Commands.XAML.NativeMenu.LocalizationKeyHeaderProperty, headerBinding);

            CommandGroup? group = controller.CommandGroups.FirstOrDefault(x =>
                x.IsVisibleProperty != null && x.Commands.Contains(item.Value.Command));

            if (group != null)
            {
                menuItem.Bind(
                    Visual.IsVisibleProperty,
                    new Binding(group.IsVisibleProperty) { Source = ViewModelMain.Current, });
            }

            if (item.Value.Items.Count == 0)
            {
                Models.Commands.XAML.NativeMenu.SetCommand(menuItem, item.Value.Command.InternalName);

                string internalName = item.Value.Command.InternalName;
                internalName = internalName[..internalName.LastIndexOf('.')];

                if (lastSubCommand != null && lastSubCommand != internalName)
                {
                    parent?.Items.Add(new NativeMenuItemSeparator());
                }

                if (parent != null)
                {
                    parent.Items.Add(menuItem);
                }
                else
                {
                    nativeMenuItems.Add(menuItem);
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
                    nativeMenuItems.Add(menuItem);
                }

                BuildBasicNativeMenuItems(controller, item.Value.Items, menuItem.Menu ??= []);
            }
        }
    }

    private void BuildMenuEntry(Commands_Command command)
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
