using System.Windows;
using System.Windows.Controls;
using PixiEditor.Models.DataHolders;

namespace PixiEditor.Helpers.UI;

internal class DocumentsTemplateSelector : DataTemplateSelector
{
    public DocumentsTemplateSelector()
    {

    }

    public DataTemplate DocumentsViewTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is Document)
        {
            return DocumentsViewTemplate;
        }

        return base.SelectTemplate(item, container);
    }
}
