using System.Windows.Input;
using AvalonDock.Layout;
using GalaSoft.MvvmLight.CommandWpf;
using PixiEditor.Models.Commands;
using PixiEditor.Views.Dialogs;
using Command = PixiEditor.Models.Commands.Attributes.Command;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    [Command.Group("PixiEditor.Window", "Windows")]
    public class WindowViewModel : SubViewModel<ViewModelMain>
    {
        private ShortcutPopup shortcutPopup;
        
        public RelayCommand<string> ShowAvalonDockWindowCommand { get; set; }

        
        
        public WindowViewModel(ViewModelMain owner, CommandController commandController)
            : base(owner)
        {
            ShowAvalonDockWindowCommand = new(ShowAvalonDockWindow);
            shortcutPopup = new(commandController);
        }

        [Command.Basic("PixiEditor.Window.OpenSettingsWindow", "Open Settings", "Open Settings Window", Key = Key.OemComma, Modifiers = ModifierKeys.Control)]
        public static void OpenSettingsWindow()
        {
            var settings = new SettingsWindow();
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
            shortcutPopup.Show();
            shortcutPopup.Activate();
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
}
