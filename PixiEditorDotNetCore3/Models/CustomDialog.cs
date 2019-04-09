using PixiEditor.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PixiEditor.Models
{
    public abstract class CustomDialog : NotifyableObject
    {
        public abstract bool ShowDialog();
    }
}
