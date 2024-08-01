using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using PixiDocks.Core.Docking;
using PixiEditor.Models.Commands;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Document;
using PixiEditor.Views;
using PixiEditor.Views.Dialogs;
using PixiEditor.Views.Windows;
using Command = PixiEditor.Models.Commands.Attributes.Commands.Command;
using Commands_Command = PixiEditor.Models.Commands.Attributes.Commands.Command;
using Settings_SettingsWindow = PixiEditor.Views.Windows.Settings.SettingsWindow;
using SettingsWindow = PixiEditor.Views.Windows.Settings.SettingsWindow;

namespace PixiEditor.ViewModels.SubViewModels;

#nullable enable
[Commands_Command.Group("PixiEditor.Window", "WINDOWS")]
internal class WindowViewModel : SubViewModel<ViewModelMain>
{
    private CommandController commandController;
    private ShortcutsPopup? shortcutPopup;
    private ShortcutsPopup ShortcutsPopup => shortcutPopup ??= new(commandController);
    public RelayCommand<string> ShowAvalonDockWindowCommand { get; set; }
    public ObservableCollection<ViewportWindowViewModel> Viewports { get; } = new();
    public event EventHandler<ViewportWindowViewModel>? ActiveViewportChanged;
    public event Action<ViewportWindowViewModel> ViewportAdded;
    public event Action<ViewportWindowViewModel> ViewportClosed;

    private object? activeWindow;
    public object? ActiveWindow
    {
        get => activeWindow;
        set
        {
            if (activeWindow == value)
                return;
            activeWindow = value;
            OnPropertyChanged(nameof(ActiveWindow));
            if (activeWindow is ViewportWindowViewModel viewport)
                ActiveViewportChanged?.Invoke(this, viewport);
        }
    }

    public WindowViewModel(ViewModelMain owner, CommandController commandController)
        : base(owner)
    {
        ShowAvalonDockWindowCommand = new(ShowDockWindow);
        this.commandController = commandController;
    }

    [Commands_Command.Basic("PixiEditor.Window.CreateNewViewport", "NEW_WINDOW_FOR_IMG", "NEW_WINDOW_FOR_IMG",
        Icon = PixiPerfectIcons.PlusSquare, CanExecute = "PixiEditor.HasDocument",
        MenuItemPath = "VIEW/NEW_WINDOW_FOR_IMG", MenuItemOrder = 0)]
    public void CreateNewViewport()
    {
        var doc = ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;
        CreateNewViewport(doc);
    }
    
    [Commands_Command.Basic("PixiEditor.Window.CenterActiveViewport", "CENTER_ACTIVE_VIEWPORT", "CENTER_ACTIVE_VIEWPORT", CanExecute = "PixiEditor.HasDocument",
        Icon = PixiPerfectIcons.Center)]
    public void CenterCurrentViewport()
    {
        if (ActiveWindow is ViewportWindowViewModel viewport)
            viewport.CenterViewportTrigger.Execute(this, viewport.Document.SizeBindable);
    }
    
    [Commands_Command.Basic("PixiEditor.Window.FlipHorizontally", "FLIP_VIEWPORT_HORIZONTALLY", "FLIP_VIEWPORT_HORIZONTALLY", CanExecute = "PixiEditor.HasDocument",
        Icon = PixiPerfectIcons.YFlip)]
    public void FlipViewportHorizontally()
    {
        if (ActiveWindow is ViewportWindowViewModel viewport)
        {
            viewport.FlipX = !viewport.FlipX;
        }
    }
    
    [Commands_Command.Basic("PixiEditor.Window.FlipVertically", "FLIP_VIEWPORT_VERTICALLY", "FLIP_VIEWPORT_VERTICALLY", CanExecute = "PixiEditor.HasDocument",
        Icon = PixiPerfectIcons.XFlip)]
    public void FlipViewportVertically()
    {
        if (ActiveWindow is ViewportWindowViewModel viewport)
        {
            viewport.FlipY = !viewport.FlipY;
        }
    }

