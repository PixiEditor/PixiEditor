using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Localization;
using PixiEditor.Views;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.Models.Dialogs;

internal static class ConfirmationDialog
{
    public static async Task<ConfirmationType> Show(LocalizedString message, LocalizedString title)
    {
        ConfirmationPopup popup = new ConfirmationPopup
        {
            Title = title,
            Body = message,
            ShowInTaskbar = false
        };

        if(Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (await popup.ShowDialog<bool>(desktop.MainWindow))
            {
                return popup.Result ? ConfirmationType.Yes : ConfirmationType.No;
            }
        }

        return ConfirmationType.Canceled;
    }
}
