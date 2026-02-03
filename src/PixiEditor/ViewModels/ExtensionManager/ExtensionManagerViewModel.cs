using System.Collections.ObjectModel;
using AvaloniaEdit.Utils;
using PixiEditor.IdentityProvider;
using PixiEditor.PixiAuth.Models;
using PixiEditor.Platform;
using PixiEditor.ViewModels.User;

namespace PixiEditor.ViewModels.ExtensionManager;

internal class ExtensionManagerViewModel : ViewModelBase
{
    public ObservableCollection<AvailableContent> AvailableExtensions { get; } =
        new ObservableCollection<AvailableContent>();
    
    public ObservableCollection<ProductData> OwnedExtensions { get; } =
        new ObservableCollection<ProductData>();
    
    public ObservableCollection<ProductData> InstalledExtensions { get; } =
        new ObservableCollection<ProductData>();
    
    private IAdditionalContentProvider contentProvider;
    private IIdentityProvider identityProvider;

    public ExtensionManagerViewModel(IAdditionalContentProvider contentProvider, IIdentityProvider identityProvider)
    {
        this.contentProvider = contentProvider;
        this.identityProvider = identityProvider;
    }

    public async Task FetchAvailableExtensions()
    {
        AvailableExtensions.Clear();
        var availableExtensions = await contentProvider.FetchAvailableExtensions();
        AvailableExtensions.AddRange(availableExtensions);
    }
    
    public void FetchOwnedExtensions()
    {
        OwnedExtensions.Clear();
        var ownedExtensions = identityProvider.User.OwnedProducts;
        OwnedExtensions.AddRange(ownedExtensions);
        
        var installedExtensions = ownedExtensions.Where(x => contentProvider.IsInstalled(x.Id)).ToList();
        InstalledExtensions.AddRange(installedExtensions);
    }
    
    
}
