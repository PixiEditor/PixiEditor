using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Extensions;
using PixiEditor.Models.AppExtensions;
using PixiEditor.Models.AppExtensions.Services;
using PixiEditor.Models.DataHolders;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.ViewModels.SubViewModels.Main;

internal class ExtensionsViewModel : SubViewModel<ViewModelMain>
{
    public ExtensionLoader ExtensionLoader { get; }
    public ExtensionsViewModel(ViewModelMain owner, ExtensionLoader loader) : base(owner)
    {
        ExtensionLoader = loader;
        ((WindowProvider)Owner.Services.GetService<IWindowProvider>()).RegisterHandler(PalettesBrowser.UniqueId, () =>
        {
            return PalettesBrowser.Open(
                Owner.ColorsSubViewModel.PaletteProvider,
                Owner.ColorsSubViewModel.ImportPaletteCommand,
                Owner.DocumentManagerSubViewModel.ActiveDocument?.Palette);
        });
        Owner.OnStartupEvent += Owner_OnStartupEvent;
    }

    private void Owner_OnStartupEvent(object sender, EventArgs e)
    {
        ExtensionLoader.InitializeExtensions(new ExtensionServices(Owner.Services));
    }
}
