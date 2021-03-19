using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixiEditor.Helpers;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class MiscViewModel : SubViewModel<ViewModelMain>
    {
        public RelayCommand OpenHyperlinkCommand { get; set; }

        public RelayCommand OpenSettingsWindowCommand { get; set; }

        public RelayCommand OpenShortcutWindowCommand { get; set; }

        public ShortcutPopup ShortcutPopup { get; set; }

        public MiscViewModel(ViewModelMain owner)
            : base(owner)
        {
            OpenHyperlinkCommand = new RelayCommand(OpenHyperlink);
            OpenSettingsWindowCommand = new RelayCommand(OpenSettingsWindow);
            OpenShortcutWindowCommand = new RelayCommand(OpenShortcutWindow);

            ShortcutPopup = new ShortcutPopup(owner.ShortcutController);
        }

        private void OpenSettingsWindow(object parameter)
        {
            SettingsWindow settings = new SettingsWindow();
            settings.Show();
        }

        private void OpenHyperlink(object parameter)
        {
            if (parameter == null)
            {
                return;
            }

            var url = (string)parameter;
            var processInfo = new ProcessStartInfo()
            {
                FileName = url,
                UseShellExecute = true
            };
            Process.Start(processInfo);
        }

        private void OpenShortcutWindow(object parameter)
        {
            ShortcutPopup.Show();
        }
    }
}