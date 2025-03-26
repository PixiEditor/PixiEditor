using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using PixiEditor.Views.Dialogs;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.IO;
using PixiEditor.OperatingSystem;
using PixiEditor.Platform;
using PixiEditor.UpdateModule;

namespace PixiEditor.ViewModels.SubViewModels;

internal class UpdateViewModel : SubViewModel<ViewModelMain>
{
    public const int MaxRetryCount = 3;
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
                VersionText =
                    new LocalizedString("TO_INSTALL_UPDATE",
                        UpdateChecker.LatestReleaseInfo.TagName); // Button shows "Restart" before this text
            }
        }
    }

    public UpdateViewModel(ViewModelMain owner)
        : base(owner)
    {
        Owner.OnStartupEvent += Owner_OnStartupEvent;
        PixiEditorSettings.Update.UpdateChannel.ValueChanged += (_, value) =>
        {
            string prevChannel = UpdateChecker.Channel.ApiUrl;
            UpdateChecker.Channel = GetUpdateChannel(value);
            if (prevChannel != UpdateChecker.Channel.ApiUrl)
            {
                ConditionalUPDATE();
            }
        };
        InitUpdateChecker();
    }

    public async Task<bool> CheckForUpdate()
    {
        if (!IOperatingSystem.Current.IsWindows)
        {
            return false;
        }

        bool updateAvailable = await UpdateChecker.CheckUpdateAvailable();
        if (!UpdateChecker.LatestReleaseInfo.WasDataFetchSuccessful ||
            string.IsNullOrEmpty(UpdateChecker.LatestReleaseInfo.TagName))
        {
            return false;
        }

        bool updateCompatible = await UpdateChecker.IsUpdateCompatible();
        bool autoUpdateFailed = CheckAutoupdateFailed();
        bool updateFileDoesNotExists = !File.Exists(
            Path.Join(UpdateDownloader.DownloadLocation, $"update-{UpdateChecker.LatestReleaseInfo.TagName}.zip"));

        bool updateExeDoesNotExists = !File.Exists(
            Path.Join(UpdateDownloader.DownloadLocation, $"update-{UpdateChecker.LatestReleaseInfo.TagName}.exe"));
        if (updateAvailable && (updateFileDoesNotExists && updateExeDoesNotExists) || autoUpdateFailed)
        {
            UpdateReadyToInstall = false;
            VersionText = new LocalizedString("DOWNLOADING_UPDATE");
            try
            {
                if (updateCompatible && !autoUpdateFailed)
                {
                    await UpdateDownloader.DownloadReleaseZip(UpdateChecker.LatestReleaseInfo);
                }
                else
                {
                    await UpdateDownloader.DownloadInstaller(UpdateChecker.LatestReleaseInfo);
                    if (autoUpdateFailed)
                    {
                        RemoveZipIfExists();
                    }
                }

                UpdateReadyToInstall = true;
            }
            catch (IOException ex)
            {
                NoticeDialog.Show("FAILED_DOWNLOADING", "FAILED_DOWNLOADING_TITLE");
                return false;
            }
            catch (TaskCanceledException ex)
            {
                return false;
            }

            return true;
        }

        return false;
    }

    private async void Install()
    {
#if RELEASE || DEVRELEASE
        if (!PixiEditorSettings.Update.CheckUpdatesOnStartup.Value)
        {
            return;
        }

        string dir = AppDomain.CurrentDomain.BaseDirectory;

        UpdateDownloader.CreateTempDirectory();
        if (UpdateChecker.LatestReleaseInfo == null ||
            string.IsNullOrEmpty(UpdateChecker.LatestReleaseInfo.TagName)) return;
        bool updateFileExists = File.Exists(
            Path.Join(UpdateDownloader.DownloadLocation, $"update-{UpdateChecker.LatestReleaseInfo.TagName}.zip"));
        string exePath = Path.Join(UpdateDownloader.DownloadLocation,
            $"update-{UpdateChecker.LatestReleaseInfo.TagName}.exe");

        bool updateExeExists = File.Exists(exePath);

        if (updateExeExists &&
            !UpdateChecker.VersionDifferent(UpdateChecker.LatestReleaseInfo.TagName, UpdateChecker.CurrentVersionTag))
        {
            File.Delete(exePath);
            updateExeExists = false;
        }

        string updaterPath = Path.Join(dir, "PixiEditor.UpdateInstaller.exe");

        if (!updateFileExists && !updateExeExists)
        {
            EnsureUpdateFilesDeleted();
            return;
        }

        ViewModelMain.Current.UpdateSubViewModel.UpdateReadyToInstall = true;
        if (!UpdateInfoExists())
        {
            CreateUpdateInfo(UpdateChecker.LatestReleaseInfo.TagName);
            return;
        }

        string[] info = IncrementUpdateInfo();

        if (!UpdateInfoValid(UpdateChecker.LatestReleaseInfo.TagName, info))
        {
            File.Delete(Path.Join(Paths.TempFilesPath, "updateInfo.txt"));
            CreateUpdateInfo(UpdateChecker.LatestReleaseInfo.TagName);
            return;
        }

        if (!CanInstallUpdate(UpdateChecker.LatestReleaseInfo.TagName, info) && !updateExeExists)
        {
            return;
        }

        if (updateFileExists && File.Exists(updaterPath))
        {
            InstallHeadless(updaterPath);
        }
        else if (updateExeExists)
        {
            OpenExeInstaller(exePath);
        }
#endif
    }

    private static void InstallHeadless(string updaterPath)
    {
        try
        {
            ProcessHelper.RunAsAdmin(updaterPath, false);
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
        try
        {
            IOperatingSystem.Current.ProcessUtility.RunAsAdmin(updateExeFile);
            Shutdown();
        }
        catch (Win32Exception)
        {
            NoticeDialog.Show("COULD_NOT_UPDATE_WITHOUT_ADMIN", "INSUFFICIENT_PERMISSIONS");
        }
    }

    private static void Shutdown()
    {
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }

    [Command.Internal("PixiEditor.RestartToUpdate")]
    public static void RestartApplicationToUpdate()
    {
        try
        {
            ProcessHelper.RunAsAdmin(Path.Join(AppDomain.CurrentDomain.BaseDirectory,
                "PixiEditor.UpdateInstaller.exe"));
            Shutdown();
        }
        catch (Win32Exception)
        {
            NoticeDialog.Show("COULD_NOT_UPDATE_WITHOUT_ADMIN", "INSUFFICIENT_PERMISSIONS");
        }
    }

    private void Owner_OnStartupEvent()
    {
        ConditionalUPDATE();
    }

    [Conditional("UPDATE")]
    private async void ConditionalUPDATE()
    {
        if (PixiEditorSettings.Update.CheckUpdatesOnStartup.Value && OsSupported())
        {
            try
            {
                await CheckForUpdate();
                if (UpdateChecker.LatestReleaseInfo != null && UpdateChecker.LatestReleaseInfo.TagName ==
                    VersionHelpers.GetCurrentAssemblyVersionString())
                {
                    EnsureUpdateFilesDeleted();
                }
            }
            catch (System.Net.Http.HttpRequestException)
            {
                NoticeDialog.Show("COULD_NOT_CHECK_FOR_UPDATES", "UPDATE_CHECK_FAILED");
            }
            catch (Exception e)
            {
                CrashHelper.SendExceptionInfoAsync(e);
                NoticeDialog.Show("COULD_NOT_CHECK_FOR_UPDATES", "UPDATE_CHECK_FAILED");
            }

            Install();
        }
    }

    private bool OsSupported()
    {
        return IOperatingSystem.Current.IsWindows;
    }

    private bool UpdateInfoExists()
    {
        return File.Exists(Path.Join(Paths.TempFilesPath, "updateInfo.txt"));
    }

    private void CreateUpdateInfo(string targetVersion)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(targetVersion);
        sb.AppendLine("0");
        File.WriteAllText(Path.Join(Paths.TempFilesPath, "updateInfo.txt"), sb.ToString());
    }

    private string[] IncrementUpdateInfo()
    {
        string[] lines = File.ReadAllLines(Path.Join(Paths.TempFilesPath, "updateInfo.txt"));
        int.TryParse(lines[1], out int count);
        count++;
        lines[1] = count.ToString();
        File.WriteAllLines(Path.Join(Paths.TempFilesPath, "updateInfo.txt"), lines);

        return lines;
    }

    private void EnsureUpdateFilesDeleted()
    {
        string path = Path.Combine(Paths.TempFilesPath, "updateInfo.txt");
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private bool CanInstallUpdate(string forVersion, string[] lines)
    {
        if (lines.Length != 2) return false;

        if (lines[0] != forVersion) return false;

        return int.TryParse(lines[1], out int count) && count < MaxRetryCount;
    }

    private bool UpdateInfoValid(string forVersion, string[] lines)
    {
        if (lines.Length != 2) return false;

        if (lines[0] != forVersion) return false;

        return int.TryParse(lines[1], out _);
    }

    private bool CheckAutoupdateFailed()
    {
        string path = Path.Combine(Paths.TempFilesPath, "updateInfo.txt");
        if (!File.Exists(path)) return false;

        string[] lines = File.ReadAllLines(path);
        if (lines.Length != 2) return false;

        if (!int.TryParse(lines[1], out int count)) return false;

        return count >= MaxRetryCount;
    }

    private void RemoveZipIfExists()
    {
        string zipPath = Path.Join(UpdateDownloader.DownloadLocation,
            $"update-{UpdateChecker.LatestReleaseInfo.TagName}.zip");
        if (File.Exists(zipPath))
        {
            File.Delete(zipPath);
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

        string updateChannel = PixiEditorSettings.Update.UpdateChannel.Value;

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
