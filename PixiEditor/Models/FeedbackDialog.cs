using PixiEditor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Models
{
    public class FeedbackMenuDialog : CustomDialog
    {
        public override bool ShowDialog()
        {
            FeedbackDialog dialog = new FeedbackDialog();
            dialog.ShowDialog();
            return (bool)dialog.DialogResult;
        }
    }
}
