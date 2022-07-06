using System.Windows;
using System.Windows.Controls;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Helpers.UI;

internal class DocumentsTemplateSelector : DataTemplateSelector
{
    public DocumentsTemplateSelector()
    {

    }

    public DataTemplate DocumentsViewTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is DocumentViewModel)
        {
            return DocumentsViewTemplate;
        }

        return base.SelectTemplate(item, container);
    }
}
