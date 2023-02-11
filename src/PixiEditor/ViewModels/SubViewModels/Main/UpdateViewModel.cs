using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.UserPreferences;
using PixiEditor.UpdateModule;

namespace PixiEditor.ViewModels.SubViewModels.Main;

internal class UpdateViewModel : SubViewModel<ViewModelMain>
{
    private bool updateReadyToInstall = false;

    public UpdateChecker UpdateChecker { get; set; }

    public List<UpdateChannel> UpdateChannels { get; } = new List<UpdateChannel>();

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
                VersionText = $"to install update ({UpdateChecker.LatestReleaseInfo.TagName})"; // Button shows "Restart" before this text
            }
        }
    }

    public UpdateViewModel(ViewModelMain owner)
        : base(owner)
    {
        Owner.OnStartupEvent += Owner_OnStartupEvent;
        IPreferences.Current.AddCallback<string>("UpdateChannel", val =>
        {
            string prevChannel = UpdateChecker.Channel.ApiUrl;
            UpdateChecker.Channel = GetUpdateChannel(val);
            if (prevChannel != UpdateChecker.Channel.ApiUrl)
            {
                ConditionalUPDATE();
            }
        });
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
            UpdateReadyToInstall = false;
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

    private void AskToInstall()
    {
#if RELEASE || DEV_RELEASE
            if (IPreferences.Current.GetPreference("CheckUpdatesOnStartup", true))
            {
                string dir = AppDomain.CurrentDomain.BaseDirectory;
                
                UpdateDownloader.CreateTempDirectory();
                if(UpdateChecker.LatestReleaseInfo == null) return;
                bool updateFileExists = File.Exists(
                    Path.Join(UpdateDownloader.DownloadLocation, $"update-{UpdateChecker.LatestReleaseInfo.TagName}.zip"));
                string exePath = Path.Join(UpdateDownloader.DownloadLocation,
                    $"update-{UpdateChecker.LatestReleaseInfo.TagName}.exe");

                bool updateExeExists = File.Exists(exePath);

                if (updateExeExists && !UpdateChecker.VersionDifferent(UpdateChecker.LatestReleaseInfo.TagName, UpdateChecker.CurrentVersionTag))
                {
                    File.Delete(exePath);
                    updateExeExists = false;
                }

                string updaterPath = Path.Join(dir, "PixiEditor.UpdateInstaller.exe");

                if (updateFileExists || updateExeExists)
                {
                    ViewModelMain.Current.UpdateSubViewModel.UpdateReadyToInstall = true;
                    var result = ConfirmationDialog.Show("Update is ready to be installed. Do you want to install it now?", "New update");
                    if (result == Models.Enums.ConfirmationType.Yes)
                    {
                        if (updateFileExists && File.Exists(updaterPath))
                        {
                            InstallHeadless(updaterPath);
                        }
                        else if (updateExeExists)
                        {
                            OpenExeInstaller(exePath);
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
            NoticeDialog.Show(
                "Couldn't update without administrator rights.",
                "Insufficient permissions");
        }
    }

    private static void OpenExeInstaller(string updateExeFile)
    {
        bool alreadyUpdated = VersionHelpers.GetCurrentAssemblyVersion().ToString() ==
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

    [Command.Internal("PixiEditor.Restart")]
    public static void RestartApplication()
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
                NoticeDialog.Show("Could not check if there is an update available", "Update check failed");
            }

            AskToInstall();
        }
    }

    private void InitUpdateChecker()
    {
#if UPDATE
        UpdateChannels.Add(new UpdateChannel("Release", "PixiEditor", "PixiEditor"));
        UpdateChannels.Add(new UpdateChannel("Development", "PixiEditor", "PixiEditor-development-channel"));
#else
    #if STEAM
        string platformName = "Steam";
    #elif MSIX
        string platformName = "Microsoft Store";
    #else
        string platformName = "Unknown";
    #endif
        UpdateChannels.Add(new UpdateChannel(platformName, "", ""));
#endif

        string updateChannel = IPreferences.Current.GetPreference<string>("UpdateChannel");

        string version = VersionHelpers.GetCurrentAssemblyVersionString();
        UpdateChecker = new UpdateChecker(version, GetUpdateChannel(updateChannel));
        VersionText = $"Version {version}";
    }

    private UpdateChannel GetUpdateChannel(string channelName)
    {
        UpdateChannel selectedChannel = UpdateChannels.FirstOrDefault(x => x.Name == channelName, UpdateChannels[0]);
        return selectedChannel;
    }
}
