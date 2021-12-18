using PixiEditor.Models.Enums;
using PixiEditor.Views;
using System;

namespace PixiEditor.Models.Dialogs
{
    public static class ConfirmationDialog
    {
        [Obsolete(message: "Use Show(message, title) instead.")]
        public static ConfirmationType Show(string message)
        {
            ConfirmationPopup popup = new ConfirmationPopup
            {
                Body = message,
                Topmost = true
            };
            if (popup.ShowDialog().GetValueOrDefault())
            {
                return popup.Result ? ConfirmationType.Yes : ConfirmationType.No;
            }

            return ConfirmationType.Canceled;
        }

        public static ConfirmationType Show(string message, string title)
        {
            ConfirmationPopup popup = new ConfirmationPopup
            {
                Title = title,
                Body = message,
                ShowInTaskbar = false
            };
            if (popup.ShowDialog().GetValueOrDefault())
            {
                return popup.Result ? ConfirmationType.Yes : ConfirmationType.No;
            }

            return ConfirmationType.Canceled;
        }
    }
}