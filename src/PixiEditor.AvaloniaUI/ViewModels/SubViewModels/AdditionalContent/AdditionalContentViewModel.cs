using PixiEditor.Platform;

namespace PixiEditor.AvaloniaUI.ViewModels.SubViewModels.AdditionalContent;

internal class AdditionalContentViewModel : ViewModelBase
{
    public IAdditionalContentProvider AdditionalContentProvider { get; }
    public AdditionalContentViewModel(IAdditionalContentProvider additionalContentProvider)
    {
        AdditionalContentProvider = additionalContentProvider;
    }

    public bool IsSupporterPackAvailable => AdditionalContentProvider != null && AdditionalContentProvider.IsContentInstalled(AdditionalContentProduct.SupporterPack);
}
