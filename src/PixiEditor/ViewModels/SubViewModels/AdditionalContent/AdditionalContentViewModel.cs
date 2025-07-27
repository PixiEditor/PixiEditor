using PixiEditor.IdentityProvider;
using PixiEditor.Platform;

namespace PixiEditor.ViewModels.SubViewModels.AdditionalContent;

internal class AdditionalContentViewModel : SubViewModel<ViewModelMain>
{
    public IAdditionalContentProvider AdditionalContentProvider { get; }
    public AdditionalContentViewModel(ViewModelMain owner, IAdditionalContentProvider additionalContentProvider) : base(owner)
    {
        AdditionalContentProvider = additionalContentProvider;
    }
}
