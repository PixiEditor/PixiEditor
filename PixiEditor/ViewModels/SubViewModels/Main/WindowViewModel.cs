using AvalonDock.Layout;
using GalaSoft.MvvmLight.CommandWpf;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class WindowViewModel : SubViewModel<ViewModelMain>
    {
        public RelayCommand<string> ShowAvalonDockWindowCommand { get; set; }

        public WindowViewModel()
            : this(null)
        {
        }

        public WindowViewModel(ViewModelMain owner)
            : base(owner)
        {
            ShowAvalonDockWindowCommand = new(ShowAvalonDockWindow);
        }

        [Command.Basic("PixiEditor.Settings.Open", "Open Settings", "Open Settings Window")]
        public static void OpenSettingsWindow()
        {
            SettingsWindow settings = new SettingsWindow();
            settings.Show();
        }

        [Command.Basic("PixiEditor.Window.OpenStartupWindow", "Open Settings", "Open Settings Window")]
        public void OpenHelloThereWindow()
        {
            new HelloTherePopup(Owner.FileSubViewModel).Show();
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
