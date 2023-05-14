using System.Windows;
using System.Windows.Input;
using PixiEditor.Exceptions;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Commands.Templates;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Localization;
using PixiEditor.Views.UserControls;

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

    private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
    {
        SystemCommands.CloseWindow(this);
    }

    private void OnTemplateCardLeftMouseButtonDown(object sender, MouseButtonEventArgs e)
    {
        ShortcutsTemplateCard card = (ShortcutsTemplateCard)sender;
        ShortcutProvider provider = card.DataContext as ShortcutProvider;

        if (ImportFromProvider(provider))
        {
            Close();
        }
    }

    /// <summary>
    ///     Imports shortcuts from a provider. If provider has installation available, then user will be asked to choose between installation and defaults.
    /// </summary>
    /// <param name="provider">Shortcut provider.</param>
    /// <returns>True if imported shortcuts.</returns>
    private bool ImportFromProvider(ShortcutProvider provider)
    {
        if (provider.ProvidesFromInstallation && provider.HasInstallationPresent)
        {
            var result = OptionDialog.Show(
                new LocalizedString("SHORTCUT_PROVIDER_DETECTED", provider.Name),
                "IMPORT_FROM_INSTALLATION",
                "IMPORT_INSTALLATION_OPTION1",
                "IMPORT_INSTALLATION_OPTION2");

            if (result == OptionResult.Option1)
            {
                ImportInstallation(provider);
            }
            else if (result == OptionResult.Option2)
            {
                ImportDefaults(provider);
            }
            
            return result != OptionResult.Canceled;
        }
        
        if (provider.HasDefaultShortcuts)
        {
            ImportDefaults(provider);
            return true;
        }
        
        return false;
    }
}
