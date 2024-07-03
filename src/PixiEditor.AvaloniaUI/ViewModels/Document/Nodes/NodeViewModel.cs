namespace PixiEditor.AvaloniaUI.ViewModels.Document.Nodes;

public class NodeViewModel : ViewModelBase
{
    private string name;
    private double x;
    private double y;
    
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
}
