using System.Windows;
using System.Windows.Controls;
using AvalonDock.Layout;
using PixiEditor.Models.DataHolders;
using PixiEditor.ViewModels;

namespace PixiEditor.Helpers.UI
{
    public class DocumentsTemplateSelector : DataTemplateSelector
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
}