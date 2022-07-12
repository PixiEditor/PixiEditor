using System.Windows.Input;
using AvalonDock.Layout;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands;
using PixiEditor.Views;
using PixiEditor.Views.Dialogs;
using Command = PixiEditor.Models.Commands.Attributes.Commands.Command;

namespace PixiEditor.ViewModels.SubViewModels.Main;

[Command.Group("PixiEditor.Window", "Windows")]
internal class WindowViewModel : SubViewModel<ViewModelMain>
{
    private CommandController commandController;
    private ShortcutPopup shortcutPopup;

    private ShortcutPopup ShortcutPopup => shortcutPopup ?? (shortcutPopup = new(commandController));

    public RelayCommand<string> ShowAvalonDockWindowCommand { get; set; }

    public WindowViewModel(ViewModelMain owner, CommandController commandController)
        : base(owner)
    {
        ShowAvalonDockWindowCommand = new(ShowAvalonDockWindow);
        this.commandController = commandController;
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
