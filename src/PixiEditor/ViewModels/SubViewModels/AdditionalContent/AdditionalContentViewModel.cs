using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Platform;

namespace PixiEditor.ViewModels.SubViewModels.AdditionalContent;

internal class AdditionalContentViewModel : ViewModelBase
{
    public IAdditionalContentProvider AdditionalContentProvider { get; }
    public AdditionalContentViewModel(IAdditionalContentProvider additionalContentProvider)
    {
        AdditionalContentProvider = additionalContentProvider;
    }

    public bool IsSupporterPackAvailable =>
#if DEBUG
        true;
#else
        AdditionalContentProvider != null && AdditionalContentProvider.IsContentAvailable(AdditionalContentProduct.SupporterPack);
#endif
}
