using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Extensions;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.CommonApi.Windowing;
using PixiEditor.Extensions.Runtime;
using PixiEditor.Models.ExtensionServices;
using PixiEditor.Views.Windows;

namespace PixiEditor.ViewModels.SubViewModels;

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

    public void LoadExtensionAdHoc(string extension)
    {
        if (extension.EndsWith(".pixiext"))
        {
            var loadedExtension = ExtensionLoader.LoadExtension(extension);
            if (loadedExtension is null)
            {
                return;
            }

            ILocalizationProvider.Current.LoadExtensionData(loadedExtension);
            loadedExtension.Initialize(new ExtensionServices(Owner.Services));
        }
    }

    private void RegisterCoreWindows(WindowProvider? windowProvider)
    {
        windowProvider?.RegisterWindow<PalettesBrowser>();
    }

    private void Owner_OnStartupEvent()
    {
        ExtensionLoader.InitializeExtensions(new ExtensionServices(Owner.Services));
    }
}
