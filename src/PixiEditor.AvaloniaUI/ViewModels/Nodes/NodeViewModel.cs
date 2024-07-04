using System.Collections.ObjectModel;
using Avalonia;
using ChunkyImageLib;

namespace PixiEditor.AvaloniaUI.ViewModels.Nodes;

public class NodeViewModel : ViewModelBase
{
    private string name;
    private double x;
    private double y;
    private ObservableCollection<NodePropertyViewModel> inputs;
    private ObservableCollection<NodePropertyViewModel> outputs;
    private Surface resultPreview;
    
    public string Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }
    
    public double X
    {
        get => x;
        set => SetProperty(ref x, value);
    }
    
    public double Y
    {
        get => y;
        set => SetProperty(ref y, value);
    }
    
    public ObservableCollection<NodePropertyViewModel> Inputs
    {
        get => inputs;
        set => SetProperty(ref inputs, value);
    }
    
    public ObservableCollection<NodePropertyViewModel> Outputs
    {
        get => outputs;
        set => SetProperty(ref outputs, value);
    }
    
    public Surface ResultPreview
    {
        get => resultPreview;
        set => SetProperty(ref resultPreview, value);
    }
}
