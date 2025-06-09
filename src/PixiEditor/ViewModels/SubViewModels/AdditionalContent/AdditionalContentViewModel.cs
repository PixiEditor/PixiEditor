using PixiEditor.IdentityProvider;
using PixiEditor.Platform;

namespace PixiEditor.ViewModels.SubViewModels.AdditionalContent;

internal class AdditionalContentViewModel : SubViewModel<ViewModelMain>
{
    public IAdditionalContentProvider AdditionalContentProvider { get; }
    public AdditionalContentViewModel(ViewModelMain owner, IAdditionalContentProvider additionalContentProvider) : base(owner)
    {
        AdditionalContentProvider = additionalContentProvider;
        Owner.ExtensionsSubViewModel.ExtensionLoader.ExtensionLoaded += OnExtensionLoaded;
        IPlatform.Current.IdentityProvider.OwnedProductsUpdated += ProviderOnOwnedProductsUpdated;
    }


    public bool IsFoundersPackAvailable => AdditionalContentProvider != null
                                           && AdditionalContentProvider.IsContentOwned("PixiEditor.FoundersPack")
                                           && ViewModelMain.Current.ExtensionsSubViewModel.ExtensionLoader.LoadedExtensions.Any(x => x.Metadata.UniqueName == "PixiEditor.FoundersPack");

    private void ProviderOnOwnedProductsUpdated(List<ProductData> obj)
    {
        OnPropertyChanged(nameof(IsFoundersPackAvailable));
    }

    private void OnExtensionLoaded(string uniqueId)
    {
        if (uniqueId == "PixiEditor.FoundersPack")
        {
            OnPropertyChanged(nameof(IsFoundersPackAvailable));
        }
    }
}
