using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Commands.Templates;
using PixiEditor.Models.Dialogs;

namespace PixiEditor.Views.Dialogs;

internal partial class ImportShortcutTemplatePopup : Window
{
    public IEnumerable<ShortcutProvider> Templates { get; set; }

    public ImportShortcutTemplatePopup()
    {
        Templates = ShortcutProvider.GetProviders();
        InitializeComponent();
        SourceInitialized += (_, _) =>
        {
            MinHeight = ActualHeight;
        };
    }

    [Command.Internal("PixiEditor.Shortcuts.Provider.ImportDefault")]
    public static void ImportDefaults(ShortcutProvider provider)
    {
        if (provider is not IShortcutDefaults defaults)
        {
            throw new ArgumentException("provider must implement IShortcutDefaults", nameof(provider));
        }

        CommandController.Current.ResetShortcuts();
        CommandController.Current.Import(defaults.DefaultShortcuts);

        Success(provider);
    }

    [Command.Internal("PixiEditor.Shortcuts.Provider.ImportFile")]
    public static void ImportFile(ShortcutProvider provider)
    {
        if (provider is not IShortcutFile defaults)
        {
            throw new ArgumentException("provider must implement IShortcutFile", nameof(provider));
        }

        var picker = new OpenFileDialog();

        if (!picker.ShowDialog().GetValueOrDefault())
        {
            return;
        }

        try
        {
            var shortcuts = defaults.GetShortcuts(picker.FileName);

            CommandController.Current.ResetShortcuts();
            CommandController.Current.Import(shortcuts);
        }
        catch (FileFormatException)
        {
            NoticeDialog.Show($"The file was not in a correct format", "Error");
            return;
        }

        Success(provider);
    }

    [Command.Internal("PixiEditor.Shortcuts.Provider.ImportInstallation")]
    public static void ImportInstallation(ShortcutProvider provider)
    {
        if (provider is not IShortcutInstallation defaults)
        {
            throw new ArgumentException("provider must implement IShortcutInstallation", nameof(provider));
        }

        CommandController.Current.ResetShortcuts();

        try
        {
            CommandController.Current.Import(defaults.GetInstalledShortcuts());
        }
        catch
        {
            NoticeDialog.Show($"The file was not in a correct format", "Error");
            return;
        }

        Success(provider);
    }

    private static void Success(ShortcutProvider provider) => NoticeDialog.Show($"Shortcuts from {provider.Name} were imported successfully", "Success");

    private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
    {
        SystemCommands.CloseWindow(this);
    }
}
