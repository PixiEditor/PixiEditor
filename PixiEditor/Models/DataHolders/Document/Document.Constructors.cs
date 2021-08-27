using PixiEditor.Models.Controllers;
using PixiEditor.Models.Layers;
using PixiEditor.ViewModels;
using System.Linq;

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
            LayerStructure = new LayerStructure(this);
            XamlAccesibleViewModel = ViewModelMain.Current;
            GeneratePreviewLayer();
            Layers.CollectionChanged += Layers_CollectionChanged;
            LayerStructure.Groups.CollectionChanged += Groups_CollectionChanged;
            LayerStructure.LayerStructureChanged += LayerStructure_LayerStructureChanged;
            Renderer = new DocumentRenderer(layers, layerStructure, Width, Height);
        }

        private void LayerStructure_LayerStructureChanged(object sender, LayerStructureChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(LayerStructure));
            foreach (var layerGuid in e.AffectedLayerGuids)
            {
                Layer layer = Layers.First(x => x.LayerGuid == layerGuid);
                layer.RaisePropertyChange(nameof(layer.IsVisible));
                layer.RaisePropertyChange(nameof(layer.Opacity));
            }
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
