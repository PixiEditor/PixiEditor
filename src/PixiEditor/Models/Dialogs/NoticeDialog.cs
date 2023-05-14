using PixiEditor.Models.Localization;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.Models.Dialogs;

internal static class NoticeDialog
{
    public static void Show(LocalizedString message, LocalizedString title)
    {
        NoticePopup popup = new()
        {
            Body = message,
            Title = title
        };

        popup.ShowDialog();
    }
}
