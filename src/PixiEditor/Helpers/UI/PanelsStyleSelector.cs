using System.Windows;
using System.Windows.Controls;

namespace PixiEditor.Helpers.UI;

internal class PanelsStyleSelector : StyleSelector
{
    public Style DocumentTabStyle { get; set; }

    public override Style SelectStyle(object item, DependencyObject container)
    {
        if (item is ViewportWindowViewModel)
        {
            return DocumentTabStyle;
        }
        return base.SelectStyle(item, container);
    }
}
