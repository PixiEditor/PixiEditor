using System.Collections.ObjectModel;
using System.Diagnostics;
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
using PixiEditor.Extensions.Runtime;
using PixiEditor.Extensions.WasmRuntime;
using PixiEditor.Extensions.WasmRuntime.Utilities;
using PixiEditor.IdentityProvider;
using PixiEditor.IdentityProvider.PixiAuth;
using PixiEditor.Models.Commands.XAML;
using PixiEditor.Models.ExternalServices;
using PixiEditor.Models.IO;
using PixiEditor.OperatingSystem;
using PixiEditor.PixiAuth.Exceptions;
using PixiEditor.PixiAuth.Models;
using PixiEditor.Platform;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.SubViewModels;
using PixiEditor.ViewModels.User;

namespace PixiEditor.ViewModels.ExtensionManager;

internal class ExtensionManagerViewModel : ViewModelBase
{
    public ObservableCollection<AvailableContentViewModel> AvailableExtensions { get; } =
        new ObservableCollection<AvailableContentViewModel>();

    public ObservableCollection<AvailableContentViewModel> FeaturedExtensions { get; } =
        new ObservableCollection<AvailableContentViewModel>();

    // TODO: refactor to LibraryExtensions - it's made from owned and custom installed
    public ObservableCollection<OwnedProductViewModel> OwnedExtensions { get; } =
        new ObservableCollection<OwnedProductViewModel>();

    public AsyncRelayCommand<string> AddToLibraryCommand { get; }
    public AsyncRelayCommand<string> InstallAndLoadExtensionCommand { get; }
    public AsyncRelayCommand<string> UninstallExtensionCommand { get; }
    public AsyncRelayCommand<string> EnableExtensionCommand { get; }
    public RelayCommand<string> DisableExtensionCommand { get; }
    public AsyncRelayCommand<string> UpdateExtensionCommand { get; }

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

    public bool ShowAllTab => SelectedTab != null && SelectedTab.Id == "All" && !IsPlatformSteam;
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

    public string DetailsErrorMessage
    {
        get => detailsErrorMessage;
        set => SetProperty(ref detailsErrorMessage, value);
    }

    public string AvailableErrorMessage
    {
        get => availableErrorMessage;
        set => SetProperty(ref availableErrorMessage, value);
    }

    public bool IsAvailableFetching
    {
        get => isAvailableFetching;
        set => SetProperty(ref isAvailableFetching, value);
    }

    public bool IsDetailsVisible => SelectedAvailableExtension != null;
    public bool IsListVisible => SelectedAvailableExtension == null;

    private ExtensionsViewModel extensionsViewModel;
    private IAdditionalContentProvider contentProvider;
    private IIdentityProvider identityProvider;
    private IPlatform platform;

    private string detailsErrorMessage;
    private string availableErrorMessage;
    private bool isAvailableFetching;

    public bool IsUserLoggedIn => identityProvider.User != null && identityProvider.User.IsLoggedIn;
    public bool IsPlatformSteam => platform.Id == "steam";
    public RelayCommand<LinkClickedEventArgs> LinkClickCommand { get; }
    public ExtensionsTab LibraryTab => Tabs.FirstOrDefault(tab => tab.Id == "Owned");
    public ExtensionsLayout ExtensionsLayout { get; set; }

    public AvailableContentViewModel FirstHighlightedExtension =>
        AvailableExtensions.FirstOrDefault(e => e.HighlightData != null);

    public AvailableContentViewModel SecondHighlightedExtension =>
        AvailableExtensions.Where(e => e.HighlightData != null).Skip(1).FirstOrDefault();

    public bool ShouldUpdateUserOwnedProducts = false;

