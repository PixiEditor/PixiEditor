using System.ComponentModel;
using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Nodes;
internal class NodeConnectionViewModel : ViewModelBase
{
    private NodeViewModel inputNode;
    private NodeViewModel outputNode;
    private NodePropertyViewModel inputProperty;
    private NodePropertyViewModel outputProperty;

    public NodeViewModel InputNode
    {
        get => inputNode;
        set
        {
            if(InputNode != null)
                InputNode.PropertyChanged -= OnInputNodePropertyChanged;
            SetProperty(ref inputNode, value);
            if(InputNode != null)
                InputNode.PropertyChanged += OnInputNodePropertyChanged;
        }
    }

    public NodeViewModel OutputNode
    {
        get => outputNode;
        set
        {
            if(OutputNode != null)
                OutputNode.PropertyChanged -= OnOutputNodePropertyChanged;
            SetProperty(ref outputNode, value);
            if(OutputNode != null)
                OutputNode.PropertyChanged += OnOutputNodePropertyChanged;
        }
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

    public NodeConnectionViewModel()
    {
        
    }
    
    private void OnInputNodePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(INodeHandler.PositionBindable))
        {
            OnPropertyChanged(nameof(InputProperty));
        }
    }
    
    private void OnOutputNodePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(INodeHandler.PositionBindable))
        {
            OnPropertyChanged(nameof(OutputProperty));
        }
    }
}
