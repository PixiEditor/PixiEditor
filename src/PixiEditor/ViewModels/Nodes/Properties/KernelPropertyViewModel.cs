using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drawie.Numerics;

namespace PixiEditor.ViewModels.Nodes.Properties;

internal class KernelPropertyViewModel : NodePropertyViewModel<Kernel?>
{
    public ObservableCollection<KernelVmReference> ReferenceCollections { get; }

    public RelayCommand<int> AdjustSizeCommand { get; }

    private bool blockUpdates = false;

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
        blockUpdates = true;

        int requiredCount = Value.Height * Value.Width;

        if (ReferenceCollections.Count != requiredCount)
        {
            ReferenceCollections.Clear();
            for (int y = -Value.RadiusY; y <= Value.RadiusY; y++)
            {
                for (int x = -Value.RadiusX; x <= Value.RadiusX; x++)
                {
                    if (ReferenceCollections.Count < requiredCount)
                    {
                        ReferenceCollections.Add(new KernelVmReference(this, x, y));
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        OnPropertyChanged(nameof(Width));
        OnPropertyChanged(nameof(Height));
        OnPropertyChanged(nameof(Sum));
        OnPropertyChanged(nameof(ReferenceCollections));

        for (int i = 0; i < ReferenceCollections.Count; i++)
        {
            var reference = ReferenceCollections[i];
            reference.ValueChanged();
        }

        blockUpdates = false;
    }

    public class KernelVmReference(KernelPropertyViewModel viewModel, int x, int y) : PixiObservableObject
    {
        public bool MergeChanges
        {
            get
            {
                return viewModel.MergeChanges;
            }
            set
            {
                viewModel.MergeChanges = value;
                OnPropertyChanged();
            }
        }

        public float Value
        {
            get => viewModel.Value[x, y];
            set
            {
                if (viewModel.blockUpdates)
                    return;

                var newVal = viewModel.Value.Clone() as Kernel;
                newVal[x, y] = value;
                if (MergeChanges)
                {
                    ViewModelMain.Current.NodeGraphManager.BeginUpdatePropertyValue((viewModel.Node,
                        viewModel.PropertyName, newVal));
                }
                else
                {
                    ViewModelMain.Current.NodeGraphManager.UpdatePropertyValue((viewModel.Node, viewModel.PropertyName,
                        newVal));
                }

                viewModel.OnPropertyChanged(nameof(Sum));
                OnPropertyChanged();
            }
        }

        public void ValueChanged()
        {
            OnPropertyChanged(nameof(Value));
        }
    }
}
