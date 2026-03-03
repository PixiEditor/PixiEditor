using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Extensions;
using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Extensions.Metadata;
using PixiEditor.IdentityProvider;
using PixiEditor.IdentityProvider.PixiAuth;
using PixiEditor.Models.Commands.XAML;
using PixiEditor.OperatingSystem;
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
    public RelayCommand<string> EnableExtensionCommand { get; }
    public RelayCommand<string> DisableExtensionCommand { get; }
    
    public RelayCommand BackToListCommand { get; }
    public RelayCommand<AvailableContentViewModel> SelectExtensionCommand { get; }
    
    public AsyncRelayCommand<string> OpenPurchaseLinkCommand { get; }
    
    public ObservableCollection<ExtensionsTab> Tabs { get; } =
        new ObservableCollection<ExtensionsTab>
        {
            new ExtensionsTab("All","EXTENSIONS_WINDOW_TAB_ALL"),
            new ExtensionsTab("Owned", "EXTENSIONS_WINDOW_TAB_OWNED")
        };
    
    private ExtensionsTab selectedTab;
    public ExtensionsTab SelectedTab
    {
        get => selectedTab;
        set
        {
            if (SetProperty(ref selectedTab, value))
            {
                OnPropertyChanged(nameof(ShowAllTab));
                OnPropertyChanged(nameof(ShowOwnedTab));
            }
        }
    }
    
    public bool ShowAllTab => SelectedTab != null && SelectedTab.Id == "All";
    public bool ShowOwnedTab => SelectedTab != null && SelectedTab.Id == "Owned";
    
    private AvailableContentViewModel? selectedAvailableExtension;
    public AvailableContentViewModel? SelectedAvailableExtension
    {
        get => selectedAvailableExtension;
        set
        {
            if (SetProperty(ref selectedAvailableExtension, value))
            {
                OnPropertyChanged(nameof(IsDetailsVisible));
                OnPropertyChanged(nameof(IsListVisible));
            }
        }
    }
    
    public bool IsDetailsVisible => SelectedAvailableExtension != null;
    public bool IsListVisible => SelectedAvailableExtension == null;
    
    private ExtensionsViewModel extensionsViewModel;
    private IAdditionalContentProvider contentProvider;
    private IIdentityProvider identityProvider;

    public bool IsUserLoggedIn => identityProvider.User != null && identityProvider.User.IsLoggedIn;
    
    public bool ShouldUpdateUserOwnedProducts = false;
    
    public ExtensionManagerViewModel(ExtensionsViewModel extensionsViewModel, IAdditionalContentProvider contentProvider, IIdentityProvider identityProvider)
    {
        this.extensionsViewModel = extensionsViewModel;
        this.contentProvider = contentProvider;
        this.identityProvider = identityProvider;
        
        InstallAndLoadExtensionCommand = new AsyncRelayCommand<string>(InstallAndLoadExtension, CanInstallAndLoadExtension);
        UninstallExtensionCommand = new AsyncRelayCommand<string>(UninstallExtension, CanUninstallExtension);
        EnableExtensionCommand = new RelayCommand<string>(EnableExtension, CanEnableExtension);
        DisableExtensionCommand = new RelayCommand<string>(DisableExtension, CanDisableExtension);
        
        BackToListCommand = new RelayCommand(BackToList);
        SelectExtensionCommand = new RelayCommand<AvailableContentViewModel>(SelectExtension);
        
        OpenPurchaseLinkCommand = new AsyncRelayCommand<string>(OpenPurchaseLink);
        
        SelectedTab = Tabs.FirstOrDefault(tab => tab.Id == "All");
        
        OnPropertyChanged(nameof(IsUserLoggedIn));
        identityProvider.OnLoggedIn += _ => UpdateLoginState();
        identityProvider.LoggedOut += () => UpdateLoginState();
        identityProvider.OwnedProductsUpdated += _ => UpdateOwnedExtensions();
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

        if (identityProvider.User != null)
        {
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
                        IPreferences.Current.UpdateLocalPreference($"product_{extension.Id}_downloaded_at_least_once",
                            true);
                    }
                }

                bool isEnabled = IsEnabled(extension.Id);
                bool isLoaded = IsLoaded(extension.Id);

                OwnedExtensions.Add(new OwnedProductViewModel(extension, isInstalled, installedVersion, isEnabled,
                    isLoaded, InstallAndLoadExtensionCommand, UninstallExtensionCommand, EnableExtensionCommand,
                    DisableExtensionCommand, IsInstalled));
            }
        }

        // Add installed extensions that aren't in user owned products
        foreach (Extension loadedExtension in extensionsViewModel.ExtensionLoader.LoadedExtensions)
        {
            AddToOwnedExtensionsIfMissing(loadedExtension.Metadata);
        }
        
        foreach (ExtensionMetadata unloadedExtensionMetadata in extensionsViewModel.ExtensionLoader.UnloadedExtensionsMetadata)
        {
            AddToOwnedExtensionsIfMissing(unloadedExtensionMetadata);
        }
    }

    public void AddToOwnedExtensionsIfMissing(ExtensionMetadata extensionMetadata)
    {
        bool owned = OwnedExtensions.Any(owned => owned.ProductData.Id == extensionMetadata.UniqueName);

        if (!owned)
        {
            ProductData productData = new ProductData(extensionMetadata.UniqueName,extensionMetadata.DisplayName);
            productData.Author = extensionMetadata.Author.Name;
            productData.Description = extensionMetadata.Description;
                
            bool isInstalled = IsInstalled(extensionMetadata.UniqueName);
            bool isEnabled = IsEnabled(extensionMetadata.UniqueName);
            bool isLoaded = IsLoaded(extensionMetadata.UniqueName);
                
            OwnedExtensions.Add(new OwnedProductViewModel(productData, isInstalled, extensionMetadata.Version, isEnabled, isLoaded, InstallAndLoadExtensionCommand, UninstallExtensionCommand, EnableExtensionCommand, DisableExtensionCommand, IsInstalled));
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

        await extensionsViewModel.UninstallExtension(extensionId);
    }
    
    private bool IsEnabled(string extensionId)
    {
        var disabled = PixiEditorSettings.Extensions.DisabledExtensions.Value.ToList();
        return !disabled.Contains(extensionId);
    }
    
    private bool IsLoaded(string extensionId)
    {
        return extensionsViewModel.IsLoaded(extensionId);
    }
    
    public bool CanEnableExtension(string extensionId)
    {
        return IsInstalled(extensionId);
    }
    
    public void EnableExtension(string extensionId)
    {
        if (string.IsNullOrEmpty(extensionId))
        {
            return;
        }
        
        extensionsViewModel.EnableExtension(extensionId);
    }
    
    public bool CanDisableExtension(string extensionId)
    {
        return IsInstalled(extensionId);
    }
    
    public void DisableExtension(string extensionId)
    {
        if (string.IsNullOrEmpty(extensionId))
        {
            return;
        }

        extensionsViewModel.DisableExtension(extensionId);
    }
    
    private void BackToList()
    {
        SelectedAvailableExtension = null;
    }

    private void SelectExtension(AvailableContentViewModel ext)
    {
        SelectedAvailableExtension = ext;
    }

    private async Task OpenPurchaseLink(string extensionId)
    {
        if (string.IsNullOrEmpty(extensionId))
            return;

        if (identityProvider is PixiAuthIdentityProvider pixiAuthIdentityProvider)
        {
            if (identityProvider.User is PixiUser pixiUser)
            {
                Guid? sessionId = pixiUser.SessionId;
                string url = pixiAuthIdentityProvider.PixiAuthClient.GetCreateCheckoutSessionFromSessionIdUrl(sessionId, extensionId);
                IOperatingSystem.Current.OpenUri(url);
                ShouldUpdateUserOwnedProducts = true;
            }
        }
    }

    public async Task UpdateUserOwnedProducts()
    {
        if (identityProvider is PixiAuthIdentityProvider pixiAuthIdentityProvider)
        {
            await pixiAuthIdentityProvider.UpdateUserOwnedProducts();
            ShouldUpdateUserOwnedProducts = false;
        }
    }
    
    private void UpdateLoginState()
    {
        Dispatcher.UIThread.Post(() =>
        {
            OnPropertyChanged(nameof(IsUserLoggedIn));

            if (IsUserLoggedIn)
            {
                FetchOwnedExtensions();
            }
            else
            {
                OwnedExtensions.Clear();
                SelectedAvailableExtension = null;
            }
        });
    }

    private void UpdateOwnedExtensions()
    {
        FetchOwnedExtensions();
        
        foreach (var ext in AvailableExtensions)
        {
            ext.NotifyIsOwnedChanged();
        }
    }    
}
