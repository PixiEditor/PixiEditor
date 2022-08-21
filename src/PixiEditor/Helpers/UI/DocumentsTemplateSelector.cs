using System.Windows;
using System.Windows.Controls;

namespace PixiEditor.Helpers.UI;

internal class DocumentsTemplateSelector : DataTemplateSelector
{
    public DocumentsTemplateSelector()
    {

    }

    public DataTemplate DocumentsViewTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is ViewportWindowViewModel)
        {
            return DocumentsViewTemplate;
        }

        return base.SelectTemplate(item, container);
    }
}
