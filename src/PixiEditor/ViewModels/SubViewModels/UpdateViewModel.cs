using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Views.Dialogs;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.IO;
using PixiEditor.OperatingSystem;
using PixiEditor.Platform;
using PixiEditor.UI.Common.Localization;
using PixiEditor.UpdateModule;

namespace PixiEditor.ViewModels.SubViewModels;

internal class UpdateViewModel : SubViewModel<ViewModelMain>
{
    private double currentProgress;
    public UpdateChecker UpdateChecker { get; set; }

    public List<UpdateChannel> UpdateChannels { get; } = new List<UpdateChannel>();

    private UpdateState _updateState = UpdateState.Checking;

    public UpdateState UpdateState
    {
        get => _updateState;
        set
        {
            _updateState = value;
            OnPropertyChanged(nameof(UpdateState));
            OnPropertyChanged(nameof(IsUpdateAvailable));
            OnPropertyChanged(nameof(UpdateReadyToInstall));
            OnPropertyChanged(nameof(IsDownloading));
            OnPropertyChanged(nameof(IsUpToDate));
            OnPropertyChanged(nameof(UpdateStateString));
        }
    }

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


    public string UpdateStateString
    {
        get
        {
            if (!SelfUpdatingAvailable)
                return string.Empty;
            switch (_updateState)
            {
                case UpdateState.UnableToCheck:
                    return new LocalizedString("UP_TO_DATE_UNKNOWN");
                case UpdateState.Checking:
                    return new LocalizedString("CHECKING_FOR_UPDATES");
                case UpdateState.FailedDownload:
                    return new LocalizedString("UPDATE_FAILED_DOWNLOAD");
                case UpdateState.ReadyToInstall:
                    return new LocalizedString("UPDATE_READY_TO_INSTALL", UpdateChecker.LatestReleaseInfo.TagName);
                case UpdateState.Downloading:
                    return new LocalizedString("DOWNLOADING_UPDATE");
                case UpdateState.UpdateAvailable:
                    return new LocalizedString("UPDATE_AVAILABLE", UpdateChecker.LatestReleaseInfo.TagName);
                case UpdateState.UpToDate:
                    return new LocalizedString("UP_TO_DATE");
                default:
                    return new LocalizedString("UP_TO_DATE_UNKNOWN");
            }
        }
    }

    public bool IsUpdateAvailable
    {
        get => _updateState == UpdateState.UpdateAvailable;
    }

    public bool UpdateReadyToInstall
    {
        get => _updateState == UpdateState.ReadyToInstall;
    }

    public bool IsDownloading
    {
        get => _updateState == UpdateState.Downloading;
    }

    public bool IsUpToDate
    {
        get => _updateState == UpdateState.UpToDate;
    }
    
    public double CurrentProgress 
    {
        get => currentProgress;
        set
        {
            currentProgress = value;
            OnPropertyChanged(nameof(CurrentProgress));
        }
    }

    public string ZipExtension => IOperatingSystem.Current.IsLinux ? "tar.gz" : "zip";
    public string ZipContentType => IOperatingSystem.Current.IsLinux ? "octet-stream" : "zip";
    public string InstallerExtension => IOperatingSystem.Current.IsWindows ? "exe" : "dmg";

    public string BinaryExtension => IOperatingSystem.Current.IsWindows ? ".exe" : string.Empty;

    public bool SelfUpdatingAvailable =>
#if UPDATE
        PixiEditorSettings.Update.CheckUpdatesOnStartup.Value && OsSupported() && !InstallDirReadOnly();
#else
        false;
#endif

    public AsyncRelayCommand DownloadCommand => new AsyncRelayCommand(Download);
    public RelayCommand InstallCommand => new RelayCommand(Install);

