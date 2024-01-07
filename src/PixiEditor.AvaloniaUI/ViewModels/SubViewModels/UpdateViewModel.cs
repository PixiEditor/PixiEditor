using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Models.Commands.Attributes.Commands;
using PixiEditor.AvaloniaUI.Models.Dialogs;
using PixiEditor.AvaloniaUI.Views.Dialogs;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.Common.UserPreferences;
using PixiEditor.Platform;
using PixiEditor.UpdateModule;

namespace PixiEditor.AvaloniaUI.ViewModels.SubViewModels;

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
            OnPropertyChanged(nameof(VersionText));
        }
    }

    public bool UpdateReadyToInstall
    {
        get => updateReadyToInstall;
        set
        {
            updateReadyToInstall = value;
            OnPropertyChanged(nameof(UpdateReadyToInstall));
            if (value)
            {
                VersionText = new LocalizedString("TO_INSTALL_UPDATE", UpdateChecker.LatestReleaseInfo.TagName); // Button shows "Restart" before this text
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
            VersionText = new LocalizedString("DOWNLOADING_UPDATE");
            try
            {
                if (updateCompatible)
                {
                    await UpdateDownloader.DownloadReleaseZip(UpdateChecker.LatestReleaseInfo);
                }
                else
                {
                    await UpdateDownloader.DownloadInstaller(UpdateChecker.LatestReleaseInfo);
                }

                UpdateReadyToInstall = true;
            }
            catch (IOException ex)
            {
                NoticeDialog.Show("FAILED_DOWNLOADING_TITLE", "FAILED_DOWNLOADING");
                return false;
            }
            catch(TaskCanceledException ex)
            {
                return false;
            }

            return true;
        }

        return false;
    }

    private void AskToInstall()
    {
#if RELEASE || DEVRELEASE
            if (IPreferences.Current.GetPreference("CheckUpdatesOnStartup", true))
            {
                string dir = AppDomain.CurrentDomain.BaseDirectory;
                
                UpdateDownloader.CreateTempDirectory();
                if(UpdateChecker.LatestReleaseInfo == null || string.IsNullOrEmpty(UpdateChecker.LatestReleaseInfo.TagName)) return;
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
                    var result = ConfirmationDialog.Show("UPDATE_READY", "NEW_UPDATE");
                    result.Wait();
                    if (result.Result == ConfirmationType.Yes)
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
            Shutdown();
        }
        catch (Win32Exception)
        {
            NoticeDialog.Show(
                "COULD_NOT_UPDATE_WITHOUT_ADMIN",
                "INSUFFICIENT_PERMISSIONS");
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
        Shutdown();
    }

    private static void Shutdown()
    {
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }

    [Command.Internal("PixiEditor.Restart")]
    public static void RestartApplication()
    {
        try
        {
            ProcessHelper.RunAsAdmin(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "PixiEditor.UpdateInstaller.exe"));
            Shutdown();
        }
        catch (Win32Exception)
        {
            NoticeDialog.Show("COULD_NOT_UPDATE_WITHOUT_ADMIN", "INSUFFICIENT_PERMISSIONS");
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
                NoticeDialog.Show("COULD_NOT_CHECK_FOR_UPDATES", "UPDATE_CHECK_FAILED");
            }
            catch (Exception e)
            {
                CrashHelper.SendExceptionInfoToWebhookAsync(e);
                NoticeDialog.Show("COULD_NOT_CHECK_FOR_UPDATES", "UPDATE_CHECK_FAILED");
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
        string platformName = IPlatform.Current.Name;
        UpdateChannels.Add(new UpdateChannel(platformName, "", ""));
#endif

        string updateChannel = IPreferences.Current.GetPreference<string>("UpdateChannel");

        string version = VersionHelpers.GetCurrentAssemblyVersionString();
        UpdateChecker = new UpdateChecker(version, GetUpdateChannel(updateChannel));
        VersionText = new LocalizedString("VERSION", version);
    }

    private UpdateChannel GetUpdateChannel(string channelName)
    {
        UpdateChannel selectedChannel = UpdateChannels.FirstOrDefault(x => x.Name == channelName, UpdateChannels[0]);
        return selectedChannel;
    }
}
