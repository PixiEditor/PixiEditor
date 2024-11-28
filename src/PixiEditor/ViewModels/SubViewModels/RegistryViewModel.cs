namespace PixiEditor.ViewModels.SubViewModels;

internal class RegistryViewModel : SubViewModel<ViewModelMain>
{
    public RegistryViewModel(ViewModelMain owner) : base(owner)
    {

    }

    // TODO: PixiEditor shouldn't handle any registry stuff. It should be handled by the installers.

    /*private void OwnerOnStartupEvent(object sender, EventArgs e)
    {
#if !STEAM // Something is wrong with Steam version of PixiEditor, so we disable this feature for now. Steam will have native association soon.
        // Check if lospec-palette is associated in registry
        if (!LospecPaletteIsAssociated())
        {
            // Associate lospec-palette URL protocol
            RegistryHelpers.TryAssociate(AssociateLospecPaletteInRegistry, "FAILED_ASSOCIATE_LOSPEC");
        }
#endif
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
    }*/
}
