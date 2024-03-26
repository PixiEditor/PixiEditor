using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.AvaloniaUI.Views.Dialogs;

public partial class AboutPopup : PixiEditorPopup
{
    public static LocalizedString VersionText =>
        new LocalizedString("VERSION", VersionHelpers.GetCurrentAssemblyVersionString(true));

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

