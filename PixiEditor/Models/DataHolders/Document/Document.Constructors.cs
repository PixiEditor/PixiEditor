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
            XamlAccesibleViewModel = ViewModelMain.Current;
            GeneratePreviewLayer();
            Layers.CollectionChanged += Layers_CollectionChanged;
            LayerStructure.Groups.CollectionChanged += Groups_CollectionChanged;
            LayerStructure.LayerStructureChanged += LayerStructure_LayerStructureChanged;
        }

        private void LayerStructure_LayerStructureChanged(object sender, System.EventArgs e)
        {
            RaisePropertyChanged(nameof(LayerStructure));
        }

        private void Groups_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != e.NewItems)
            {
                RaisePropertyChanged(nameof(LayerStructure));
            }
        }

        private void Layers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(Layers));
        }
    }
}