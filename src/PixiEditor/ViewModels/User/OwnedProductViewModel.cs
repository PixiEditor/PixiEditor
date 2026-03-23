using System.Windows.Input;
using AsyncImageLoader;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Extensions.FlyUI.Converters;
using PixiEditor.Extensions.IO;
using PixiEditor.IdentityProvider;
using PixiEditor.Models.Dialogs;
using PixiEditor.UI.Common.Localization;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.ViewModels.User;

public class OwnedProductViewModel : ObservableObject
{
    private IResourceStorage? storage;

    public IResourceStorage? ResourceStorage
    {
        get => storage;
        set
        {
            storage = value;
            UpdateImageSource();
        }
    }
    public ProductData ProductData { get; }

    private bool isInstalled;

    public bool IsInstalled
    {
        get => isInstalled;
        set => SetProperty(ref isInstalled, value);
    }

    private bool isInstalling;

    public bool IsInstalling
    {
        get => isInstalling;
        set => SetProperty(ref isInstalling, value);
    }

    private bool updateAvailable;

    public bool UpdateAvailable
    {
        get => updateAvailable;
        set => SetProperty(ref updateAvailable, value);
    }

    private bool restartRequired;

    public bool RestartRequired
    {
        get => restartRequired;
        set => SetProperty(ref restartRequired, value);
    }

    private bool isUninstalling;

    public bool IsUninstalling
    {
        get => isUninstalling;
        set => SetProperty(ref isUninstalling, value);
    }

    private bool isEnabled;

    public bool IsEnabled
    {
        get => isEnabled;
        set => SetProperty(ref isEnabled, value);
    }

    private bool isLoaded;

    public bool IsLoaded
    {
        get => isLoaded;
        set => SetProperty(ref isLoaded, value);
    }

    private bool canBeEnabled;

    public bool CanBeEnabled
    {
        get => canBeEnabled;
        set => SetProperty(ref canBeEnabled, value);
    }

    public IAsyncRelayCommand InstallCommand { get; }
    public IAsyncRelayCommand UninstallCommand { get; }
    public IAsyncRelayCommand ToggleEnabledCommand { get; }

    public IImage? ImageSource { get; set; }

    public OwnedProductViewModel(ProductData productData, bool isInstalled, string? installedVersion, bool isEnabled,
        bool isLoaded,
        IAsyncRelayCommand<string> installContentCommand, IAsyncRelayCommand<string> uninstallContentCommand,
        IRelayCommand<string> enableContentCommand, IRelayCommand<string> disableContentCommand,
        Func<string, bool> isInstalledFunc, Func<string, bool> areDependenciesReachableFunc,
        Func<string, int> countLoadedDependenciesFunc, IResourceStorage? storage = null)
    {
        ProductData = productData;
        IsInstalled = isInstalled;
        IsLoaded = isLoaded;
        this.storage = storage;
        if (productData.LatestVersion != null && installedVersion != null)
        {
            UpdateAvailable = productData.LatestVersion != installedVersion;
        }
        else
        {
            UpdateAvailable = false;
        }

        IsEnabled = isEnabled;
        CanBeEnabled = areDependenciesReachableFunc(ProductData.Id);

        InstallCommand = new AsyncRelayCommand(
            async () =>
            {
                IsInstalling = true;
                bool wasUpdating = UpdateAvailable;
                UpdateAvailable = false;
                RestartRequired = false;
                await installContentCommand.ExecuteAsync(ProductData.Id);
                IsInstalling = false;

                IsLoaded = true;
                IsEnabled = true;
                if (wasUpdating)
                {
                    RestartRequired = true;
                }
                else
                {
                    IsInstalled = isInstalledFunc(ProductData.Id);

                    UninstallCommand.NotifyCanExecuteChanged();
                    ToggleEnabledCommand.NotifyCanExecuteChanged();
                }
            }, () => !IsInstalled && !IsInstalling || UpdateAvailable);

        UninstallCommand = new AsyncRelayCommand(
            async () =>
            {
                IsUninstalling = true;
                RestartRequired = false;
                bool wasEnabled = IsEnabled;

                int dependentCount = countLoadedDependenciesFunc(ProductData.Id);
                if (dependentCount > 0)
                {
                    var result = await ConfirmationDialog.Show(
                        new LocalizedString("EXTENSIONS_WINDOW_DISABLE_CONFIRMATION_MESSAGE", dependentCount),
                        "EXTENSIONS_WINDOW_UNINSTALL_CONFIRMATION_TITLE");

                    if (result != ConfirmationType.Yes)
                    {
                        IsUninstalling = false;
                        return;
                    }
                }
                else
                {
                    var result = await ConfirmationDialog.Show(
                        new LocalizedString("EXTENSIONS_WINDOW_UNINSTALL_CONFIRMATION_MESSAGE"),
                        "EXTENSIONS_WINDOW_UNINSTALL_CONFIRMATION_TITLE");

                    if (result != ConfirmationType.Yes)
                    {
                        IsUninstalling = false;
                        return;
                    }
                }

                await uninstallContentCommand.ExecuteAsync(ProductData.Id);

                IsUninstalling = false;
                IsInstalled = false;
                UpdateAvailable = false;
                RestartRequired = wasEnabled;
                InstallCommand.NotifyCanExecuteChanged();
                ToggleEnabledCommand.NotifyCanExecuteChanged();
            },
            () => IsInstalled && !IsInstalling && !IsUninstalling
        );

        ToggleEnabledCommand = new AsyncRelayCommand<bool>(
            async (isOn) =>
            {
                if (isOn)
                {
                    CanBeEnabled = areDependenciesReachableFunc(ProductData.Id);
                    if (CanBeEnabled)
                    {
                        IsEnabled = true;
                        enableContentCommand.Execute(ProductData.Id);
                        IsLoaded = true;
                    }
                }
                else
                {
                    int dependentCount = countLoadedDependenciesFunc(ProductData.Id);
                    if (dependentCount > 0)
                    {
                        var result = await ConfirmationDialog.Show(
                            new LocalizedString("EXTENSIONS_WINDOW_DISABLE_CONFIRMATION_MESSAGE", dependentCount),
                            "EXTENSIONS_WINDOW_DISABLE_CONFIRMATION_TITLE");

                        if (result != ConfirmationType.Yes)
                        {
                            IsEnabled = true;
                            return;
                        }
                    }

                    IsEnabled = false;
                    disableContentCommand.Execute(ProductData.Id);
                }
            },
            (isOn) => IsInstalled && !IsInstalling && !IsUninstalling
        );

        UpdateImageSource();
    }

    private void UpdateImageSource()
    {
        ImageSource = ResourceStorage != null && ProductData.ImageUrl != null
            ? PathToImgSourceConverter.GetImageFromPath(ProductData.ImageUrl, ResourceStorage) : null;

        OnPropertyChanged(nameof(ImageSource));
        if (ResourceStorage == null)
        {
            Task.Run(async () =>
            {
                return await ImageLoader.AsyncImageLoader.ProvideImageAsync(ProductData.ImageUrl);
            }).ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        ImageSource = t.Result;
                        OnPropertyChanged(nameof(ImageSource));
                    });
                }
            });
        }
    }
}
