using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using PixiEditor.AvaloniaUI.Models.DocumentModels;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.ViewModels.Nodes.Properties;

namespace PixiEditor.AvaloniaUI.ViewModels.Nodes;

internal abstract class NodePropertyViewModel : ViewModelBase, INodePropertyHandler
{
    private string propertyName;
    private string displayName;
    private object value;
    private INodeHandler node;
    private bool isInput;
    private bool isFunc;
    private IBrush socketBrush;
    
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

    public bool IsFunc
    {
        get => isFunc;
        set => SetProperty(ref isFunc, value);
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
    
    public IBrush SocketBrush
    {
        get => socketBrush;
        set => SetProperty(ref socketBrush, value);
    }
    
    public Type PropertyType { get; }

    public NodePropertyViewModel(INodeHandler node, Type propertyType)
    {
        Node = node;
        PropertyType = propertyType;
        var targetType = propertyType;

        if (propertyType.IsAssignableTo(typeof(Delegate)))
        {
            targetType = propertyType.GetMethod("Invoke").ReturnType;
        }

        if (Application.Current.Styles.TryGetResource($"{targetType.Name}SocketBrush", App.Current.ActualThemeVariant, out object brush))
        {
            if (brush is IBrush brushValue)
            {
                SocketBrush = brushValue;
            }
        }
        
        if(SocketBrush == null)
        {
            if(Application.Current.Styles.TryGetResource($"DefaultSocketBrush", App.Current.ActualThemeVariant, out object defaultBrush))
            {
                if (defaultBrush is IBrush defaultBrushValue)
                {
                    SocketBrush = defaultBrushValue;
                }
            }
        }
    }

    public static NodePropertyViewModel? CreateFromType(Type type, INodeHandler node)
    {
        Type propertyType = type;
        
        if (type.IsAssignableTo(typeof(Delegate)))
        {
            propertyType = type.GetMethod("Invoke").ReturnType;
        }
        
        string name = $"{propertyType.Name}PropertyViewModel";
        
        Type viewModelType = Type.GetType($"PixiEditor.AvaloniaUI.ViewModels.Nodes.Properties.{name}");
        if (viewModelType == null)
        {
            return new GenericPropertyViewModel(node, type);
        }
        
        return (NodePropertyViewModel)Activator.CreateInstance(viewModelType, node, type);
    }
}

internal abstract class NodePropertyViewModel<T> : NodePropertyViewModel
{
    private T nodeValue;
    
    public new T Value
    {
        get => nodeValue;
        set => SetProperty(ref nodeValue, value);
    }
    
    public NodePropertyViewModel(NodeViewModel node, Type valueType) : base(node, valueType)
    {
    }
}
