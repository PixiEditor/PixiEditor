using PixiEditor.Helpers;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Views.Dialogs;

public partial class AboutPopup : PixiEditorPopup
{
    public static LocalizedString VersionText =>
        new LocalizedString("VERSION", VersionHelpers.GetCurrentAssemblyVersionString(true));
    
    public static LocalizedString BuildIdText =>
        new LocalizedString("BUILD_ID", VersionHelpers.GetBuildId());

    public bool DisplayDonationButton
    {
#if STEAM
        get => false;
#else
        get => true;
#endif
    }
    public AboutPopup()
    {
        InitializeComponent();
    }
}

