using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using PixiEditor.Helpers;
using PixiEditor.Models.Dialogs;
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
        }

        private static void AskToInstall()
        {
#if RELEASE
            if (IPreferences.Current.GetPreference("CheckUpdatesOnStartup", true))
            {
                string dir = AppDomain.CurrentDomain.BaseDirectory;
                UpdateDownloader.CreateTempDirectory();
                bool updateZipExists = Directory.GetFiles(UpdateDownloader.DownloadLocation, "update-*.zip").Length > 0;
                string[] updateExeFiles = Directory.GetFiles(UpdateDownloader.DownloadLocation, "update-*.exe");
                bool updateExeExists = updateExeFiles.Length > 0;

                string updaterPath = Path.Join(dir, "PixiEditor.UpdateInstaller.exe");

                if (updateZipExists || updateExeExists)
                {
                    ViewModelMain.Current.UpdateSubViewModel.UpdateReadyToInstall = true;
                    var result = ConfirmationDialog.Show("Update is ready to install. Do you want to install it now?");
                    if (result == Models.Enums.ConfirmationType.Yes)
                    {
                        if (updateZipExists && File.Exists(updaterPath))
                        {
                            InstallHeadless(updaterPath);
                        }
                        else if (updateExeExists)
                        {
                            OpenExeInstaller(updateExeFiles[0]);
                        }
                    }
                }
            }
#endif
        }

        private static void InstallHeadless(string updaterPath)
        {
            try
            {
                ProcessHelper.RunAsAdmin(updaterPath);
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

        private static void OpenExeInstaller(string updateExeFile)
        {
            bool alreadyUpdated = AssemblyHelper.GetCurrentAssemblyVersion().ToString() ==
                    updateExeFile.Split('-')[1].Split(".exe")[0];

            if (!alreadyUpdated)
            {
                RestartToUpdate(updateExeFile);
            }
            else
            {
                File.Delete(updateExeFile);
            }
        }

        private static void RestartToUpdate(string updateExeFile)
        {
            Process.Start(updateExeFile);
            Application.Current.Shutdown();
        }

        private static void RestartApplication(object parameter)
        {
            try
            {
                ProcessHelper.RunAsAdmin(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "PixiEditor.UpdateInstaller.exe"));
                Application.Current.Shutdown();
            }
            catch (Win32Exception)
            {
                NoticeDialog.Show("Couldn't update without administrator rights.", "Insufficient permissions");
            }
        }

        private void Owner_OnStartupEvent(object sender, EventArgs e)
        {
            ConditionalUPDATE();
        }

        [Conditional("UPDATE")]
        private async void ConditionalUPDATE()
        {
            if (IPreferences.Current.GetPreference("CheckUpdatesOnStartup", true))
            {
                try
                {
                    await CheckForUpdate();
                }
                catch (System.Net.Http.HttpRequestException)
                {
                    NoticeDialog.Show("Could not check if there's an update available");
                }

                AskToInstall();
            }
        }

        private void InitUpdateChecker()
        {
            string version = AssemblyHelper.GetCurrentAssemblyVersion().ToString();
            UpdateChecker = new UpdateChecker(version);
            VersionText = $"Version {version}";
        }
    }
}