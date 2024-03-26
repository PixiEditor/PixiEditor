using Microsoft.Extensions.DependencyInjection;
using PixiEditor.AvaloniaUI.Models.ExtensionServices;
using PixiEditor.AvaloniaUI.Views.Windows;
using PixiEditor.Extensions;
using PixiEditor.Extensions.Runtime;
using PixiEditor.Extensions.Windowing;

namespace PixiEditor.AvaloniaUI.ViewModels.SubViewModels;

internal class ExtensionsViewModel : SubViewModel<ViewModelMain>
{
    public ExtensionLoader ExtensionLoader { get; }
    public ExtensionsViewModel(ViewModelMain owner, ExtensionLoader loader) : base(owner)
    {
        ExtensionLoader = loader;
        WindowProvider windowProvider = (WindowProvider)Owner.Services.GetService<IWindowProvider>();

        RegisterCoreWindows(windowProvider);
        Owner.OnStartupEvent += Owner_OnStartupEvent;
    }

    private void RegisterCoreWindows(WindowProvider? windowProvider)
    {
        windowProvider?.RegisterWindow<PalettesBrowser>();
    }

    private void Owner_OnStartupEvent(object sender, EventArgs e)
    {
        ExtensionLoader.InitializeExtensions(new ExtensionServices(Owner.Services));
    }
}
