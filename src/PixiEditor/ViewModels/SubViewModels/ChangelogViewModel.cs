using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands.Attributes.Commands;

namespace PixiEditor.ViewModels.SubViewModels;

internal class ChangelogViewModel : SubViewModel<ViewModelMain>
{
    public ChangelogViewModel(ViewModelMain owner) : base(owner)
    {
        owner.OnUserReady += OwnerOnOnUserReady;
    }

    [Command.Basic("PixiEditor.Misc.OpenReleaseNotes", "OPEN_RELEASE_NOTES", "OPEN_RELEASE_NOTES_DESC",
        MenuItemPath = "HELP/OPEN_RELEASE_NOTES", MenuItemOrder = 5, AnalyticsTrack = true)]
    public void OpenReleaseNotes()
    {
        ShowChangelogTab();
    }

    private void OwnerOnOnUserReady()
    {
        string lastSeen =
            IPreferences.Current.GetLocalPreference<string>(PreferencesConstants.LastSeenChangelogVersion);

        if(Version.TryParse(lastSeen, out Version lastSeenVersion))
        {
            if (lastSeenVersion < CurrentVersion())
            {
                ShowChangelog();
            }
        }
        else
        {
            ShowChangelog();
        }
    }

    public void ShowChangelogTab()
    {
        string changelog = ResourceLoader.ReadEmbeddedFile("changelog.md");
        string changelogVersion =
            changelog.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).FirstOrDefault();

        changelogVersion = changelogVersion?.TrimStart('#', ' ');

        Owner.LayoutSubViewModel.LayoutManager.AddViewport(new ChangelogDockViewModel(changelogVersion?.Trim(), changelog));
    }

    private void ShowChangelog()
    {
        try
        {
            string changelog = ResourceLoader.ReadEmbeddedFile("changelog.md");
            string changelogVersion =
                changelog.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).FirstOrDefault();

            changelogVersion = changelogVersion?.TrimStart('#', ' ');

            if (changelogVersion == null)
                return;

            if (changelogVersion != null && Version.TryParse(changelogVersion.Trim(), out Version changelogVer))
            {
                if (changelogVer > CurrentVersion())
                {
                    return;
                }
            }

            IPreferences.Current.UpdateLocalPreference(PreferencesConstants.LastSeenChangelogVersion, CurrentVersion().ToString());
            Owner.LayoutSubViewModel.LayoutManager.AddViewport(new ChangelogDockViewModel(changelogVersion.Trim(), changelog));
        }
        catch (Exception ex)
        {
            CrashHelper.SendExceptionInfo(ex);
        }
    }

    private Version CurrentVersion()
    {
        return typeof(ViewModelMain).Assembly.GetName().Version;
    }
}
