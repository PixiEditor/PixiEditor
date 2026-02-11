using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.Platform;

namespace PixiEditor.ViewModels.ExtensionManager;

internal class AvailableContentViewModel : ObservableObject
{
    public AvailableContent AvailableContent { get; }
    
    public bool IsOwned => extensionManager.OwnedExtensions.Any(x => x.ProductData.Id == AvailableContent.Id);
    
    private readonly ExtensionManagerViewModel extensionManager;

    public AvailableContentViewModel(AvailableContent content, ExtensionManagerViewModel extensionManager)
    {
        AvailableContent = content;
        this.extensionManager = extensionManager;
    }
}
