using System.Collections.ObjectModel;
using System.ComponentModel;
using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.ViewModels.Nodes.Properties;

internal class KernelPropertyViewModel : NodePropertyViewModel<Kernel?>
{
    public ObservableCollection<List<KernelVmReference>> ReferenceCollections { get; }
    
    public KernelPropertyViewModel(NodeViewModel node, Type valueType) : base(node, valueType)
    {
        ReferenceCollections = new ObservableCollection<List<KernelVmReference>>();
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Value) || Value == null)
            return;

        ReferenceCollections.Clear();
        
        for (int y = -Value.RadiusY; y <= Value.RadiusY; y++)
        {
            var collection = new List<KernelVmReference>();
            
            for (int x = -Value.RadiusX; x <= Value.RadiusX; x++)
            {
                collection.Add(new KernelVmReference(this, x, y));
            }

            ReferenceCollections.Add(collection);
        }
    }

    public class KernelVmReference(KernelPropertyViewModel viewModel, int x, int y) : PixiObservableObject
    {
        public float Value
        {
            get => viewModel.Value[x, y];
            set
            {
                viewModel.Value[x, y] = value;
                ViewModelMain.Current.NodeGraphManager.UpdatePropertyValue((viewModel.Node, viewModel.PropertyName, viewModel.Value));
                OnPropertyChanged();
            }
        }
    }
}
