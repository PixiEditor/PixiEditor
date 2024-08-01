using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Numerics;

namespace PixiEditor.ViewModels.Nodes.Properties;

internal class KernelPropertyViewModel : NodePropertyViewModel<Kernel?>
{
    public ObservableCollection<KernelVmReference> ReferenceCollections { get; }
    
    public RelayCommand<int> AdjustSizeCommand { get; }
    
    public KernelPropertyViewModel(NodeViewModel node, Type valueType) : base(node, valueType)
    {
        ReferenceCollections = new ObservableCollection<KernelVmReference>();
        PropertyChanged += OnPropertyChanged;
        AdjustSizeCommand = new RelayCommand<int>(Execute, i => i > 0 && Width < 9 || i < 0 && Width > 3);
    }

    private void Execute(int by)
    {
        Value.Resize(Width + by * 2, Height + by * 2);
        OnPropertyChanged(nameof(Value));
        AdjustSizeCommand.NotifyCanExecuteChanged();
    }

    public int Width => Value.Width;
    
    public int Height => Value.Height;

    public float Sum => Value.Sum;

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(Value) || Value == null)
            return;

        ReferenceCollections.Clear();
        
        for (int y = -Value.RadiusY; y <= Value.RadiusY; y++)
        {
            for (int x = -Value.RadiusX; x <= Value.RadiusX; x++)
            {
                ReferenceCollections.Add(new KernelVmReference(this, x, y));
            }
        }
        
        OnPropertyChanged(nameof(Width));
        OnPropertyChanged(nameof(Height));
        OnPropertyChanged(nameof(Sum));
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
                viewModel.OnPropertyChanged(nameof(Sum));
                OnPropertyChanged();
            }
        }
    }
}
