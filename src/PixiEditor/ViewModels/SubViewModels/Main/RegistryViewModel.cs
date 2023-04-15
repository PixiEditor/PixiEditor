using System.Diagnostics;
using System.Security.AccessControl;
using System.Windows;
using Microsoft.Win32;
using PixiEditor.Helpers;
using PixiEditor.Models.Dialogs;

namespace PixiEditor.ViewModels.SubViewModels.Main;

internal class RegistryViewModel : SubViewModel<ViewModelMain>
{
    public RegistryViewModel(ViewModelMain owner) : base(owner)
    {
        Owner.OnStartupEvent += OwnerOnStartupEvent;
    }

    private void OwnerOnStartupEvent(object sender, EventArgs e)
    {
        // Check if lospec-palette is associated in registry

        if (!LospecPaletteIsAssociated())
        {
            // Associate lospec-palette URL protocol
            RegistryHelpers.TryAssociate(AssociateLospecPaletteInRegistry, "FAILED_ASSOCIATE_LOSPEC");
        }

#if STEAM // Only associate .pixi file if it's a steam version, other versions handle it during installation
        if(!PixiFileIsAssociated())
        {
            RegistryHelpers.TryAssociate(AssociatePixiFileInRegistry, "FAILED_ASSOCIATE_PIXI");
        }
#endif
    }

    private bool PixiFileIsAssociated()
    {
        // Check if HKEY_CLASSES_ROOT\.pixi is present
        return RegistryHelpers.IsKeyPresentInRoot(".pixi");
    }

    private void AssociatePixiFileInRegistry()
    {
        try
        {
            using RegistryKey key = Registry.ClassesRoot.CreateSubKey(".pixi");
            key.SetValue("", "PixiEditor");

            using RegistryKey shellKey = key.CreateSubKey("OpenWithProgids");
        }
        catch
        {
            NoticeDialog.Show("FAILED_ASSOCIATE_PIXI", "ERROR");
        }
    }

    private void AssociateLospecPaletteInRegistry()
    {
        try
        {
            using RegistryKey key = Registry.ClassesRoot.CreateSubKey("lospec-palette");

            key.SetValue("", "PixiEditor");
            key.SetValue("URL Protocol", "");

            // Create a new key
            using RegistryKey shellKey = key.CreateSubKey("shell");
            // Create a new key
            using RegistryKey openKey = shellKey.CreateSubKey("open");
            // Create a new key
            using RegistryKey commandKey = openKey.CreateSubKey("command");
            // Set the default value of the key
            commandKey.SetValue("", $"\"{Process.GetCurrentProcess().MainModule?.FileName}\" \"%1\"");
        }
        catch
        {
            NoticeDialog.Show("Failed to associate lospec-palette protocol", "Error");
        }
    }

    private bool LospecPaletteIsAssociated()
    {
        // Check if HKEY_CLASSES_ROOT\lospec-palette is present
        return RegistryHelpers.IsKeyPresentInRoot("lospec-palette");
    }
}
