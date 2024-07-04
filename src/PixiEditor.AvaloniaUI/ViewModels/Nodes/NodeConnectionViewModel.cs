namespace PixiEditor.AvaloniaUI.ViewModels.Nodes;

public class NodeConnectionViewModel : ViewModelBase
{
    private NodeViewModel inputNode;
    private NodeViewModel outputNode;
    private NodePropertyViewModel inputProperty;
    private NodePropertyViewModel outputProperty;

    public NodeViewModel InputNode
    {
        get => inputNode;
        set => SetProperty(ref inputNode, value);
    }

    public NodeViewModel OutputNode
    {
        get => outputNode;
        set => SetProperty(ref outputNode, value);
    }

    public NodePropertyViewModel InputProperty
    {
        get => inputProperty;
        set => SetProperty(ref inputProperty, value);
    }

    public NodePropertyViewModel OutputProperty
    {
        get => outputProperty;
        set => SetProperty(ref outputProperty, value);
    }
}
