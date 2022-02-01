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

        public RelayCommand OpenHelloThereWindowCommand { get; set; }

        public ShortcutPopup ShortcutPopup { get; set; }

        public MiscViewModel(ViewModelMain owner)
            : base(owner)
        {
            OpenHyperlinkCommand = new RelayCommand(OpenHyperlink);
            OpenSettingsWindowCommand = new RelayCommand(OpenSettingsWindow);
            OpenShortcutWindowCommand = new RelayCommand(OpenShortcutWindow);
            OpenHelloThereWindowCommand = new RelayCommand(OpenHelloThereWindow);

            ShortcutPopup = new ShortcutPopup(owner.ShortcutController);
        }

        private void OpenSettingsWindow(object parameter)
        {
            SettingsWindow settings = new SettingsWindow();
            settings.Show();
        }

        private void OpenHyperlink(object parameter)
        {
            if (parameter is not string s)
            {
                return;
            }

            ProcessHelpers.ShellExecute(s);
        }

        private void OpenShortcutWindow(object parameter)
        {
            ShortcutPopup.Show();
        }

        private void OpenHelloThereWindow(object parameter)
        {
            new HelloTherePopup(Owner.FileSubViewModel).Show();
        }
    }
}