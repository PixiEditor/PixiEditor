using PixiEditor.Models.Enums;
using PixiEditor.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace PixiEditor.Models.Dialogs
{
    public static class ConfirmationDialog
    {
        public static ConfirmationType Show(string message)
        {
            ConfirmationPopup popup = new ConfirmationPopup()
            {
                Body = message
            };
            if ((bool)popup.ShowDialog())
            {
                return popup.Result ? ConfirmationType.Yes : ConfirmationType.No;
            }
            return ConfirmationType.Canceled;
        }
    }
}
