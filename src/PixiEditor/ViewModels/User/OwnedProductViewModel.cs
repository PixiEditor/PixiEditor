using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.IdentityProvider;

namespace PixiEditor.ViewModels.User;

public class OwnedProductViewModel : ObservableObject
{
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

    public IAsyncRelayCommand InstallCommand { get; }
    public IAsyncRelayCommand UninstallCommand { get; }
    public IRelayCommand<bool> ToggleEnabledCommand { get; }

    public OwnedProductViewModel(ProductData productData, bool isInstalled, string? installedVersion, bool isEnabled, bool isLoaded,
        IAsyncRelayCommand<string> installContentCommand, IAsyncRelayCommand<string> uninstallContentCommand, IRelayCommand<string> enableContentCommand, IRelayCommand<string> disableContentCommand, Func<string, bool> isInstalledFunc)
    {
        ProductData = productData;
        IsInstalled = isInstalled;
        IsLoaded = isLoaded;
        if (productData.LatestVersion != null && installedVersion != null)
        {
            UpdateAvailable = productData.LatestVersion != installedVersion;
        }
        else
        {
            UpdateAvailable = false;
        }
        
        IsEnabled = isEnabled;

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

                await uninstallContentCommand.ExecuteAsync(ProductData.Id);

                IsUninstalling = false;
                IsInstalled = false;
                UpdateAvailable = false;
                RestartRequired = true;
            },
            () => IsInstalled && !IsInstalling && !IsUninstalling
        );
        
        ToggleEnabledCommand = new RelayCommand<bool>(
            (isOn) =>
            {
                if (isOn)
                {
                    IsEnabled = true;
                    enableContentCommand.Execute(ProductData.Id);
                    IsLoaded = true;
                }
                else
                {
                    IsEnabled = false;
                    disableContentCommand.Execute(ProductData.Id);
                }
            },
            (isOn) => IsInstalled && !IsInstalling && !IsUninstalling
        );
    }
}
