using PixiEditor.Extensions.CommonApi.Commands;
using PixiEditor.Extensions.Sdk;

namespace Sample8_Menu;

public class CommandsSampleExtension : PixiEditorExtension
{
    /// <summary>
    ///     This method is called when extension is loaded.
    ///  All extensions are first loaded and then initialized. This method is called before <see cref="OnInitialized"/>.
    /// </summary>
    public override void OnLoaded()
    {
    }

    /// <summary>
    ///     This method is called when extension is initialized. After this method is called, you can use Api property to access PixiEditor API.
    /// </summary>
    public override void OnInitialized()
    {
        // A good practice is to use localization keys instead of hardcoded strings.
        // And add them to the localization file. Check Sample2_LocalizationSample for more information.

        CommandMetadata firstCommand = new CommandMetadata("Loggers.WriteHello");
        firstCommand.DisplayName = "Write Hello"; // can be localized
        firstCommand.Description = "Writes Hello to the log"; // can be localized

        // Either an icon key (https://github.com/PixiEditor/PixiEditor/blob/master/src/PixiEditor.UI.Common/Fonts/PixiPerfectIcons.axaml)
        // or unicode character
        firstCommand.Icon = "icon-terminal";
        firstCommand.MenuItemPath = "AWESOME_LOGGER/Write Hello"; // AWESOME_LOGGER is taken from localization, same can be done for the rest
        firstCommand.Shortcut = new Shortcut(Key.H, KeyModifiers.Control | KeyModifiers.Alt);

        Api.Commands.RegisterCommand(firstCommand, () => { Api.Logger.Log("Hello from the command!"); });

        int clickedCount = 0;
        CommandMetadata secondCommand = new CommandMetadata("Loggers.WriteClickedCount");
        secondCommand.DisplayName = "Write Clicked Count";
        secondCommand.Description = "Writes clicked count to the log";
        secondCommand.Icon = "icon-terminal";
        secondCommand.MenuItemPath = "EDIT/Write Clicked Count"; // append to EDIT menu
        secondCommand.Order = 1000; // Last

        secondCommand.Shortcut = new Shortcut(Key.C, KeyModifiers.Control | KeyModifiers.Alt);
        Api.Commands.RegisterCommand(secondCommand, () =>
        {
            clickedCount++;
            Api.Logger.Log($"Clicked {clickedCount} times");
        });
    }
}