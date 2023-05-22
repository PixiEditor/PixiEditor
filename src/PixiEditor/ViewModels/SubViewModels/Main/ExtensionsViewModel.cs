using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Extensions;
using PixiEditor.Extensions.Palettes;
using PixiEditor.Models.AppExtensions;
using PixiEditor.Models.DataHolders;

namespace PixiEditor.ViewModels.SubViewModels.Main;

internal class ExtensionsViewModel : SubViewModel<ViewModelMain>
{
    public ExtensionLoader ExtensionLoader { get; }
    public ExtensionsViewModel(ViewModelMain owner) : base(owner)
    {
        ExtensionLoader loader = new ExtensionLoader(new ExtensionServices(owner.Services));
        loader.LoadExtensions();

        ExtensionLoader = loader;
        Owner.OnStartupEvent += Owner_OnStartupEvent;
    }

    private void Owner_OnStartupEvent(object sender, EventArgs e)
    {
        ExtensionLoader.InitializeExtensions();
    }
}
