using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.Platform;

namespace PixiEditor.ViewModels.ExtensionManager;

internal class AvailableContentViewModel : ObservableObject
{
    public AvailableContent AvailableContent { get; }
    
    public bool IsOwned => extensionManager.IsExtensionOwned(AvailableContent.Id);
    
    public bool IsBundle => AvailableContent.IncludedExtensions.Count > 0;

    public bool AllBundleItemsOwned =>
        IsBundle && AvailableContent.IncludedExtensions.All(id => extensionManager.IsExtensionOwned(id));
    
    public string CalculatedPrice
    {
        get
        {
            string currency = "$";
            if (AvailableContent.IncludedExtensions.Count > 0)
            {
                int ownedCount = AvailableContent.IncludedExtensions
                    .Count(id => extensionManager.IsExtensionOwned(id));

                int price = AvailableContent.Price -
                    ((AvailableContent.Price / AvailableContent.IncludedExtensions.Count) * ownedCount);

                return $"{price}{currency}";
            }
            
            return $"{AvailableContent.Price}{currency}";
        }
    }
    
    private readonly ExtensionManagerViewModel extensionManager;

    public AvailableContentViewModel(AvailableContent content, ExtensionManagerViewModel extensionManager)
    {
        AvailableContent = content;
        this.extensionManager = extensionManager;
    }
    
    public void NotifyChanged()
    {
        OnPropertyChanged(nameof(IsOwned));
        OnPropertyChanged(nameof(IsBundle));
        OnPropertyChanged(nameof(AllBundleItemsOwned));
        OnPropertyChanged(nameof(CalculatedPrice));
    }
}
