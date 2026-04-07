using System.Collections.Concurrent;
using System.Diagnostics;
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
using PixiEditor.IdentityProvider.PixiAuth;
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
    
    private ConcurrentQueue<string> installQueue = new ConcurrentQueue<string>();
    private TaskCompletionSource<string>? installTask = null;

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

        var platform = Owner.Services.GetService<IPlatform>();
        
        ExtensionManager = new ExtensionManagerViewModel(this, Owner.UserViewModel.AdditionalContentProvider,
            Owner.UserViewModel.IdentityProvider, platform);
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

    public async Task<List<string>> LoadExtensionWithDependenciesAdHoc(string extensionPath, IAdditionalContentProvider additionalContentProvider)
    {
        var metadata = ExtensionLoader.ReadExtensionMetadata(extensionPath);
        if (metadata is null)
        {
            return [];
        }

        List<DiscoveredExtension> discoveredExtensions = ExtensionLoader.UnloadedExtensionsMetadata
            .Select(x => new DiscoveredExtension { Metadata = x.metadata, Disabled = true, PackagePath = x.path })
            .ToList();

        discoveredExtensions.AddRange(ExtensionManager.OwnedExtensions
            .Where(x => !x.IsInstalled)
            .Select(x => new DiscoveredExtension
            {
                Metadata = new ExtensionMetadata() { UniqueName = x.ProductData.Id },
                Disabled = true,
                PackagePath = null
            }));

        var ordered = ExtensionDependencyResolver.ResolveDependencies(discoveredExtensions, metadata.UniqueName);

        List<string> loadedExtensions = new List<string>();
        foreach (var ext in ordered)
        {
            if (ext.PackagePath == null)
            {
                var installed = await InstallAndLoadExtensionWithDependencies(additionalContentProvider, ext.Metadata.UniqueName, false);
                loadedExtensions.AddRange(installed);
            }
            else if (ext.Disabled)
            {
                LoadExtensionAdHoc(ext.PackagePath);
                loadedExtensions.Add(ext.Metadata.UniqueName);
            }
        }

        return loadedExtensions.Distinct().ToList();
    }

    public void LoadExtensionAdHoc(string extension)
    {
        if (extension.EndsWith(".pixiext"))
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load extension from {extension}: {ex}");
            }
        }
    }

    public async Task<List<string>> InstallAndLoadExtensionWithDependencies(
        IAdditionalContentProvider additionalContentProvider, string productId, bool force)
    {
        try
        {
            if (installTask != null)
            {
                installQueue.Enqueue(productId);
                while (installQueue.Count > 0)
                {
                    await installTask.Task;
                    if (!installQueue.TryPeek(out string next) || next == productId)
                    {
                        break;
                    }
                }

                lock (installTask)
                {
                    installTask = new TaskCompletionSource<string>();
                    installQueue.TryDequeue(out _);
                }
            }
            else
            {
                installTask = new TaskCompletionSource<string>();
            }
            

            List<DiscoveredExtension> installedExtensions = new List<DiscoveredExtension>();

            await InstallRecursive(additionalContentProvider, productId, installedExtensions, force);

            List<DiscoveredExtension> prevInstalledExtensions = new List<DiscoveredExtension>();
            foreach (var loaded in ExtensionLoader.LoadedExtensions)
            {
                if (installedExtensions.All(x => x.Metadata.UniqueName != loaded.Metadata.UniqueName))
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
                if (!installedExtensions.Any(x => x.Metadata.UniqueName == unloaded.metadata.UniqueName))
                {
                    prevInstalledExtensions.Add(new DiscoveredExtension
                    {
                        Metadata = unloaded.metadata,
                        Disabled = !IsLoaded(unloaded.metadata.UniqueName),
                        PackagePath = unloaded.path
                    });
                }
            }

            List<DiscoveredExtension> sortedInstalledExtensions =
                ExtensionDependencyResolver.ResolveDependencies(installedExtensions.Concat(prevInstalledExtensions)
                    .ToList());

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
                        ExtensionLoader.UnloadedExtensionsMetadata.Add((ext.Metadata, ext.PackagePath));
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
        finally
        {
            installTask?.TrySetResult(null);
        }
    }

    private async Task InstallRecursive(IAdditionalContentProvider provider,
        string extensionId,
        List<DiscoveredExtension> installedExtensions, bool force)
    {
        if (provider.IsInstalled(extensionId) && !force)
            return;
        
        if (!provider.IsContentOwned(extensionId))
        {
            if (ExtensionManager.AvailableExtensions == null || ExtensionManager.AvailableExtensions.Count == 0)
            {
                await ExtensionManager.FetchAvailableExtensions();
            }
            
            var ext = ExtensionManager.AvailableExtensions.FirstOrDefault(x =>
                x.AvailableContent.Id == extensionId);
            if (ext.IsFree)
            {
                await ExtensionManager.AddToLibrary(extensionId);
            }
        }

        string? extensionPath = await provider.InstallContent(extensionId);

        if (extensionPath == null)
        {
            // Skip if user doesn't own
            return;
        }

        var metadata = ExtensionLoader.ReadExtensionMetadata(extensionPath);

        if (metadata != null)
        {
            foreach (var dep in metadata.DependsOn)
            {
                await InstallRecursive(provider, dep, installedExtensions, false);
            }
        }

        installedExtensions.Add(new DiscoveredExtension
        {
            Metadata = metadata, Disabled = false, PackagePath = extensionPath
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

    public async Task EnableExtension(string extensionId, IAdditionalContentProvider additionalContentProvider)
    {
        var disabled = PixiEditorSettings.Extensions.DisabledExtensions.Value.ToList();

        var allPossiblePaths = ExtensionLoader.PackagesPath;

        string? extensionPathRoot = allPossiblePaths.FirstOrDefault(x =>
        {
            string pathToTest = Path.Combine(x, $"{extensionId}.pixiext");
            return File.Exists(pathToTest);
        });

        string extensionPath = Path.Combine(extensionPathRoot, $"{extensionId}.pixiext");

        var loaded = await LoadExtensionWithDependenciesAdHoc(extensionPath, additionalContentProvider);

        foreach (var ext in loaded)
        {
            disabled.RemoveAll(x => x == ext);
        }

        PixiEditorSettings.Extensions.DisabledExtensions.Value = disabled;
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
}
