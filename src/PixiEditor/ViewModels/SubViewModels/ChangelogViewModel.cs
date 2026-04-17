using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Helpers;

namespace PixiEditor.ViewModels.SubViewModels;

internal class ChangelogViewModel : SubViewModel<ViewModelMain>
{
    public ChangelogViewModel(ViewModelMain owner) : base(owner)
    {
        owner.OnUserReady += OwnerOnOnUserReady;
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
