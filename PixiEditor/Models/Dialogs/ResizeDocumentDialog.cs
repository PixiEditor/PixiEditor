using PixiEditor.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace PixiEditor.Models.Dialogs
{
    public class ResizeDocumentDialog : CustomDialog
    {
        public override bool ShowDialog()
        {
            ResizeDocumentPopup popup = new ResizeDocumentPopup();
            popup.ShowDialog();
            return (bool)popup.DialogResult;
        }
    }
}
