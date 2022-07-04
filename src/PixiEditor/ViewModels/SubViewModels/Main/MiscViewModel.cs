using PixiEditor.Helpers;
using PixiEditor.Models.Commands.Attributes;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    [Command.Group("PixiEditor.Links", "Misc")]
    public class MiscViewModel : SubViewModel<ViewModelMain>
    {
        public MiscViewModel(ViewModelMain owner)
            : base(owner)
        {
        }

        [Command.Basic("PixiEditor.Links.OpenDocumentation", "https://pixieditor.net/docs/introduction", "Documentation", "Open Documentation")]
        [Command.Basic("PixiEditor.Links.OpenWebsite", "https://pixieditor.net", "Website", "Open Website")]
        [Command.Basic("PixiEditor.Links.OpenRepository", "https://github.com/PixiEditor/PixiEditor", "Repository", "Open Repository")]
        [Command.Basic("PixiEditor.Links.OpenLicense", "https://github.com/PixiEditor/PixiEditor/blob/master/LICENSE", "License", "Open License")]
        [Command.Basic("PixiEditor.Links.OpenOtherLicenses", "https://pixieditor.net/docs/Third-party-licenses", "Third Party Licenses", "Open Third Party Licenses")]
        public static void OpenHyperlink(string url)
        {
            ProcessHelpers.ShellExecute(url);
        }
    }
}