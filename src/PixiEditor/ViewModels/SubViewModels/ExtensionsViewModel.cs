using Avalonia;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Extensions;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Extensions.CommonApi.Windowing;
using PixiEditor.Extensions.Metadata;
using PixiEditor.Extensions.Runtime;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.ExtensionServices;
using PixiEditor.Models.IO;
using PixiEditor.Platform;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.ExtensionManager;
using PixiEditor.Views;
using PixiEditor.Views.Auth;
using PixiEditor.Views.Dialogs;
using PixiEditor.Views.Windows;

namespace PixiEditor.ViewModels.SubViewModels;

internal class ExtensionsViewModel : SubViewModel<ViewModelMain>
{
    public ExtensionLoader ExtensionLoader { get; }
    public ExtensionManagerViewModel ExtensionManager { get; set; }

    public ExtensionsViewModel(ViewModelMain owner, ExtensionLoader loader) : base(owner)
    {
        ExtensionLoader = loader;
    }

    public void Init()
    {
        WindowProvider windowProvider = (WindowProvider)Owner.Services.GetService<IWindowProvider>();
        RegisterCoreWindows(windowProvider);
        Owner.OnEarlyStartupEvent += Owner_OnEarlyStartupEvent;
        Owner.OnUserReady += Owner_OnUserReady;
        if (Owner.AttachedWindow != null)
        {
            OwnerOnAttachedToWindow(Owner.AttachedWindow);
        }
        else
        {
            Owner.AttachedToWindow += OwnerOnAttachedToWindow;
        }
        
        ExtensionManager = new ExtensionManagerViewModel(this, Owner.UserViewModel.AdditionalContentProvider, Owner.UserViewModel.IdentityProvider);
    }

    private void OwnerOnAttachedToWindow(MainWindow obj)
    {
        if (obj.IsLoaded)
        {
            MainWindowLoaded(obj, null);
        }
        else
        {
            obj.Loaded += MainWindowLoaded;
        }
    }

    public void LoadExtensionAdHoc(string extension)
    {
        if (extension.EndsWith(".pixiext"))
        {
            var loadedExtension = ExtensionLoader.LoadExtension(extension);
            if (loadedExtension is null)
            {
                return;
            }

            ILocalizationProvider.Current.LoadExtensionData(loadedExtension.Metadata.Localization?.Languages,
                loadedExtension.Location);
            loadedExtension.Initialize(new ExtensionServices(Owner.Services));
            if (Owner.AttachedWindow != null && Owner.AttachedWindow.IsLoaded)
            {
                loadedExtension.MainWindowLoaded();
            }

            if (Owner.IsUserReady)
            {
                loadedExtension.UserReady();
            }
        }
    }
    
    public async Task<List<string>> InstallAndLoadExtensionWithDependencies(IAdditionalContentProvider additionalContentProvider, string productId)
    {
        try
        {
            List<DiscoveredExtension> installedExtensions = new List<DiscoveredExtension>();
            
            await InstallRecursive(additionalContentProvider, productId, installedExtensions);
            
            List<DiscoveredExtension> prevInstalledExtensions = new List<DiscoveredExtension>();
            foreach (var loaded in ExtensionLoader.LoadedExtensions)
            {
                if (!installedExtensions.Any(x => x.Metadata.UniqueName == loaded.Metadata.UniqueName))
                {
                    prevInstalledExtensions.Add(new DiscoveredExtension
                    {
                        Metadata = loaded.Metadata,
                        Disabled = !IsLoaded(loaded.Metadata.UniqueName),
                        PackagePath = null,
                    });
                }
            }
            foreach (var unloaded in ExtensionLoader.UnloadedExtensionsMetadata)
            {
                if (!installedExtensions.Any(x => x.Metadata.UniqueName == unloaded.UniqueName))
                {
                    prevInstalledExtensions.Add(new DiscoveredExtension
                    {
                        Metadata = unloaded,
                        Disabled = !IsLoaded(unloaded.UniqueName),
                        PackagePath = null,
                    });
                }
            }
            
            List<DiscoveredExtension> sortedInstalledExtensions =
                ExtensionDependencyResolver.ResolveDependencies(installedExtensions.Concat(prevInstalledExtensions).ToList());
            
            foreach (var ext in sortedInstalledExtensions)
            {
                // Only load newly installed extensions
                if (installedExtensions.Any(x => x.Metadata.UniqueName == ext.Metadata.UniqueName))
                {
                    if (!ext.Disabled)
                    {
                        Owner.ExtensionsSubViewModel.LoadExtensionAdHoc(ext.PackagePath);

                    }
                    else
                    {
                        ExtensionLoader.UnloadedExtensionsMetadata.Add(ext.Metadata);
                    }
                }
            }
            return installedExtensions
                .Select(x => x.Metadata.UniqueName)
                .ToList();
        }
        catch (Exception ex)
        {
            CrashHelper.SendExceptionInfo(ex);
            return new List<string>();
        }
    }
    
