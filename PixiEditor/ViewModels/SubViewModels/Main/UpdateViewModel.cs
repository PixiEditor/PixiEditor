using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using PixiEditor.Helpers;
using PixiEditor.Models.Processes;
using PixiEditor.Models.UserPreferences;
using PixiEditor.UpdateModule;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class UpdateViewModel : SubViewModel<ViewModelMain>
    {
        private bool updateReadyToInstall = false;

        public UpdateChecker UpdateChecker { get; set; }

        public RelayCommand RestartApplicationCommand { get; set; }

        private string versionText;

        public string VersionText
        {
            get => versionText;
            set
            {
                versionText = value;
                RaisePropertyChanged(nameof(VersionText));
            }
        }

        public bool UpdateReadyToInstall
        {
            get => updateReadyToInstall;
            set
            {
                updateReadyToInstall = value;
                RaisePropertyChanged(nameof(UpdateReadyToInstall));
                if (value)
                {
                    VersionText = $"to install update (current {UpdateChecker.CurrentVersionTag})"; // Button shows "Restart" before this text
                }
            }
        }

        public UpdateViewModel(ViewModelMain owner)
            : base(owner)
        {
            Owner.OnStartupEvent += Owner_OnStartupEvent;
            RestartApplicationCommand = new RelayCommand(RestartApplication);
            InitUpdateChecker();
        }

        public async Task<bool> CheckForUpdate()
        {
            return await Task.Run(async () =>
            {
                bool updateAvailable = await UpdateChecker.CheckUpdateAvailable();
                bool updateCompatible = await UpdateChecker.IsUpdateCompatible();
                bool updateFileDoesNotExists = !File.Exists(
                    Path.Join(UpdateDownloader.DownloadLocation, $"update-{UpdateChecker.LatestReleaseInfo.TagName}.zip"));
                bool updateExeDoesNotExists = !File.Exists(
                    Path.Join(UpdateDownloader.DownloadLocation, $"update-{UpdateChecker.LatestReleaseInfo.TagName}.exe"));
                if (updateAvailable && updateFileDoesNotExists && updateExeDoesNotExists)
                {
                    VersionText = "Downloading update...";
                    if (updateCompatible)
                    {
                        await UpdateDownloader.DownloadReleaseZip(UpdateChecker.LatestReleaseInfo);
                    }
                    else
                    {
                        await UpdateDownloader.DownloadInstaller(UpdateChecker.LatestReleaseInfo);
                    }

                    UpdateReadyToInstall = true;
                    return true;
                }

                return false;
            });
        }

        private async void Owner_OnStartupEvent(object sender, EventArgs e)
        {
            if (PreferencesSettings.GetPreference<bool>("CheckUpdatesOnStartup"))
            {
                await CheckForUpdate();
            }
        }

        private void RestartApplication(object parameter)
        {
            try
            {
                ProcessHelper.RunAsAdmin(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "PixiEditor.UpdateInstaller.exe"));
                Application.Current.Shutdown();
            }
            catch (Win32Exception)
            {
                MessageBox.Show(
                    "Couldn't update without administrator rights.",
                    "Insufficient permissions",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void InitUpdateChecker()
        {
            string version = AssemblyHelper.GetCurrentAssemblyVersion();
            UpdateChecker = new UpdateChecker(version);
            VersionText = $"Version {version}";
        }
    }
}