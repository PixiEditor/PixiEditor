using PixiEditor.Helpers;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Dialogs;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.ViewModels.SubViewModels.Main;

[Command.Group("PixiEditor.Links", "MISC")]
internal class MiscViewModel : SubViewModel<ViewModelMain>
{
    public MiscViewModel(ViewModelMain owner)
        : base(owner)
    {
    }

    [Command.Internal("PixiEditor.Links.OpenHyperlink")]
    [Command.Basic("PixiEditor.Links.OpenDocumentation", "https://pixieditor.net/docs/introduction", "DOCUMENTATION", "Open Documentation", IconPath = "Globe.png")]
    [Command.Basic("PixiEditor.Links.OpenWebsite", "https://pixieditor.net", "WEBSITE", "OPEN_WEBSITE", IconPath = "Globe.png")]
    [Command.Basic("PixiEditor.Links.OpenRepository", "https://github.com/PixiEditor/PixiEditor", "REPOSITORY", "OPEN_REPOSITORY", IconPath = "Globe.png")]
    [Command.Basic("PixiEditor.Links.OpenLicense", "https://github.com/PixiEditor/PixiEditor/blob/master/LICENSE", "LICENSE", "OPEN_LICENSE", IconPath = "Globe.png")]
    [Command.Basic("PixiEditor.Links.OpenOtherLicenses", "https://pixieditor.net/docs/Third-party-licenses", "THIRD_PARTY_LICENSES", "OPEN_THIRD_PARTY_LICENSES", IconPath = "Globe.png")]
    public static async Task OpenHyperlink(string url)
    {
        try
        {
            ProcessHelpers.ShellExecute(url);
        }
        catch (Exception e)
        {
            NoticeDialog.Show(title: "Error", message: $"Couldn't open the address {url} in your default browser");
            await CrashHelper.SendExceptionInfoToWebhook(e);
        }
    }
}
