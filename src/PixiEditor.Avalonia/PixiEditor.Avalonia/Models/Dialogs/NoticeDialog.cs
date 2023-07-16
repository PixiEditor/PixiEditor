using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Localization;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.Models.Dialogs;

internal static class NoticeDialog
{
    /// <summary>
    ///     Shows a notice dialog with specified message and title.
    /// </summary>
    /// <param name="message">Localized string key for message.</param>
    /// <param name="title">Localized string key for title.</param>
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
