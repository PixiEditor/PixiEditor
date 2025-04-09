using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Input;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.Exceptions;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Commands.Templates;
using PixiEditor.Models.Dialogs;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.Views.Shortcuts;

internal partial class ImportShortcutTemplatePopup : PixiEditorPopup
{
    public IEnumerable<ShortcutProvider> Templates { get; set; }

    public ImportShortcutTemplatePopup()
    {
        Templates = ShortcutProvider.GetProviders();
        InitializeComponent();
    }

    [Command.Internal("PixiEditor.Shortcuts.Provider.ImportDefault")]
    public static void ImportDefaults(ShortcutProvider provider)
    {
        ImportDefaults(provider, false);
    }

    public static void ImportDefaults(ShortcutProvider provider, bool quiet)
    {
        if (provider is not IShortcutDefaults defaults)
        {
            throw new ArgumentException("Provider must implement IShortcutDefaults", nameof(provider));
        }

        CommandController.Current.ResetShortcuts();
        CommandController.Current.Import(defaults.DefaultShortcuts);

        if (!quiet)
            Success(provider);
    }

    [Command.Internal("PixiEditor.Shortcuts.Provider.ImportInstallation")]
    public static void ImportInstallation(ShortcutProvider provider)
    {
        ImportInstallation(provider, false);
    }

    public static void ImportInstallation(ShortcutProvider provider, bool quiet)
    {
        if (provider is not IShortcutInstallation defaults)
        {
            throw new ArgumentException("Provider must implement IShortcutInstallation", nameof(provider));
        }

        CommandController.Current.ResetShortcuts();

        try
        {
            CommandController.Current.Import(defaults.GetInstalledShortcuts().Shortcuts);
        }
        catch (RecoverableException e)
        {
            NoticeDialog.Show(e.DisplayMessage, "ERROR");
            return;
        }

        if (!quiet)
        {
            Success(provider);
        }
    }

    private static void Success(ShortcutProvider provider) =>
        NoticeDialog.Show(new LocalizedString("SHORTCUTS_IMPORTED", provider.Name), "SUCCESS");

    // TODO figure out what these are for
    /*
    private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
    {
        SystemCommands.CloseWindow(this);
    }*/


    /// <summary>
    ///     Imports shortcuts from a provider. If provider has installation available, then user will be asked to choose between installation and defaults.
    /// </summary>
    /// <param name="provider">Shortcut provider.</param>
    /// <returns>True if imported shortcuts.</returns>
    public static async Task<bool> ImportFromProvider(ShortcutProvider? provider, bool quiet = false)
    {
        if (provider is null)
            return false;
        if (provider is { ProvidesFromInstallation: true, HasInstallationPresent: true })
        {
            if (!quiet)
            {
                OptionsDialog<string> dialog =
                    new(provider.Name, new LocalizedString("SHORTCUT_PROVIDER_DETECTED", provider.Name),
                        MainWindow.Current!)
                    {
                        {
                            new LocalizedString("IMPORT_INSTALLATION_OPTION1"), x => ImportInstallation(provider, quiet)
                        },
                        { new LocalizedString("IMPORT_INSTALLATION_OPTION2"), x => ImportDefaults(provider, quiet) },
                    };
                return await dialog.ShowDialog();
            }

            ImportInstallation(provider, quiet);
            return true;
        }

        if (provider.HasDefaultShortcuts)
        {
            ImportDefaults(provider, quiet);
            return true;
        }

        return false;
    }

    private async void OnTemplateCardPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton != MouseButton.Left)
            return;
        ShortcutsTemplateCard? card = (ShortcutsTemplateCard?)sender;
        ShortcutProvider? provider = card?.DataContext as ShortcutProvider;

        if (await ImportFromProvider(provider))
        {
            Close();
        }
    }
}
