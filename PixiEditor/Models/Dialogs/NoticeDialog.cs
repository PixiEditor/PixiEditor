using PixiEditor.Views.Dialogs;

namespace PixiEditor.Models.Dialogs
{
    public static class NoticeDialog
    {
        public static void Show(string message)
        {
            NoticePopup popup = new NoticePopup
            {
                Body = message
            };

            popup.ShowDialog();
        }
    }
}