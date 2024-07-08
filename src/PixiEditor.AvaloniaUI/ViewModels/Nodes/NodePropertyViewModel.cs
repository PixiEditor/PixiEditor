using System.Collections.ObjectModel;
using Avalonia;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.ViewModels.Nodes.Properties;

namespace PixiEditor.AvaloniaUI.ViewModels.Nodes;

public abstract class NodePropertyViewModel : ViewModelBase, INodePropertyHandler
{
    private string propertyName;
    private string displayName;
    private object value;
    private INodeHandler node;
    private bool isInput;
    
    private ObservableCollection<INodePropertyHandler> connectedInputs = new();
    private INodePropertyHandler? connectedOutput;
    
    public string DisplayName
    {
        get => displayName;
        set => SetProperty(ref displayName, value);
    }
    
    public object Value
    {
        get => value;
        set => SetProperty(ref value, value);
    }
    
    public bool IsInput
    {
        get => isInput;
        set => SetProperty(ref isInput, value);
    }

    public INodePropertyHandler? ConnectedOutput
    {
        get => connectedOutput;
        set => SetProperty(ref connectedOutput, value);
    }

    public ObservableCollection<INodePropertyHandler> ConnectedInputs
    {
        get => connectedInputs;
        set => SetProperty(ref connectedInputs, value);
    }

    public INodeHandler Node
    {
        get => node;
        set => SetProperty(ref node, value);
    }

    public string PropertyName
    {
        get => propertyName;
        set => SetProperty(ref propertyName, value);
    }

    public NodePropertyViewModel(INodeHandler node)
    {
        Node = node;
    }

    public static NodePropertyViewModel? CreateFromType(Type type, INodeHandler node)
    {
        string name = type.Name;
        name += "PropertyViewModel";
        
        Type viewModelType = Type.GetType($"PixiEditor.AvaloniaUI.ViewModels.Nodes.Properties.{name}");
        if (viewModelType == null)
        {
            return new GenericPropertyViewModel(node);
        }
        
        return (NodePropertyViewModel)Activator.CreateInstance(viewModelType, node);
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
