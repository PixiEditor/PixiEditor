﻿using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Extensions;
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

    private void RegisterCoreWindows(WindowProvider? windowProvider)
    {
        windowProvider?.RegisterWindow<PalettesBrowser>();
    }

    private void Owner_OnStartupEvent(object sender, EventArgs e)
    {
        ExtensionLoader.InitializeExtensions(new ExtensionServices(Owner.Services));
    }
}