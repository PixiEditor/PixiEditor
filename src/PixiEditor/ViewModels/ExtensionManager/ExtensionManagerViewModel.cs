using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.Input;
using LiveMarkdown.Avalonia;
using PixiEditor.Extensions;
using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Extensions.IO;
using PixiEditor.Extensions.Metadata;
using PixiEditor.Extensions.WasmRuntime;
using PixiEditor.Extensions.WasmRuntime.Utilities;
using PixiEditor.IdentityProvider;
using PixiEditor.IdentityProvider.PixiAuth;
using PixiEditor.Models.Commands.XAML;
using PixiEditor.Models.ExternalServices;
using PixiEditor.OperatingSystem;
using PixiEditor.PixiAuth.Exceptions;
using PixiEditor.PixiAuth.Models;
using PixiEditor.Platform;
using PixiEditor.Platform.Standalone;
using PixiEditor.ViewModels.SubViewModels;
using PixiEditor.ViewModels.User;

namespace PixiEditor.ViewModels.ExtensionManager;

internal class ExtensionManagerViewModel : ViewModelBase
{
    public ObservableCollection<AvailableContentViewModel> AvailableExtensions { get; } =
        new ObservableCollection<AvailableContentViewModel>();

    // TODO: refactor to LibraryExtensions - it's made from owned and custom installed
    public ObservableCollection<OwnedProductViewModel> OwnedExtensions { get; } =
        new ObservableCollection<OwnedProductViewModel>();

    public AsyncRelayCommand<string> AddToLibraryCommand { get; }
    public AsyncRelayCommand<string> InstallAndLoadExtensionCommand { get; }
    public AsyncRelayCommand<string> UninstallExtensionCommand { get; }
    public AsyncRelayCommand<string> EnableExtensionCommand { get; }
    public RelayCommand<string> DisableExtensionCommand { get; }

    public RelayCommand BackToListCommand { get; }
    public RelayCommand<AvailableContentViewModel> SelectExtensionCommand { get; }
    public RelayCommand<OwnedProductViewModel> SelectOwnedExtensionCommand { get; }

    public AsyncRelayCommand<string> OpenPurchaseLinkCommand { get; }

