using System.Collections.ObjectModel;
using System.Windows.Input;
using AvalonDock.Layout;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.Views;
using PixiEditor.Views.Dialogs;
using Command = PixiEditor.Models.Commands.Attributes.Commands.Command;

namespace PixiEditor.ViewModels.SubViewModels.Main;

#nullable enable
[Command.Group("PixiEditor.Window", "Windows")]
internal class WindowViewModel : SubViewModel<ViewModelMain>
{
    private CommandController commandController;
    private ShortcutPopup? shortcutPopup;
    private ShortcutPopup ShortcutPopup => shortcutPopup ?? (shortcutPopup = new(commandController));

    public RelayCommand<string> ShowAvalonDockWindowCommand { get; set; }
    public ObservableCollection<ViewportWindowViewModel> Viewports { get; } = new();
    public event EventHandler<ViewportWindowViewModel>? ActiveViewportChanged;

    private object? activeWindow;
    public object? ActiveWindow
    {
        get => activeWindow;
        set
        {
            if (activeWindow == value)
                return;
            activeWindow = value;
            RaisePropertyChanged(nameof(ActiveWindow));
            if (activeWindow is ViewportWindowViewModel viewport)
                ActiveViewportChanged?.Invoke(this, viewport);
        }
    }

    public WindowViewModel(ViewModelMain owner, CommandController commandController)
        : base(owner)
    {
        ShowAvalonDockWindowCommand = new(ShowAvalonDockWindow);
        this.commandController = commandController;
    }

    [Command.Basic("PixiEditor.Window.CreateNewViewport", "New window for current image", "New window for current image", CanExecute = "PixiEditor.HasDocument")]
    public void CreateNewViewport()
    {
        var doc = ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;
        CreateNewViewport(doc);
    }

    public void CreateNewViewport(DocumentViewModel doc)
    {
        Viewports.Add(new ViewportWindowViewModel(doc));
        foreach (var viewport in Viewports.Where(vp => vp.Document == doc))
        {
            viewport.RaisePropertyChanged(nameof(viewport.Index));
        }
    }

    public void MakeDocumentViewportActive(DocumentViewModel? doc)
    {
        if (doc is null)
        {
            ActiveWindow = null;
            Owner.DocumentManagerSubViewModel.MakeActiveDocumentNull();
            return;
        }
        ActiveWindow = Viewports.Where(viewport => viewport.Document == doc).FirstOrDefault();
    }

    public string CalculateViewportIndex(ViewportWindowViewModel viewport)
    {
        ViewportWindowViewModel[] viewports = Viewports.Where(a => a.Document == viewport.Document).ToArray();
        if (viewports.Length < 2)
            return "";
        return $"[{Array.IndexOf(viewports, viewport) + 1}]";
    }

    public void OnViewportWindowCloseButtonPressed(ViewportWindowViewModel viewport)
    {
        var viewports = Viewports.Where(vp => vp.Document == viewport.Document).ToArray();
        if (viewports.Length == 1)
        {
            Owner.DisposeDocumentWithSaveConfirmation(viewport.Document);
        }
        else
        {
            Viewports.Remove(viewport);
            foreach (var sibling in viewports)
            {
                sibling.RaisePropertyChanged(nameof(sibling.Index));
            }
        }
    }

    public void CloseViewportsForDocument(DocumentViewModel document)
    {
        var viewports = Viewports.Where(vp => vp.Document == document).ToArray();
        foreach (ViewportWindowViewModel viewport in viewports)
        {
            Viewports.Remove(viewport);
        }
    }

    [Command.Basic("PixiEditor.Window.OpenSettingsWindow", "Open Settings", "Open Settings Window", Key = Key.OemComma, Modifiers = ModifierKeys.Control)]
    public static void OpenSettingsWindow(string page)
    {
        if (string.IsNullOrWhiteSpace(page))
        {
            page = "General";
        }

        var settings = new SettingsWindow(page);
        settings.Show();
    }

    [Command.Basic("PixiEditor.Window.OpenStartupWindow", "Open Startup Window", "Open Startup Window")]
    public void OpenHelloThereWindow()
    {
        new HelloTherePopup(Owner.FileSubViewModel).Show();
    }

    [Command.Basic("PixiEditor.Window.OpenShortcutWindow", "Open Shortcut Window", "Open Shortcut Window", Key = Key.F1)]
    public void ShowShortcutWindow()
    {
        ShortcutPopup.Show();
        ShortcutPopup.Activate();
    }

    [Command.Basic("PixiEditor.Window.OpenNavigationWindow", "navigation", "Open Navigation Window", "Open Navigation Window")]
    public static void ShowAvalonDockWindow(string id)
    {
        if (MainWindow.Current?.LayoutRoot?.Manager?.Layout == null) return;
        var anchorables = new List<LayoutAnchorable>(MainWindow.Current.LayoutRoot.Manager.Layout
            .Descendents()
            .OfType<LayoutAnchorable>());

        foreach (var la in anchorables)
        {
            if (la.ContentId == id)
            {
                la.Show();
                la.IsActive = true;
            }
        }
    }
}
