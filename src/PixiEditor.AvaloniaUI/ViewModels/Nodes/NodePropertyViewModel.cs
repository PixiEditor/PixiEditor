namespace PixiEditor.AvaloniaUI.ViewModels.Nodes;

public abstract class NodePropertyViewModel : ViewModelBase
{
    private string name;
    private object value;
    private NodeViewModel node;
    
    public string Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }
    
    public object Value
    {
        get => value;
        set => SetProperty(ref value, value);
    }
    
    public NodeViewModel Node
    {
        get => node;
        set => SetProperty(ref node, value);
    }
    
    public NodePropertyViewModel(NodeViewModel node)
    {
        Node = node;
    }
}

public abstract class NodePropertyViewModel<T> : NodePropertyViewModel
{
    private T nodeValue;
    
    public new T Value
    {
        get => nodeValue;
        set => SetProperty(ref nodeValue, value);
    }
    
    public NodePropertyViewModel(NodeViewModel node) : base(node)
    {
    }
}
