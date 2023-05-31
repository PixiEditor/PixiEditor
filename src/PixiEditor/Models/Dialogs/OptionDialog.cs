using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Localization;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.Models.Dialogs;

internal static class OptionDialog
{
    public static OptionResult Show(LocalizedString message, LocalizedString title, LocalizedString option1Text, LocalizedString option2Text)
    {
        ConfirmationPopup popup = new ConfirmationPopup
        {
            Title = title,
            Body = message,
            ShowInTaskbar = false,
            FirstOptionText = option1Text,
            SecondOptionText = option2Text,
        };
        if (popup.ShowDialog().GetValueOrDefault())
        {
            return popup.Result ? OptionResult.Option1 : OptionResult.Option2;
        }

        return OptionResult.Canceled;
    }
}

public enum OptionResult
{
    Option1,
    Option2,
    Canceled
}