    public void CreateNewViewport(DocumentViewModel doc)
    {
        ViewportWindowViewModel newViewport = new ViewportWindowViewModel(this, doc);
        Viewports.Add(newViewport);
        foreach (var viewport in Viewports.Where(vp => vp.Document == doc))
        {
            viewport.IndexChanged();
        }

        ViewportAdded?.Invoke(newViewport);
    }

    public void MakeDocumentViewportActive(DocumentViewModel? doc)
    {
        if (doc is null)
        {
            ActiveWindow = null;
            Owner.DocumentManagerSubViewModel.MakeActiveDocumentNull();
            return;
        }

        ActiveWindow = Viewports.FirstOrDefault(viewport => viewport.Document == doc);
    }

    public string CalculateViewportIndex(ViewportWindowViewModel viewport)
    {
        ViewportWindowViewModel[] viewports = Viewports.Where(a => a.Document == viewport.Document).ToArray();
        if (viewports.Length < 2)
            return "";
        return $"[{Array.IndexOf(viewports, viewport) + 1}]";
    }

    public async Task<bool> OnViewportWindowCloseButtonPressed(ViewportWindowViewModel viewport)
    {
        var viewports = Viewports.Where(vp => vp.Document == viewport.Document).ToArray();
        if (viewports.Length == 1)
        {
            return await Owner.DisposeDocumentWithSaveConfirmation(viewport.Document);
        }

        Viewports.Remove(viewport);

        foreach (var sibling in viewports)
        {
            sibling.IndexChanged();
        }

        ViewportClosed?.Invoke(viewport);

        return true;
    }

    public void CloseViewportsForDocument(DocumentViewModel document)
    {
        var viewports = Viewports.Where(vp => vp.Document == document).ToArray();
        foreach (ViewportWindowViewModel viewport in viewports)
        {
            Viewports.Remove(viewport);
            ViewportClosed?.Invoke(viewport);
        }
    }

    [Commands_Command.Basic("PixiEditor.Window.OpenSettingsWindow", "OPEN_SETTINGS", "OPEN_SETTINGS_DESCRIPTIVE", Key = Key.OemComma, Modifiers = KeyModifiers.Control,
        MenuItemPath = "EDIT/SETTINGS", MenuItemOrder = 16, Icon = PixiPerfectIcons.Settings)]
    public static void OpenSettingsWindow(int page)
    {
        if (page < 0)
        {
            page = 0;
        }

        var settings = new Settings_SettingsWindow(page);
        settings.Show();
    }

    [Commands_Command.Basic("PixiEditor.Window.OpenStartupWindow", "OPEN_STARTUP_WINDOW", "OPEN_STARTUP_WINDOW",
        Icon = PixiPerfectIcons.Home, MenuItemPath = "VIEW/OPEN_STARTUP_WINDOW", MenuItemOrder = 1)]
    public void OpenHelloThereWindow()
    {
        new HelloTherePopup(Owner.FileSubViewModel).Show(MainWindow.Current);
    }

    [Commands_Command.Basic("PixiEditor.Window.OpenShortcutWindow", "OPEN_SHORTCUT_WINDOW", "OPEN_SHORTCUT_WINDOW", Key = Key.F1,
        Icon = PixiPerfectIcons.Book, MenuItemPath = "VIEW/OPEN_SHORTCUT_WINDOW", MenuItemOrder = 2)]
    public void ShowShortcutWindow()
    {
        ShortcutsPopup.Show();
        ShortcutsPopup.Activate();
    }
        
    [Commands_Command.Basic("PixiEditor.Window.OpenAboutWindow", "OPEN_ABOUT_WINDOW", "OPEN_ABOUT_WINDOW",
        Icon = PixiPerfectIcons.Info, MenuItemPath = "HELP/ABOUT", MenuItemOrder = 5)]
    public void OpenAboutWindow()
    {
        new AboutPopup().Show();
    }

    [Commands_Command.Internal("PixiEditor.Window.ShowDockWindow")]
    [Commands_Command.Basic("PixiEditor.Window.OpenNavigationWindow", "Navigator", "OPEN_NAVIGATION_WINDOW", "OPEN_NAVIGATION_WINDOW")]
    public void ShowDockWindow(string id)
    {
        Owner.LayoutSubViewModel.LayoutManager.ShowDockable(id);
    }
}