    private async Task InstallRecursive(
        IAdditionalContentProvider provider,
        string extensionId,
        List<DiscoveredExtension> installedExtensions)
    {
        if (provider.IsInstalled(extensionId))
            return;

        string? extensionPath = await provider.InstallContent(extensionId);

        if (extensionPath == null)
        {
            // Skip if user doesn't own
            return;
        }
        
        var metadata = ExtensionLoader.LoadExtensionMetadata(extensionPath);

        if (metadata != null)
        {
            foreach (var dep in metadata.Dependencies)
            {
                await InstallRecursive(provider, dep, installedExtensions);
            }
        }
        
        installedExtensions.Add(new DiscoveredExtension
        {
            Metadata = metadata,
            Disabled = false,
            PackagePath = extensionPath
        });
    }
    
    public async Task UninstallExtension(string extensionId)
    {
        this.ExtensionLoader.UninstallExtension(extensionId);
    }

    public bool IsLoaded(string extensionId)
    {
        return ExtensionLoader.LoadedExtensions.Any(x => x.Metadata.UniqueName == extensionId);
    }
    
    public void EnableExtension(string extensionId)
    {
        var disabled = PixiEditorSettings.Extensions.DisabledExtensions.Value.ToList();
        disabled.Remove(extensionId);
        PixiEditorSettings.Extensions.DisabledExtensions.Value = disabled;
        
        string extensionPath = Path.Combine(Paths.LocalExtensionPackagesPath, $"{extensionId}.pixiext");
        
        LoadExtensionAdHoc(extensionPath);
    }
    
    public void DisableExtension(string extensionId)
    {
        var disabled = PixiEditorSettings.Extensions.DisabledExtensions.Value.ToList();
        disabled.Add(extensionId);
        PixiEditorSettings.Extensions.DisabledExtensions.Value = disabled;
    }

    private void RegisterCoreWindows(WindowProvider? windowProvider)
    {
        windowProvider?.RegisterWindow<PalettesBrowser>();
        windowProvider?.RegisterWindow<HelloTherePopup>();
        windowProvider?.RegisterWindow<LoginPopup>();
    }

    private void Owner_OnEarlyStartupEvent()
    {
        ExtensionLoader.InitializeExtensions(new ExtensionServices(Owner.Services));
    }

    private void Owner_OnUserReady()
    {
        ExtensionLoader.InvokeOnUserReady();
    }

    private void MainWindowLoaded(object? sender, RoutedEventArgs e)
    {
        ExtensionLoader.InvokeMainWindowLoaded();
    }

    [Command.Basic("PixiEditor.Extensions.OpenExtensionsWindow", "OPEN_EXTENSIONS_WINDOW", "OPEN_EXTENSIONS_WINDOW_DESCRIPTIVE", AnalyticsTrack = true, MenuItemPath = "VIEW/OPEN_EXTENSIONS_WINDOW")]
    public void OpenExtensionsWindow()
    {
        ExtensionsPopup popup = new ExtensionsPopup();
        popup.DataContext = ExtensionManager;
        
        popup.Show();
    }
}
