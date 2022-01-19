using PixiEditor.Views.Dialogs;

namespace PixiEditor.Models.Dialogs
{
    public static class NoticeDialog
    {
        public static void Show(string message)
        {
            NoticePopup popup = new()
            {
                Body = message,
                Title = string.Empty
            };

            popup.ShowDialog();
        }

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
}
