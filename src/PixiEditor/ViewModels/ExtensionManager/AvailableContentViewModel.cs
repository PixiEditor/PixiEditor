using System.Globalization;
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
            decimal price = AvailableContent.Price;
            
            if (AvailableContent.IncludedExtensions.Count > 0)
            {
                int ownedCount = AvailableContent.IncludedExtensions
                    .Count(id => extensionManager.IsExtensionOwned(id));

                price = (price - ((price / AvailableContent.IncludedExtensions.Count) * ownedCount));
            }

            if (Currency != "PLN")
            {
                return $"{((price / Rate) * 1.04m):0.00} {Currency}";
            }
            else
            {
                return $"{price:0.00} {Currency}";
            }
        }
    }
    
    private readonly ExtensionManagerViewModel extensionManager;
    
    private decimal Rate { get; }
    private string Currency { get; }

    public AvailableContentViewModel(AvailableContent content, ExtensionManagerViewModel extensionManager, decimal rate, string currency)
    {
        AvailableContent = content;
        this.extensionManager = extensionManager;
        Rate = rate;
        Currency = currency;
    }
    
    public void NotifyChanged()
    {
        OnPropertyChanged(nameof(IsOwned));
        OnPropertyChanged(nameof(IsBundle));
        OnPropertyChanged(nameof(AllBundleItemsOwned));
        OnPropertyChanged(nameof(CalculatedPrice));
    }
}
