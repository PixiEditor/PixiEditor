using PixiEditor.Models;
using PixiEditor.SDK;
using System;
using System.IO;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class ExtensionViewModel : SubViewModel<ViewModelMain>
    {
        internal SDKManager SDKManager { get; set; }

        public ExtensionViewModel(ViewModelMain owner)
            : base(owner)
        {
            SDKManager = new SDKManager();

            SDKManager.AddBaseExtension(new BaseExtension());

            SDKManager.LoadExtensions(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PixiEditor", "Extensions"));
            SDKManager.SetupExtensions();
        }
    }
}