    public ExtensionManagerViewModel(ExtensionsViewModel extensionsViewModel,
        IAdditionalContentProvider contentProvider, IIdentityProvider identityProvider, IPlatform platform)
    {
        this.extensionsViewModel = extensionsViewModel;
        this.contentProvider = contentProvider;
        this.identityProvider = identityProvider;
        this.platform = platform;

        InstallAndLoadExtensionCommand =
            new AsyncRelayCommand<string>(InstallAndLoadExtension, CanInstallAndLoadExtension);
        UninstallExtensionCommand = new AsyncRelayCommand<string>(UninstallExtension, CanUninstallExtension);
        EnableExtensionCommand = new AsyncRelayCommand<string>(EnableExtension, CanEnableExtension);
        DisableExtensionCommand = new RelayCommand<string>(DisableExtension, CanDisableExtension);
        UpdateExtensionCommand = new AsyncRelayCommand<string>(UpdateExtension, CanUpdateExtension);
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
                var created = new AvailableContentViewModel(
                    new AvailableContent
                    {
                        Id = ext.ProductData.Id,
                        Name = ext.ProductData.DisplayName,
                        Description = ext.ProductData.Description,
                        Author = ext.ProductData.Author,
                        HideAddToLibrary = true,
                        IsBundle = ext.ProductData.IsBundle,
                        Body = ext.ProductData.Description,
                        Versions = ext.ProductData?.LatestVersion != null ? new List<ExtensionVersion>()
                        {
                            new ExtensionVersion()
                            {
                                Version = ext.ProductData.LatestVersion,
                                PixiEditorApiVersion = ExtensionRuntimeInfo.ApiVersion // TODO: This is not a true value, it would have to call extension to get the real api version
                            }
                        } : new List<ExtensionVersion>()
                    }, this, 1, "PLN", false);

                SelectExtension(created);
            }
        });

        OpenPurchaseLinkCommand = new AsyncRelayCommand<string>(OpenPurchaseLink);

        SelectedTab = Tabs.FirstOrDefault(tab => tab.Id == "All");

        OnPropertyChanged(nameof(IsUserLoggedIn));
        if (identityProvider != null)
        {
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
    }

    private bool CanUpdateExtension(string? id)
    {
        return UpdateAvailable(id);
    }

    public async Task FetchAvailableExtensions()
    {
        AvailableExtensions.Clear();
        FeaturedExtensions.Clear();
        List<AvailableContent> availableExtensions = new List<AvailableContent>();
        try
        {
            IsAvailableFetching = true;
            AvailableErrorMessage = "";
            if (PixiEditorSettings.Extensions.LastFetchedAvailableExtensionsDate.Value.AddHours(24) > DateTime.Now &&
                File.Exists(Path.Combine(Paths.LocalPath, "available_extensions_cache.json")))
            {
                try
                {
                    availableExtensions.AddRange(LoadCachedAvailableExtensions());
                }
                catch (Exception)
                {
                    availableExtensions.AddRange(await contentProvider.FetchAvailableExtensions());
                }
            }
            else
            {
                availableExtensions.AddRange(await contentProvider.FetchAvailableExtensions());
            }

            ExtensionsLayout = await LoadExtensionsLayout();
        }
        catch (Exception ex)
        {
            AvailableErrorMessage = new LocalizedString("FAILED_FETCH_EXTENSIONS", ex.Message);
            return;
        }
        finally
        {
            IsAvailableFetching = false;
        }

        SaveAvailableExtensionsToCache(availableExtensions);
        double rate = 1;
        if (PixiEditorSettings.Extensions.DisplayedCurrency?.Value == null)
        {
            await SetUserCurrencyFromLocation();
        }

        string selectedCurrency = PixiEditorSettings.Extensions.DisplayedCurrency.Value;
        if (selectedCurrency != "PLN")
        {
            DateTime lastFetchedExchangeRateDate = PixiEditorSettings.Extensions.LastFetchedExchangeRateDate.Value;
            if (lastFetchedExchangeRateDate.AddHours(24) > DateTime.Now)
            {
                rate = PixiEditorSettings.Extensions.LastFetchedExchangeRate.Value;
            }
            else
            {
                var fetchedRate = await NbpFetcher.FetchExchangeRate(selectedCurrency);
                rate = fetchedRate ?? 1d;
                PixiEditorSettings.Extensions.LastFetchedExchangeRate.Value = rate;
                PixiEditorSettings.Extensions.LastFetchedExchangeRateDate.Value = DateTime.Now;
            }
        }

        foreach (var extension in availableExtensions)
        {
            var vm =
            new AvailableContentViewModel(extension, this, rate, selectedCurrency,
                (IsPlatformSteam && !IsExtensionOwned(extension.Id)));
            if (ExtensionsLayout != null)
            {
                HighlightData? highlightData = ExtensionsLayout.HighlightedExtensions
                    .FirstOrDefault(h => h.ExtensionId == extension.Id);
                if (highlightData != null)
                {
                    vm.HighlightData = highlightData;
                }

                if(ExtensionsLayout.FeaturedExtensionIds.Contains(extension.Id))
                {
                    FeaturedExtensions.Add(vm);
                }
            }

            AvailableExtensions.Add(vm);
        }

        OnPropertyChanged(nameof(FirstHighlightedExtension));
        OnPropertyChanged(nameof(SecondHighlightedExtension));
    }

    public void FetchOwnedExtensions()
    {
        List<OwnedProductViewModel> toRemove = new List<OwnedProductViewModel>();
        List<string> existing = new List<string>();

        if (identityProvider.User != null && identityProvider.User.OwnedProducts != null && !IsPlatformSteam)
        {
            var extensions = identityProvider.User.OwnedProducts;

            foreach (var owned in OwnedExtensions)
            {
                if (extensions.All(e => e.Id != owned.ProductData.Id))
                {
                    toRemove.Add(owned);
                }
            }

            foreach (var ext in extensions)
            {
                if (OwnedExtensions.Any(owned => owned.ProductData.Id == ext.Id))
                {
                    existing.Add(ext.Id);
                }
            }

            foreach (var owned in toRemove)
            {
                OwnedExtensions.Remove(owned);
            }

            foreach (ProductData extension in extensions)
            {
                if (existing.Contains(extension.Id))
                {
                    continue;
                }

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
                    DisableExtensionCommand, UpdateExtensionCommand, IsInstalled, AreDependenciesReachable,
                    CountLoadedDependencies));
            }

            RefreshDependenciesState();
        }

        // Add installed extensions that aren't in user owned products
        foreach (Extension loadedExtension in extensionsViewModel.ExtensionLoader.LoadedExtensions)
        {
            AddToOwnedExtensionsIfMissing(loadedExtension.Metadata,
                new ExtensionResourceStorage(loadedExtension as WasmExtensionInstance));
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
                DisableExtensionCommand, UpdateExtensionCommand, IsInstalled, AreDependenciesReachable,
                CountLoadedDependencies, storage)
            );
        }
    }


    public bool IsExtensionOwned(string productId)
    {
        return contentProvider.IsContentOwned(productId);
    }

    public bool CanInstallAndLoadExtension(string extensionId)
    {
        return !IsInstalled(extensionId);
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
            await extensionsViewModel.InstallAndLoadExtensionWithDependencies(contentProvider, extensionId, false);
        RefreshInstalledExtensions(installedExtensionsIds);
        RefreshDependenciesState();
    }

    public async Task UpdateExtension(string extensionId)
    {
        if (string.IsNullOrEmpty(extensionId))
        {
            return;
        }

        List<string> installedExtensionsIds =
            await extensionsViewModel.InstallAndLoadExtensionWithDependencies(contentProvider, extensionId, true);
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
                DetailsErrorMessage = "Identity Provider is not available. Are you using an official PixiEditor build?";
                return;
            }

            if (!provider.IsLoggedIn)
            {
                DetailsErrorMessage = "LOGIN_REQUIRED";
                return;
            }

            await provider.PixiAuthClient.AddProductToLibrary(extensionId, provider.User.SessionToken);
            await provider.RefreshOwnedProducts();
        }
        catch (Exception ex)
        {
            DetailsErrorMessage = ex.Message;
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

    private (bool allReachable, string[] nonReachable) AreDependenciesReachable(string extensionId)
    {
        List<string> nonReachable = new List<string>();
        var extensionMetadata = extensionsViewModel.ExtensionLoader.LoadedExtensions
            .FirstOrDefault(x => x.Metadata.UniqueName == extensionId)?.Metadata;

        if (extensionMetadata == null)
        {
            extensionMetadata = extensionsViewModel.ExtensionLoader.UnloadedExtensionsMetadata
                .FirstOrDefault(x => x.metadata.UniqueName == extensionId).metadata;
            if (extensionMetadata == null)
            {
                return (false, Array.Empty<string>());
            }
        }


        foreach (var dep in extensionMetadata.DependsOn)
        {
            if (!IsInstalled(dep))
            {
                var availableDep = OwnedExtensions.FirstOrDefault(e => e.ProductData.Id == dep);
                if (availableDep == null)
                {
                    nonReachable.Add(dep);
                }
            }
        }

        return (nonReachable.Count == 0, nonReachable.ToArray());
    }

    public bool CanEnableExtension(string extensionId)
    {
        return IsInstalled(extensionId) && AreDependenciesReachable(extensionId).allReachable;
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

    private async Task<ExtensionsLayout?> LoadExtensionsLayout()
    {
        try
        {
            var lastFetchedDate = PixiEditorSettings.Extensions.LastFetchedExtensionsLayoutDate.Value;
            if (lastFetchedDate.AddHours(24) > DateTime.Now)
            {
                var cache = await LoadLayoutFromCache();
                if (cache != null)
                {
                    return cache;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Failed to load extensions layout from cache: " + ex.Message);
            return await FetchExtensionsLayout();
        }

        try
        {
            return await FetchExtensionsLayout();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Failed to fetch extensions layout: " + ex.Message);
            return null;
        }
    }

    private async Task<ExtensionsLayout?> FetchExtensionsLayout()
    {
        var layout = await contentProvider.FetchExtensionsLayout();
        if (layout != null)
        {
            string cachePath = Path.Combine(Paths.LocalPath, "extensions_layout_cache.json");
            try
            {
                string json = System.Text.Json.JsonSerializer.Serialize(layout);
                await File.WriteAllTextAsync(cachePath, json);
                PixiEditorSettings.Extensions.LastFetchedExtensionsLayoutDate.Value = DateTime.Now;
                return layout;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to save extensions layout to cache: " + ex.Message);
            }
        }

        return null;
    }


    private static async Task<ExtensionsLayout?> LoadLayoutFromCache()
    {
        string cachePath = Path.Combine(Paths.LocalPath, "extensions_layout_cache.json");
        if (File.Exists(cachePath))
        {
            string json = await File.ReadAllTextAsync(cachePath);
            var cachedLayout = System.Text.Json.JsonSerializer.Deserialize<ExtensionsLayout>(json);
            if (cachedLayout != null)
            {
                return cachedLayout;
            }
        }

        return null;
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

    private bool AnyUpdatesAvailable()
    {
        foreach (var owned in OwnedExtensions)
        {
            if (owned.UpdateAvailable)
            {
                return true;
            }
        }

        return false;
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

                var deps = AreDependenciesReachable(extId);
                owned.CanBeEnabled = deps.allReachable;
                owned.MissingDeps = deps.nonReachable;

                owned.InstallCommand.NotifyCanExecuteChanged();
                owned.UninstallCommand.NotifyCanExecuteChanged();
                owned.ToggleEnabledCommand.NotifyCanExecuteChanged();
            }
        }

        Tabs[1].ShowStatusIndicator = AnyUpdatesAvailable();
    }

    public void RefreshDependenciesState()
    {
        foreach (var ext in OwnedExtensions)
        {
            var deps = AreDependenciesReachable(ext.ProductData.Id);
            ext.CanBeEnabled = deps.allReachable;
            ext.MissingDeps = deps.nonReachable;
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

    private List<AvailableContent> LoadCachedAvailableExtensions()
    {
        string cachePath = Path.Combine(Paths.LocalPath, "available_extensions_cache.json");
        if (File.Exists(cachePath))
        {
            string json = File.ReadAllText(cachePath);
            var cachedExtensions = System.Text.Json.JsonSerializer.Deserialize<List<AvailableContent>>(json);
            if (cachedExtensions != null)
            {
                return cachedExtensions;
            }
        }

        return new List<AvailableContent>();
    }

    private void SaveAvailableExtensionsToCache(List<AvailableContent> availableExtensions)
    {
        string cachePath = Path.Combine(Paths.LocalPath, "available_extensions_cache.json");
        try
        {
            string json = System.Text.Json.JsonSerializer.Serialize(availableExtensions);
            File.WriteAllText(cachePath, json);
            PixiEditorSettings.Extensions.LastFetchedAvailableExtensionsDate.Value = DateTime.Now;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Failed to save available extensions to cache: " + ex.Message);
        }
    }
}
