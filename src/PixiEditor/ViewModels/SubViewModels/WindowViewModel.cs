using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using PixiDocks.Core.Docking;
using PixiEditor.Models.AnalyticsAPI;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Handlers;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.UserPreferences;
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
internal class WindowViewModel : SubViewModel<ViewModelMain>, IWindowHandler
{
    private CommandController commandController;
    public RelayCommand<string> ShowAvalonDockWindowCommand { get; set; }
    public ObservableCollection<ViewportWindowViewModel> Viewports { get; } = new();
    public ObservableCollection<LazyViewportWindowViewModel> LazyViewports { get; } = new();
    public event EventHandler<ViewportWindowViewModel>? ActiveViewportChanged;
    public event Action<ViewportWindowViewModel> ViewportAdded;
    public event Action<ViewportWindowViewModel> ViewportClosed;

    public event Action<LazyViewportWindowViewModel> LazyViewportAdded;
    public event Action<LazyViewportWindowViewModel> LazyViewportRemoved;

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
            {
                Owner.LayoutSubViewModel.LayoutManager.ShowViewport(viewport);
                ActiveViewportChanged?.Invoke(this, viewport);
            }
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
        MenuItemPath = "VIEW/NEW_WINDOW_FOR_IMG", MenuItemOrder = 0, AnalyticsTrack = true)]
    public void CreateNewViewport()
    {
        var doc = ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;
        CreateNewViewport(doc);
    }

    [Commands_Command.Basic("PixiEditor.Window.CenterActiveViewport", "CENTER_ACTIVE_VIEWPORT",
        "CENTER_ACTIVE_VIEWPORT", CanExecute = "PixiEditor.HasDocument",
        Icon = PixiPerfectIcons.Center, AnalyticsTrack = true)]
    public void CenterCurrentViewport()
    {
        if (ActiveWindow is ViewportWindowViewModel viewport)
            viewport.CenterViewportTrigger.Execute(this, viewport.Document.SizeBindable);
    }


    [Command.Basic("PixiEditor.Viewport.ToggleHud", "TOGGLE_HUD", "TOGGLE_HUD_DESCRIPTION",
        AnalyticsTrack = true, Key = Key.H, Modifiers = KeyModifiers.Shift, MenuItemPath = "VIEW/TOGGLE_HUD")]
    public void ToggleHudOfCurrentViewport()
    {
        if (ActiveWindow is ViewportWindowViewModel viewport)
        {
            viewport.HudVisible = !viewport.HudVisible;
        }
    }

    [Commands_Command.Basic("PixiEditor.Window.FlipHorizontally", "FLIP_VIEWPORT_HORIZONTALLY",
        "FLIP_VIEWPORT_HORIZONTALLY", CanExecute = "PixiEditor.HasDocument",
        Icon = PixiPerfectIcons.YFlip, AnalyticsTrack = true)]
    public void FlipViewportHorizontally()
    {
        if (ActiveWindow is ViewportWindowViewModel viewport)
        {
            viewport.FlipX = !viewport.FlipX;
        }
    }

    [Commands_Command.Basic("PixiEditor.Window.FlipVertically", "FLIP_VIEWPORT_VERTICALLY", "FLIP_VIEWPORT_VERTICALLY",
        CanExecute = "PixiEditor.HasDocument",
        Icon = PixiPerfectIcons.XFlip, AnalyticsTrack = true)]
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

    public void CreateNewViewport(LazyDocumentViewModel lazyDoc)
    {
        LazyViewportWindowViewModel newViewport = new LazyViewportWindowViewModel(this, lazyDoc);
        LazyViewports.Add(newViewport);

        LazyViewportAdded?.Invoke(newViewport);
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

    public void MakeDocumentViewportActive(LazyDocumentViewModel? doc)
    {
        if (doc is null)
        {
            ActiveWindow = null;
            return;
        }

        ActiveWindow = LazyViewports.FirstOrDefault(viewport => viewport.LazyDocument == doc);
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
            Analytics.SendCloseDocument();
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

    public void OnLazyViewportWindowCloseButtonPressed(LazyViewportWindowViewModel viewport)
    {
        LazyViewports.Remove(viewport);
        LazyViewportRemoved?.Invoke(viewport);
        Owner.CloseLazyDocument(viewport.LazyDocument);
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

    public void CloseViewportForLazyDocument(LazyDocumentViewModel lazyDoc)
    {
        if (lazyDoc is null)
            return;

        var viewport = LazyViewports.FirstOrDefault(vp => vp.LazyDocument == lazyDoc);
        if (viewport is not null)
        {
            LazyViewports.Remove(viewport);
            LazyViewportRemoved?.Invoke(viewport);
        }
    }

    [Commands_Command.Basic("PixiEditor.Window.OpenSettingsWindow", "OPEN_SETTINGS", "OPEN_SETTINGS_DESCRIPTIVE",
        Key = Key.OemComma, Modifiers = KeyModifiers.Control,
        MenuItemPath = "EDIT/SETTINGS", MenuItemOrder = 16, Icon = PixiPerfectIcons.Settings, AnalyticsTrack = true)]
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
        Icon = PixiPerfectIcons.Home, MenuItemPath = "VIEW/OPEN_STARTUP_WINDOW", MenuItemOrder = 1,
        AnalyticsTrack = true)]
    public void OpenHelloThereWindow()
    {
        new HelloTherePopup(Owner.FileSubViewModel).Show(MainWindow.Current);
    }

    [Command.Basic("PixiEditor.Window.OpenOnboardingWindow", "OPEN_ONBOARDING_WINDOW", "OPEN_ONBOARDING_WINDOW",
        Icon = PixiPerfectIcons.Compass, MenuItemPath = "VIEW/OPEN_ONBOARDING_WINDOW", MenuItemOrder = 2,
        AnalyticsTrack = true)]
    public void OpenOnboardingWindow()
    {
        new OnboardingDialog { DataContext = new OnboardingViewModel() }.ShowDialog(MainWindow.Current);
    }

    [Commands_Command.Basic("PixiEditor.Window.OpenShortcutWindow", "OPEN_SHORTCUT_WINDOW", "OPEN_SHORTCUT_WINDOW",
        Key = Key.F1,
        Icon = PixiPerfectIcons.Book, MenuItemPath = "VIEW/OPEN_SHORTCUT_WINDOW", MenuItemOrder = 2,
        AnalyticsTrack = true)]
    public void ShowShortcutWindow()
    {
        var popup = new ShortcutsPopup(commandController);
        popup.Show();
        popup.Activate();
    }

    [Commands_Command.Basic("PixiEditor.Window.OpenAboutWindow", "OPEN_ABOUT_WINDOW", "OPEN_ABOUT_WINDOW",
        Icon = PixiPerfectIcons.Info, MenuItemPath = "HELP/ABOUT", MenuItemOrder = 5, AnalyticsTrack = true)]
    public void OpenAboutWindow()
    {
        new AboutPopup().Show();
    }

    [Commands_Command.Internal("PixiEditor.Window.ShowDockWindow")]
    [Commands_Command.Basic("PixiEditor.Window.OpenPreviewWindow", "DocumentPreview", "OPEN_PREVIEW_WINDOW",
        "OPEN_PREVIEW_WINDOW")]
    public void ShowDockWindow(string id)
    {
        Owner.LayoutSubViewModel.LayoutManager.ShowDockable(id);
    }
}
