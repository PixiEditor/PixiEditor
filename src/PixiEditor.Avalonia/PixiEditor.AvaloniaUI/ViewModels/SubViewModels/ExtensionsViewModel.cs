using Microsoft.Extensions.DependencyInjection;
using PixiEditor.AvaloniaUI.Models.AppExtensions;
using PixiEditor.AvaloniaUI.Models.AppExtensions.Services;
using PixiEditor.AvaloniaUI.Views.Windows;
using PixiEditor.Extensions;
using PixiEditor.Extensions.Windowing;

namespace PixiEditor.AvaloniaUI.ViewModels.SubViewModels;

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
