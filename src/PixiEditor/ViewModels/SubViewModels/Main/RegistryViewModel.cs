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
            AssociateLospecPalette();
        }
    }

    private void AssociateLospecPalette()
    {
        if (!ProcessHelper.IsRunningAsAdministrator())
        {
            ProcessHelper.RunAsAdmin(Process.GetCurrentProcess().MainModule?.FileName);
            Application.Current.Shutdown();
        }
        else
        {
            AssociateLospecPaletteInRegistry();
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

        RegistryKey lospecPaletteKey = Registry.ClassesRoot.OpenSubKey("lospec-palette", RegistryRights.ReadKey);
        return lospecPaletteKey != null;
    }
}