    public UpdateViewModel(ViewModelMain owner)
        : base(owner)
    {
        if (IOperatingSystem.Current.IsLinux)
        {
            if (File.Exists("no-updates"))
            {
                UpdateState = UpdateState.UnableToCheck;
                return;
            }
        }

        Owner.OnStartupEvent += Owner_OnStartupEvent;
        Owner.OnClose += Owner_OnClose;
        UpdateDownloader.ProgressChanged += d =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                CurrentProgress = d;
            });
        };
        PixiEditorSettings.Update.UpdateChannel.ValueChanged += (_, value) =>
        {
            string prevChannel = UpdateChecker.Channel.ApiUrl;
            UpdateChecker.Channel = GetUpdateChannel(value);
            if (prevChannel != UpdateChecker.Channel.ApiUrl)
            {
                ConditionalUPDATE();
            }

            OnPropertyChanged(nameof(UpdateStateString));
            OnPropertyChanged(nameof(SelfUpdatingAvailable));
        };
        InitUpdateChecker();
    }

    public async Task CheckForUpdate()
    {
        if (!OsSupported())
        {
            return;
        }

        bool updateAvailable = await UpdateChecker.CheckUpdateAvailable();
        if (!UpdateChecker.LatestReleaseInfo.WasDataFetchSuccessful ||
            string.IsNullOrEmpty(UpdateChecker.LatestReleaseInfo.TagName))
        {
            UpdateState = UpdateState.UnableToCheck;
            return;
        }

        if (updateAvailable)
        {
            if (!IOperatingSystem.Current.IsWindows && UpdateChecker.LatestReleaseInfo.TagName.StartsWith("1."))
            {
                // 1.0 is windows only
                UpdateState = UpdateState.UpToDate;
                return;
            }
        }

        UpdateState = updateAvailable ? UpdateState.UpdateAvailable : UpdateState.UpToDate;
    }

    private void Owner_OnClose()
    {
#if RELEASE || DEVRELEASE
        if (UpdateState == UpdateState.ReadyToInstall)
        {
            Install(false);
        }
#endif
    }

    public async Task Download()
    {
        bool updateCompatible = await UpdateChecker.IsUpdateCompatible();
        bool updateFileDoesNotExists = !AutoUpdateFileExists();
        bool updateExeDoesNotExists = !UpdateInstallerFileExists();

        if (!updateExeDoesNotExists || !updateFileDoesNotExists)
        {
            UpdateState = UpdateState.ReadyToInstall;
            return;
        }

        if ((updateFileDoesNotExists && updateExeDoesNotExists))
        {
            try
            {
                UpdateState = UpdateState.Downloading;
                CurrentProgress = 0;
                if (updateCompatible || !IOperatingSystem.Current.IsWindows)
                {
                    await UpdateDownloader.DownloadReleaseZip(UpdateChecker.LatestReleaseInfo, ZipContentType,
                        ZipExtension);
                }
                else if (IOperatingSystem.Current.IsWindows)
                {
                    await UpdateDownloader.DownloadInstaller(UpdateChecker.LatestReleaseInfo);
                }

                UpdateState = UpdateState.ReadyToInstall;
            }
            catch (IOException ex)
            {
                UpdateState = UpdateState.FailedDownload;
            }
            catch (TaskCanceledException ex)
            {
                UpdateState = UpdateState.UpdateAvailable;
            }
            catch (Exception ex)
            {
                UpdateState = UpdateState.FailedDownload;
            }
        }
    }

    private bool AutoUpdateFileExists()
    {
        string path = Path.Join(UpdateDownloader.DownloadLocation,
            $"update-{UpdateChecker.LatestReleaseInfo.TagName}.{ZipExtension}");
        return File.Exists(path);
    }

    private bool UpdateInstallerFileExists()
    {
        if (IOperatingSystem.Current.IsLinux) return false;

        string path = Path.Join(UpdateDownloader.DownloadLocation,
            $"update-{UpdateChecker.LatestReleaseInfo.TagName}.{InstallerExtension}");
        return File.Exists(path);
    }


    private void Install()
    {
        Install(true);
    }

    [Command.Debug("PixiEditor.Update.DebugInstall", "Debug Install Update",
        "(DEBUG) Install update zip file without checking for updates")]
    public void DebugInstall()
    {
        UpdateChecker.SetLatestReleaseInfo(new ReleaseInfo(true) { TagName = "2.2.2.2" });
        Install(true);
    }

    private void Install(bool startAfterUpdate)
    {
        string dir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName) ??
                     AppDomain.CurrentDomain.BaseDirectory;

        UpdateDownloader.CreateTempDirectory();
        if (UpdateChecker.LatestReleaseInfo == null ||
            string.IsNullOrEmpty(UpdateChecker.LatestReleaseInfo.TagName)) return;
        bool updateFileExists = AutoUpdateFileExists();
        string exePath = Path.Join(UpdateDownloader.DownloadLocation,
            $"update-{UpdateChecker.LatestReleaseInfo.TagName}.{InstallerExtension}");

        bool updateExeExists = File.Exists(exePath);

        if (updateExeExists &&
            !UpdateChecker.VersionDifferent(UpdateChecker.LatestReleaseInfo.TagName, UpdateChecker.CurrentVersionTag))
        {
            File.Delete(exePath);
            updateExeExists = false;
        }

        string updaterPath = Path.Join(dir, $"PixiEditor.UpdateInstaller{BinaryExtension}");


        if (!updateFileExists && !updateExeExists)
        {
            EnsureUpdateFilesDeleted();
            UpdateState = UpdateState.Checking;
            Dispatcher.UIThread.InvokeAsync(async () => await CheckForUpdate());
            return;
        }

        try
        {
            if (Path.Exists(updaterPath))
            {
                File.Copy(updaterPath,
                    Path.Join(UpdateDownloader.DownloadLocation, $"PixiEditor.UpdateInstaller" + BinaryExtension),
                    true);
                updaterPath = Path.Join(UpdateDownloader.DownloadLocation,
                    $"PixiEditor.UpdateInstaller" + BinaryExtension);
                File.WriteAllText(Path.Join(UpdateDownloader.DownloadLocation, "update-location.txt"),
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty);
            }
        }
        catch (IOException)
        {
            NoticeDialog.Show("COULD_NOT_UPDATE_WITHOUT_ADMIN", "INSUFFICIENT_PERMISSIONS");
            return;
        }

        if (updateFileExists && File.Exists(updaterPath))
        {
            InstallHeadless(updaterPath, startAfterUpdate);
        }
        else if (updateExeExists)
        {
            OpenExeInstaller(exePath);
        }
    }

    private static void InstallHeadless(string updaterPath, bool startAfterUpdate)
    {
        TryRestartToUpdate(updaterPath, startAfterUpdate ? "--startOnSuccess" : null);
    }

    private void OpenExeInstaller(string updateExeFile)
    {
        bool alreadyUpdated = VersionHelpers.GetCurrentAssemblyVersion().ToString() ==
                              updateExeFile.Split('-')[1].Split("." + InstallerExtension)[0];

        if (!alreadyUpdated)
        {
            RestartToUpdate(updateExeFile);
        }
        else
        {
            File.Delete(updateExeFile);
        }
    }

    private static bool InstallDirReadOnly()
    {
        string installDir = AppDomain.CurrentDomain.BaseDirectory;
        DirectoryInfo dirInfo = new DirectoryInfo(installDir);
        return dirInfo.Attributes.HasFlag(FileAttributes.ReadOnly);
    }

    private static void RestartToUpdate(string updateExeFile)
    {
        TryRestartToUpdate(updateExeFile, null);
    }

    private static void TryRestartToUpdate(string updateExeFile, string? args)
    {
        try
        {
            if (IOperatingSystem.Current.IsLinux)
            {
                bool hasWritePermissions = !InstallDirReadOnly();
                if (hasWritePermissions)
                {
                    IOperatingSystem.Current.ProcessUtility.ShellExecute(updateExeFile, args);
                    Shutdown();
                }
                else
                {
                    NoticeDialog.Show("COULD_NOT_UPDATE_WITHOUT_ADMIN", "INSUFFICIENT_PERMISSIONS");
                    return;
                }
            }
            else
            {
                // couldn't get Process.Start to work correctly with osascript environment
                args = IOperatingSystem.Current.IsMacOs ? args + " --postExecute 'open -a PixiEditor'" : args;
                var proc = IOperatingSystem.Current.ProcessUtility.RunAsAdmin(updateExeFile, args);
                if (IOperatingSystem.Current.IsMacOs)
                {
                    proc.WaitForExitAsync().ContinueWith(t =>
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            if (t.IsCompletedSuccessfully && proc.ExitCode == 0)
                            {
                                Shutdown();
                            }
                            else
                            {
                                NoticeDialog.Show("COULD_NOT_UPDATE_WITHOUT_ADMIN", "INSUFFICIENT_PERMISSIONS");
                            }
                        });
                    });
                }
                else
                {
                    Shutdown();
                }
            }
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
            string binExtension = IOperatingSystem.Current.IsWindows ? ".exe" : string.Empty;
            ProcessHelper.RunAsAdmin(Path.Join(AppDomain.CurrentDomain.BaseDirectory,
                $"PixiEditor.UpdateInstaller{binExtension}"));
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
        if (PixiEditorSettings.Update.CheckUpdatesOnStartup.Value && OsSupported() && !InstallDirReadOnly())
        {
            try
            {
                await CheckForUpdate();
                if (UpdateState == UpdateState.UpdateAvailable)
                {
                    bool updateFileExists = AutoUpdateFileExists() || UpdateInstallerFileExists();
                    if (updateFileExists)
                    {
                        UpdateState = UpdateState.ReadyToInstall;
                    }
                }

                if (UpdateChecker.LatestReleaseInfo != null && UpdateChecker.LatestReleaseInfo.TagName ==
                    VersionHelpers.GetCurrentAssemblyVersionString())
                {
                    EnsureUpdateFilesDeleted();
                }
            }
            catch (System.Net.Http.HttpRequestException)
            {
                UpdateState = UpdateState.UnableToCheck;
            }
            catch (Exception e)
            {
                UpdateState = UpdateState.UnableToCheck;
                CrashHelper.SendExceptionInfoAsync(e);
            }
        }
    }

    private bool OsSupported()
    {
        return IOperatingSystem.Current.IsWindows || IOperatingSystem.Current.IsLinux ||
               IOperatingSystem.Current.IsMacOs;
    }

    private void EnsureUpdateFilesDeleted()
    {
        string path = Path.Combine(Paths.TempFilesPath, "updateInfo.txt");
        if (File.Exists(path))
        {
            File.Delete(path);
        }
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
        string platformName = IPlatform.Current.Name;
#if UPDATE
        UpdateChannels.Add(new UpdateChannel("Release", "PixiEditor", "PixiEditor"));
        UpdateChannels.Add(new UpdateChannel("Development", "PixiEditor", "PixiEditor-development-channel"));
#else
        UpdateChannels.Add(new UpdateChannel(platformName, "", ""));
#endif

        if (IPreferences.Current.GetLocalPreference<bool>("UseTestingUpdateChannel", false))
        {
            UpdateChannels.Add(new UpdateChannel("Testing", "PixiEditor", "PixiEditor-testing-channel"));
        }

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

public enum UpdateState
{
    Checking,
    UnableToCheck,
    UpdateAvailable,
    Downloading,
    FailedDownload,
    ReadyToInstall,
    UpToDate,
}
