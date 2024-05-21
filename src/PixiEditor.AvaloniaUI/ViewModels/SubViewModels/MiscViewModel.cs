using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Models.Commands.Attributes.Commands;
using PixiEditor.AvaloniaUI.Models.Dialogs;
using PixiEditor.OperatingSystem;

namespace PixiEditor.AvaloniaUI.ViewModels.SubViewModels;

[Command.Group("PixiEditor.Links", "MISC")]
internal class MiscViewModel : SubViewModel<ViewModelMain>
{
    public MiscViewModel(ViewModelMain owner)
        : base(owner)
    {
    }

    [Command.Internal("PixiEditor.Links.OpenHyperlink")]
    [Command.Basic("PixiEditor.Links.OpenDocumentation", "https://pixieditor.net/docs/introduction", "DOCUMENTATION", "OPEN_DOCUMENTATION", IconPath = "Globe.png",
        MenuItemPath = "HELP/DOCUMENTATION", MenuItemOrder = 0)]
    [Command.Basic("PixiEditor.Links.OpenWebsite", "https://pixieditor.net", "WEBSITE", "OPEN_WEBSITE", IconPath = "Globe.png",
        MenuItemPath = "HELP/WEBSITE", MenuItemOrder = 1)]
    [Command.Basic("PixiEditor.Links.OpenRepository", "https://github.com/PixiEditor/PixiEditor", "REPOSITORY", "OPEN_REPOSITORY", IconPath = "Globe.png",
        MenuItemPath = "HELP/REPOSITORY", MenuItemOrder = 2)]
    [Command.Basic("PixiEditor.Links.OpenLicense", "LICENSE", "LICENSE", "OPEN_LICENSE", IconPath = "Globe.png",
        MenuItemPath = "HELP/LICENSE", MenuItemOrder = 3)]
    [Command.Basic("PixiEditor.Links.OpenOtherLicenses", "THIRD_PARTY_LICENSES.txt", "THIRD_PARTY_LICENSES", "OPEN_THIRD_PARTY_LICENSES", IconPath = "Globe.png",
        MenuItemPath = "HELP/THIRD_PARTY_LICENSES", MenuItemOrder = 4)]
    public static void OpenUri(string uri)
    {
        try
        {
            IOperatingSystem.Current.OpenUri(uri);
        }
        catch (Exception e)
        {
            CrashHelper.SendExceptionInfoToWebhook(e);
            NoticeDialog.Show(title: "Error", message: $"Couldn't open the address {uri} in your default browser");
        }
    }
}
