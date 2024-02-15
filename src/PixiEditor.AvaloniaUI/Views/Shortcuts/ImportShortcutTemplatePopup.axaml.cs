using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using PixiEditor.AvaloniaUI.Models.Commands;
using PixiEditor.AvaloniaUI.Models.Commands.Attributes.Commands;
using PixiEditor.AvaloniaUI.Models.Commands.Templates;
using PixiEditor.AvaloniaUI.Models.Dialogs;
using PixiEditor.AvaloniaUI.Views.Dialogs;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.Exceptions;

namespace PixiEditor.AvaloniaUI.Views.Shortcuts;

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
        if (provider is not IShortcutDefaults defaults)
        {
            throw new ArgumentException("Provider must implement IShortcutDefaults", nameof(provider));
        }

        CommandController.Current.ResetShortcuts();
        CommandController.Current.Import(defaults.DefaultShortcuts);

        Success(provider);
    }

    [Command.Internal("PixiEditor.Shortcuts.Provider.ImportInstallation")]
    public static void ImportInstallation(ShortcutProvider provider)
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

        Success(provider);
    }

    private static void Success(ShortcutProvider provider) => NoticeDialog.Show(new LocalizedString("SHORTCUTS_IMPORTED", provider.Name), "SUCCESS");

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
    private async Task<bool> ImportFromProvider(ShortcutProvider? provider)
    {
        if (provider is null)
            return false;
        if (provider.ProvidesFromInstallation && provider.HasInstallationPresent)
        {
            OptionsDialog<string> dialog = new(provider.Name, new LocalizedString("SHORTCUT_PROVIDER_DETECTED"), MainWindow.Current!)
            {
                { new LocalizedString("IMPORT_INSTALLATION_OPTION1"), x => ImportInstallation(provider) },
                { new LocalizedString("IMPORT_INSTALLATION_OPTION2"), x => ImportDefaults(provider) },
            };

            return await dialog.ShowDialog();
        }
        
        if (provider.HasDefaultShortcuts)
        {
            ImportDefaults(provider);
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