    public ObservableCollection<ExtensionsTab> Tabs { get; } =
        new ObservableCollection<ExtensionsTab>
        {
            new ExtensionsTab("All", "EXTENSIONS_WINDOW_TAB_ALL"),
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

    public string ErrorMessage
    {
        get => errorMessage;
        set => SetProperty(ref errorMessage, value);
    }

    public bool IsDetailsVisible => SelectedAvailableExtension != null;
    public bool IsListVisible => SelectedAvailableExtension == null;

    private ExtensionsViewModel extensionsViewModel;
    private IAdditionalContentProvider contentProvider;
    private IIdentityProvider identityProvider;

    private string errorMessage;

    public bool IsUserLoggedIn => identityProvider.User != null && identityProvider.User.IsLoggedIn;
    public RelayCommand<LinkClickedEventArgs> LinkClickCommand { get; }

    public bool ShouldUpdateUserOwnedProducts = false;

    public ExtensionManagerViewModel(ExtensionsViewModel extensionsViewModel,
        IAdditionalContentProvider contentProvider, IIdentityProvider identityProvider)
    {
        this.extensionsViewModel = extensionsViewModel;
        this.contentProvider = contentProvider;
        this.identityProvider = identityProvider;

        InstallAndLoadExtensionCommand =
            new AsyncRelayCommand<string>(InstallAndLoadExtension, CanInstallAndLoadExtension);
        UninstallExtensionCommand = new AsyncRelayCommand<string>(UninstallExtension, CanUninstallExtension);
        EnableExtensionCommand = new AsyncRelayCommand<string>(EnableExtension, CanEnableExtension);
        DisableExtensionCommand = new RelayCommand<string>(DisableExtension, CanDisableExtension);
        AddToLibraryCommand = new AsyncRelayCommand<string>(AddToLibrary, CanAddToLibrary);
        LinkClickCommand = new RelayCommand<LinkClickedEventArgs>(args =>
        {
            if (args.HRef != null)
            {
                IOperatingSystem.Current.OpenUri(args.HRef.AbsoluteUri);
            }
        });

        BackToListCommand = new RelayCommand(BackToList);
        SelectExtensionCommand = new RelayCommand<AvailableContentViewModel>(SelectExtension);
        SelectOwnedExtensionCommand = new RelayCommand<OwnedProductViewModel>(ext =>
        {
            var availableExt = AvailableExtensions.FirstOrDefault(e => e.AvailableContent.Id == ext.ProductData.Id);
            if (availableExt != null)
            {
                SelectExtension(availableExt);
            }
            else
            {
                var created = new AvailableContentViewModel(new AvailableContent
                {
                    Id = ext.ProductData.Id,
                    Name = ext.ProductData.DisplayName,
                    Description = ext.ProductData.Description,
                    Author = ext.ProductData.Author,
                    HideAddToLibrary = true,
                    Body = ext.ProductData.Description
                }, this, 1m, "PLN");

                SelectExtension(created);
            }
        });

        OpenPurchaseLinkCommand = new AsyncRelayCommand<string>(OpenPurchaseLink);

        SelectedTab = Tabs.FirstOrDefault(tab => tab.Id == "All");

        OnPropertyChanged(nameof(IsUserLoggedIn));
        identityProvider.OnLoggedIn += _ =>
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
                await (identityProvider as PixiAuthIdentityProvider)?.RefreshOwnedProducts());
            UpdateOwnedStates();
        };
        identityProvider.LoggedOut += () =>
        {
            UpdateOwnedStates();
        };
        identityProvider.OwnedProductsUpdated += _ => UpdateOwnedExtensions();
    }

    public async Task FetchAvailableExtensions()
    {
        AvailableExtensions.Clear();
        var availableExtensions = await contentProvider.FetchAvailableExtensions();

        decimal rate = 1m;
        if (PixiEditorSettings.Extensions.DisplayedCurrency?.Value == null)
        {
            await SetUserCurrencyFromLocation();
        }

        string selectedCurrency = PixiEditorSettings.Extensions.DisplayedCurrency.Value;
        if (selectedCurrency != "PLN")
        {
            var fetchedRate = await NbpFetcher.FetchExchangeRate(selectedCurrency);
            rate = fetchedRate ?? 1m;
        }

        foreach (var extension in availableExtensions)
        {
            AvailableExtensions.Add(new AvailableContentViewModel(extension, this, rate, selectedCurrency));
        }
    }

    public void FetchOwnedExtensions()
    {
        OwnedExtensions.Clear();

        if (identityProvider.User != null && identityProvider.User.OwnedProducts != null)
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

                bool isEnabled = IsLoaded(extension.Id) &&
                                 !PixiEditorSettings.Extensions.DisabledExtensions.Value.Contains(extension.Id);
                bool isLoaded = IsLoaded(extension.Id);

                OwnedExtensions.Add(new OwnedProductViewModel(extension, isInstalled, installedVersion, isEnabled,
                    isLoaded, InstallAndLoadExtensionCommand, UninstallExtensionCommand, EnableExtensionCommand,
                    DisableExtensionCommand, IsInstalled, AreDependenciesReachable, CountLoadedDependencies));
            }

            RefreshDependenciesState();
        }

        // Add installed extensions that aren't in user owned products
        foreach (Extension loadedExtension in extensionsViewModel.ExtensionLoader.LoadedExtensions)
        {
            AddToOwnedExtensionsIfMissing(loadedExtension.Metadata, new ExtensionResourceStorage(loadedExtension as WasmExtensionInstance));
        }

        foreach (var unloadedExtensionMetadata in extensionsViewModel.ExtensionLoader
                     .UnloadedExtensionsMetadata)
        {
            AddToOwnedExtensionsIfMissing(unloadedExtensionMetadata.metadata, null);
        }
    }

    public void AddToOwnedExtensionsIfMissing(ExtensionMetadata extensionMetadata, IResourceStorage? storage)
    {
        bool owned = OwnedExtensions.Any(owned => owned.ProductData.Id == extensionMetadata.UniqueName);

        if (!owned)
        {
            ProductData productData = new ProductData(extensionMetadata.UniqueName, extensionMetadata.DisplayName);
            productData.Author = extensionMetadata.Author.Name;
            productData.Description = extensionMetadata.Description;
            productData.ImageUrl = extensionMetadata.Image;

            bool isInstalled = IsInstalled(extensionMetadata.UniqueName);
            bool isEnabled = IsLoaded(extensionMetadata.UniqueName);
            bool isLoaded = IsLoaded(extensionMetadata.UniqueName);

            OwnedExtensions.Add(new OwnedProductViewModel(productData, isInstalled, extensionMetadata.Version,
                isEnabled, isLoaded, InstallAndLoadExtensionCommand, UninstallExtensionCommand, EnableExtensionCommand,
                DisableExtensionCommand, IsInstalled, AreDependenciesReachable, CountLoadedDependencies, storage)
            );
        }
    }


    public bool IsExtensionOwned(string productId)
    {
        if (identityProvider.User != null && identityProvider.User.OwnedProducts != null)
        {
            return identityProvider.User.OwnedProducts
                .Any(p => p.Id == productId);
        }

        return false;
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

        List<string> installedExtensionsIds =
            await extensionsViewModel.InstallAndLoadExtensionWithDependencies(contentProvider, extensionId);
        RefreshInstalledExtensions(installedExtensionsIds);
        RefreshDependenciesState();
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
        if (!IsUserLoggedIn)
        {
            OwnedExtensions.Remove(OwnedExtensions.FirstOrDefault(e => e.ProductData.Id == extensionId));
        }
    }

    public async Task AddToLibrary(string extensionId)
    {
        if (string.IsNullOrEmpty(extensionId))
        {
            return;
        }

        try
        {
            var provider = (identityProvider as PixiAuthIdentityProvider);
            if (provider == null)
            {
                ErrorMessage = "Identity Provider is not available. Are you using an official PixiEditor build?";
                return;
            }

            if (!provider.IsLoggedIn)
            {
                ErrorMessage = "LOGIN_REQUIRED";
                return;
            }

            await provider.PixiAuthClient.AddProductToLibrary(extensionId, provider.User.SessionToken);
            await provider.RefreshOwnedProducts();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private bool IsLoaded(string extensionId)
    {
        return extensionsViewModel.IsLoaded(extensionId);
    }

    private bool CanAddToLibrary(string extensionId)
    {
        return !IsExtensionOwned(extensionId) && IsFree(extensionId);
    }

    private bool IsFree(string extensionId)
    {
        var extension = AvailableExtensions.FirstOrDefault(e => e.AvailableContent.Id == extensionId);
        return extension is { IsFree: true };
    }

    private bool AreDependenciesReachable(string extensionId)
    {
        var extensionMetadata = extensionsViewModel.ExtensionLoader.LoadedExtensions
            .FirstOrDefault(x => x.Metadata.UniqueName == extensionId)?.Metadata;

        if (extensionMetadata == null)
        {
            extensionMetadata = extensionsViewModel.ExtensionLoader.UnloadedExtensionsMetadata
                .FirstOrDefault(x => x.metadata.UniqueName == extensionId).metadata;
            if (extensionMetadata == null)
            {
                return false;
            }
        }

        foreach (var dep in extensionMetadata.DependsOn)
        {
            if (!IsInstalled(dep))
            {
                var availableDep = OwnedExtensions.FirstOrDefault(e => e.ProductData.Id == dep);
                if (availableDep == null)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public bool CanEnableExtension(string extensionId)
    {
        return IsInstalled(extensionId) && AreDependenciesReachable(extensionId);
    }

    public async Task EnableExtension(string extensionId)
    {
        if (string.IsNullOrEmpty(extensionId))
        {
            return;
        }

        await extensionsViewModel.EnableExtension(extensionId, contentProvider);
        RefreshDependenciesState();
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
                string url =
                    pixiAuthIdentityProvider.PixiAuthClient.GetCreateCheckoutSessionFromSessionIdUrl(sessionId,
                        extensionId);
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

    public async Task SetUserCurrencyFromLocation()
    {
        string currency = await GeoFetcher.GetUserCurrency();
        PixiEditorSettings.Extensions.DisplayedCurrency.Value = currency;
    }

    private void UpdateOwnedStates()
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

            foreach (var ext in AvailableExtensions)
            {
                ext.NotifyChanged();
            }
        });
    }

    private void UpdateOwnedExtensions()
    {
        FetchOwnedExtensions();

        foreach (var ext in AvailableExtensions)
        {
            ext.NotifyChanged();
        }
    }

    public void RefreshInstalledExtensions(List<string> installedExtensionIds)
    {
        var userDisabled = PixiEditorSettings.Extensions.DisabledExtensions.Value.ToList();
        foreach (var extId in installedExtensionIds)
        {
            var owned = OwnedExtensions.FirstOrDefault(x => x.ProductData.Id == extId);
            if (owned != null)
            {
                owned.IsInstalled = IsInstalled(extId);
                owned.IsLoaded = IsLoaded(extId);
                owned.IsEnabled = IsLoaded(extId) && !userDisabled.Contains(extId);

                owned.CanBeEnabled = AreDependenciesReachable(extId);

                owned.InstallCommand.NotifyCanExecuteChanged();
                owned.UninstallCommand.NotifyCanExecuteChanged();
                owned.ToggleEnabledCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public void RefreshDependenciesState()
    {
        foreach (var ext in OwnedExtensions)
        {
            ext.CanBeEnabled = AreDependenciesReachable(ext.ProductData.Id);
            ext.ToggleEnabledCommand.NotifyCanExecuteChanged();
        }

        RefreshInstalledExtensions(extensionsViewModel.ExtensionLoader.LoadedExtensions
            .Select(x => x.Metadata.UniqueName).ToList());
    }

    public int CountLoadedDependencies(string extensionId)
    {
        int count = 0;
        foreach (var ext in extensionsViewModel.ExtensionLoader.LoadedExtensions)
        {
            if (ext.Metadata.DependsOn.Contains(extensionId))
            {
                count++;
            }
        }

        return count;
    }
}
