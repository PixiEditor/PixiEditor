using PixiEditor.Views.Dialogs;

namespace PixiEditor.Models.Dialogs;

internal static class NoticeDialog
{
    public static void Show(string message, string title)
    {
        NoticePopup popup = new()
        {
            Body = message,
            Title = title
        };

        popup.ShowDialog();
    }
}
