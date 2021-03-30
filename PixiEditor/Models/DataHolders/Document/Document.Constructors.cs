using PixiEditor.Models.Controllers;
using PixiEditor.ViewModels;

namespace PixiEditor.Models.DataHolders
{
    public partial class Document
    {
        public Document(int width, int height)
            : this()
        {
            Width = width;
            Height = height;
            DocumentSizeChanged?.Invoke(this, new DocumentSizeChangedEventArgs(0, 0, width, height));
        }

        private Document()
        {
            SetRelayCommands();
            UndoManager = new UndoManager();
            LayerStructure = new Layers.LayerStructure(this);
            XamlAccesibleViewModel = ViewModelMain.Current ?? null;
            GeneratePreviewLayer();
            Layers.CollectionChanged += Layers_CollectionChanged;
            LayerStructure.Groups.CollectionChanged += Folders_CollectionChanged1;
        }

        private void Folders_CollectionChanged1(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(LayerStructure));
        }

        private void Layers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(Layers));
        }
    }
}