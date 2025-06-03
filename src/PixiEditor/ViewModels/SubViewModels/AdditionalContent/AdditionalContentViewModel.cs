using PixiEditor.Platform;

namespace PixiEditor.ViewModels.SubViewModels.AdditionalContent;

internal class AdditionalContentViewModel : ViewModelBase
{
    public IAdditionalContentProvider AdditionalContentProvider { get; }
    public AdditionalContentViewModel(IAdditionalContentProvider additionalContentProvider)
    {
        AdditionalContentProvider = additionalContentProvider;
    }

    public bool IsFoundersPackAvailable => AdditionalContentProvider != null
                                            && AdditionalContentProvider.IsContentOwned("PixiEditor.FoundersPack")
                                            && ViewModelMain.Current.ExtensionsSubViewModel.ExtensionLoader.LoadedExtensions.Any(x => x.Metadata.UniqueName == "PixiEditor.FoundersPack");
}
