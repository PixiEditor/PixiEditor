using PixiEditor.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PixiEditorDotNetCore3.Models.Dialogs
{
    public abstract class CustomDialog : NotifyableObject
    {
        public abstract bool ShowDialog();
    }
}
