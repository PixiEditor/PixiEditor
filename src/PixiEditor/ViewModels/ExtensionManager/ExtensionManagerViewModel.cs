using System.Collections.ObjectModel;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.IdentityProvider;
using PixiEditor.PixiAuth.Models;
using PixiEditor.Platform;
using PixiEditor.ViewModels.SubViewModels;
using PixiEditor.ViewModels.User;

namespace PixiEditor.ViewModels.ExtensionManager;

internal class ExtensionManagerViewModel : ViewModelBase
{
    public ObservableCollection<AvailableContentViewModel> AvailableExtensions { get; } =
        new ObservableCollection<AvailableContentViewModel>();
    
    public ObservableCollection<OwnedProductViewModel> OwnedExtensions { get; } =
        new ObservableCollection<OwnedProductViewModel>();
    
    public AsyncRelayCommand<string> InstallAndLoadExtensionCommand { get; }
    public AsyncRelayCommand<string> UninstallExtensionCommand { get; }
    
    private ExtensionsViewModel extensionsViewModel;
    private IAdditionalContentProvider contentProvider;
    private IIdentityProvider identityProvider;

    public ExtensionManagerViewModel(ExtensionsViewModel extensionsViewModel, IAdditionalContentProvider contentProvider, IIdentityProvider identityProvider)
    {
        this.extensionsViewModel = extensionsViewModel;
        this.contentProvider = contentProvider;
        this.identityProvider = identityProvider;
        
        InstallAndLoadExtensionCommand = new AsyncRelayCommand<string>(InstallAndLoadExtension, CanInstallAndLoadExtension);
        UninstallExtensionCommand = new AsyncRelayCommand<string>(UninstallExtension, CanUninstallExtension);
    }

    public async Task FetchAvailableExtensions()
    {
        AvailableExtensions.Clear();
        var availableExtensions = await contentProvider.FetchAvailableExtensions();
        foreach (var extension in availableExtensions)
        {
            AvailableExtensions.Add(new AvailableContentViewModel(extension, this));
        }
    }
    
    public void FetchOwnedExtensions()
    {
        OwnedExtensions.Clear();
        var extensions = identityProvider.User.OwnedProducts;
        foreach (ProductData extension in extensions)
        {
            bool isInstalled = IsInstalled(extension.Id);

            string? installedVersion = null;
            if (isInstalled)
            {
                installedVersion = extensionsViewModel.ExtensionLoader.LoadedExtensions
                    .FirstOrDefault(x => x.Metadata.UniqueName == extension.Id)?.Metadata.Version;
            }
            else
            {
                bool extensionDownloadedAtLeastOnce = IPreferences.Current.GetLocalPreference<bool>(
                    $"product_{extension.Id}_downloaded_at_least_once", false);
                if (!extensionDownloadedAtLeastOnce)
                {
                    Dispatcher.UIThread.InvokeAsync(async () => await InstallAndLoadExtension(extension.Id));
                    IPreferences.Current.UpdateLocalPreference($"product_{extension.Id}_downloaded_at_least_once", true);
                }
            }

            OwnedExtensions.Add(new OwnedProductViewModel(extension, isInstalled, installedVersion, InstallAndLoadExtensionCommand, UninstallExtensionCommand,
                IsInstalled));
        }
    }
    
    public bool CanInstallAndLoadExtension(string extensionId)
    {
        return !IsInstalled(extensionId) || UpdateAvailable(extensionId);
    }
    
    private bool UpdateAvailable(string extensionId)
    {
        ProductData product = identityProvider.User.OwnedProducts
            .FirstOrDefault(x => x.Id == extensionId);

        if (product == null)
        {
            return false;
        }

        return extensionsViewModel.ExtensionLoader.LoadedExtensions
            .FirstOrDefault(x => x.Metadata.UniqueName == extensionId)?.Metadata.Version != product.LatestVersion;
    }
    
    private bool IsInstalled(string extensionId)
    {
        if (contentProvider.IsInstalled(extensionId))
        {
            return true;
        }

        return extensionsViewModel.ExtensionLoader.LoadedExtensions.Any(x =>
            x.Metadata.UniqueName == extensionId);
    }
    
    public async Task InstallAndLoadExtension(string extensionId)
    {
        if (string.IsNullOrEmpty(extensionId))
        {
            return;
        }

        await extensionsViewModel.InstallAndLoadExtension(contentProvider,  extensionId);
    }
    
    public bool CanUninstallExtension(string extensionId)
    {
        return IsInstalled(extensionId);
    }
    
    public async Task UninstallExtension(string extensionId)
    {
        if (string.IsNullOrEmpty(extensionId))
        {
            return;
        }

        await extensionsViewModel.UninstallExtension(contentProvider,  extensionId);
    }
}
