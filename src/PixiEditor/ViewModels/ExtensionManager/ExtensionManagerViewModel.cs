using System.Collections.ObjectModel;
using AvaloniaEdit.Utils;
using PixiEditor.IdentityProvider;
using PixiEditor.PixiAuth.Models;
using PixiEditor.Platform;

namespace PixiEditor.ViewModels.ExtensionManager;

internal class ExtensionManagerViewModel : ViewModelBase
{
    private IAdditionalContentProvider contentProvider;
    
    public ObservableCollection<AvailableContent> AvailableExtensions { get; } =
        new ObservableCollection<AvailableContent>();

    public ExtensionManagerViewModel(IAdditionalContentProvider contentProvider)
    {
        this.contentProvider = contentProvider;
    }

    public async Task FetchAvailableExtensions()
    {
        AvailableExtensions.Clear();
        var availableExtensions = await contentProvider.FetchAvailableExtensions();
        AvailableExtensions.AddRange(availableExtensions);
    }
}
