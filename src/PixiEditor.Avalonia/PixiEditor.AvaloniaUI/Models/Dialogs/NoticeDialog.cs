using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using PixiEditor.AvaloniaUI.Views.Dialogs;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.AvaloniaUI.Models.Dialogs;

internal static class NoticeDialog
{
    /// <summary>
    ///     Shows a notice dialog with specified message and title.
    /// </summary>
    /// <param name="message">Localized string key for message.</param>
    /// <param name="title">Localized string key for title.</param>
    public static void Show(LocalizedString message, LocalizedString title)
    {
        if(Application.Current?.ApplicationLifetime is ClassicDesktopStyleApplicationLifetime lifetime)
        {
            NoticePopup popup = new()
            {
                Body = message,
                Title = title
            };

            if(lifetime.MainWindow is not null)
            {
                popup.ShowDialog(lifetime.MainWindow);
            }
        }
        else
        {
            throw new InvalidOperationException("NoticeDialog can only be shown in ClassicDesktopStyleApplicationLifetime");
        }
    }
}
