using System.Diagnostics;
using System.Reflection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using PixiEditor.Helpers;
using PixiEditor.Initialization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Dialogs;
using PixiEditor.OperatingSystem;
using PixiEditor.UI.Common.Fonts;

namespace PixiEditor.ViewModels.SubViewModels;

[Command.Group("PixiEditor.Links", "MISC")]
internal class MiscViewModel : SubViewModel<ViewModelMain>
{
    public MiscViewModel(ViewModelMain owner)
        : base(owner)
    {
    }

    [Command.Internal("PixiEditor.Links.OpenHyperlink")]
    [Command.Basic("PixiEditor.Links.OpenDocumentation", "https://pixieditor.net/docs/introduction", "DOCUMENTATION",
        "OPEN_DOCUMENTATION", Icon = PixiPerfectIcons.Globe,
        MenuItemPath = "HELP/DOCUMENTATION", MenuItemOrder = 0, AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Links.OpenWebsite", "https://pixieditor.net", "WEBSITE", "OPEN_WEBSITE",
        Icon = PixiPerfectIcons.Globe,
        MenuItemPath = "HELP/WEBSITE", MenuItemOrder = 1, AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Links.OpenRepository", "https://github.com/PixiEditor/PixiEditor", "REPOSITORY",
        "OPEN_REPOSITORY", Icon = PixiPerfectIcons.Globe,
        MenuItemPath = "HELP/REPOSITORY", MenuItemOrder = 2, AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Links.OpenLicense", "{BaseDir}LICENSE", "LICENSE", "OPEN_LICENSE",
        Icon = PixiPerfectIcons.Folder,
        MenuItemPath = "HELP/LICENSE", MenuItemOrder = 3, AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Links.OpenOtherLicenses", "{BaseDir}/Third Party Licenses", "THIRD_PARTY_LICENSES",
        "OPEN_THIRD_PARTY_LICENSES", Icon = PixiPerfectIcons.Folder,
        MenuItemPath = "HELP/THIRD_PARTY_LICENSES", MenuItemOrder = 4, AnalyticsTrack = true)]
    public static void OpenUri(string uri)
    {
        try
        {
            if (uri.StartsWith("{BaseDir}"))
            {
                uri = uri.Replace("{BaseDir}", AppDomain.CurrentDomain.BaseDirectory);
            }

            IOperatingSystem.Current.OpenUri(uri);
        }
        catch (Exception e)
        {
            CrashHelper.SendExceptionInfo(e);
            NoticeDialog.Show(title: "Error", message: $"Couldn't open the address {uri} in your default browser");
        }
    }

    [Command.Internal("PixiEditor.Restart")]
    public static void Restart()
    {
        ClassicDesktopEntry.Active?.Restart();
    }
}
