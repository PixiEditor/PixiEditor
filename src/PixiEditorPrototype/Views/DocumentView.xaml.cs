using PixiEditorPrototype.ViewModels;
using System.Windows.Controls;

namespace PixiEditorPrototype.Views
{
    /// <summary>
    /// Interaction logic for DocumentView.xaml
    /// </summary>
    internal partial class DocumentView : UserControl, IDocumentView
    {
        public DocumentView()
        {
            InitializeComponent();
            DataContextChanged += (_, e) =>
            {
                if (e.NewValue is not null)
                    ((DocumentViewModel)e.NewValue).View = this;
            };
        }

        public void ForceRefreshFinalImage()
        {
            mainImage.InvalidateVisual();
        }
    }
}
