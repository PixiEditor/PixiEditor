using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PixiEditor.Models.DataHolders;

namespace PixiEditor.Helpers.UI
{
    public class PanelsStyleSelector : StyleSelector
    {
        public Style DocumentTabStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is Document)
            {
                return DocumentTabStyle;
            }
            return base.SelectStyle(item, container);
        }
    }
}