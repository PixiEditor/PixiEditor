using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Extensions;
using PixiEditor.Models.AppExtensions;

namespace PixiEditor.ViewModels.SubViewModels.Main;

internal class ExtensionsViewModel : SubViewModel<ViewModelMain>
{
    public ExtensionLoader ExtensionLoader { get; }
    public ExtensionsViewModel(ViewModelMain owner) : base(owner)
    {
        var services = new ServiceCollection().AddExtensionServices(owner.ColorsSubViewModel.PaletteDataSources.ToList()).BuildServiceProvider();
        ExtensionLoader loader = new ExtensionLoader(new ExtensionServices(services));
        loader.LoadExtensions();

        ExtensionLoader = loader;
        Owner.OnStartupEvent += Owner_OnStartupEvent;
    }

    private void Owner_OnStartupEvent(object sender, EventArgs e)
    {
        ExtensionLoader.InitializeExtensions();
    }
}
