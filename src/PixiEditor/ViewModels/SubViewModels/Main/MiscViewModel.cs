using PixiEditor.Helpers;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Dialogs;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.ViewModels.SubViewModels.Main;

[Command.Group("PixiEditor.Links", "Misc")]
internal class MiscViewModel : SubViewModel<ViewModelMain>
{
    public MiscViewModel(ViewModelMain owner)
        : base(owner)
    {
    }

    [Command.Internal("PixiEditor.Links.OpenHyperlink")]
    [Command.Basic("PixiEditor.Links.OpenDocumentation", "https://pixieditor.net/docs/introduction", "Documentation", "Open Documentation", IconPath = "Globe.png")]
    [Command.Basic("PixiEditor.Links.OpenWebsite", "https://pixieditor.net", "Website", "Open Website", IconPath = "Globe.png")]
    [Command.Basic("PixiEditor.Links.OpenRepository", "https://github.com/PixiEditor/PixiEditor", "Repository", "Open Repository", IconPath = "Globe.png")]
    [Command.Basic("PixiEditor.Links.OpenLicense", "https://github.com/PixiEditor/PixiEditor/blob/master/LICENSE", "License", "Open License", IconPath = "Globe.png")]
    [Command.Basic("PixiEditor.Links.OpenOtherLicenses", "https://pixieditor.net/docs/Third-party-licenses", "Third Party Licenses", "Open Third Party Licenses", IconPath = "Globe.png")]
    public static void OpenHyperlink(string url)
    {
        try
        {
            ProcessHelpers.ShellExecute(url);
        }
        catch
        {
            NoticeDialog.Show(title: "Error", message: $"Couldn't open the address {url} in your default browser");
        }
    }
}
