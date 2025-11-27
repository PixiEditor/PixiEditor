using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Media;
using Drawie.Backend.Core.Shaders.Generation;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.Models.Events;
using PixiEditor.Models.Handlers;
using PixiEditor.ViewModels.Nodes.Properties;

namespace PixiEditor.ViewModels.Nodes;

internal abstract class NodePropertyViewModel : ViewModelBase, INodePropertyHandler
{
    private string propertyName;
    private bool isVisible = true;
    private string displayName;
    private object? _value;
    private INodeHandler node;
    private bool isInput;
    private bool isFunc;
    private IBrush socketBrush;
    private string errors = string.Empty;
    private bool mergeChanges = false;
    private bool socketEnabledEnabled = true;

    private object computedValue;

    private ObservableCollection<INodePropertyHandler> connectedInputs = new();
    private INodePropertyHandler? connectedOutput;

    public event NodePropertyValueChanged? ValueChanged;
    public event EventHandler? ConnectedOutputChanged;

    public string DisplayName
    {
        get => displayName;
        set => SetProperty(ref displayName, value);
    }

    public object? Value
    {
        get => _value;
        set
        {
            var oldValue = _value;
            if (value == null && oldValue == null) return;
            if (oldValue != null && oldValue.Equals(value)) return;

            if (MergeChanges)
            {
                ViewModelMain.Current.NodeGraphManager.BeginUpdatePropertyValue((node, PropertyName, value));
            }
            else
            {
                ViewModelMain.Current.NodeGraphManager.UpdatePropertyValue((node, PropertyName, value));
            }

            if (SetProperty(ref _value, value))
            {
                ValueChanged?.Invoke(this, new NodePropertyValueChangedArgs(oldValue, value));
            }
        }
    }

    public bool MergeChanges
    {
        get => mergeChanges;
        set
        {
            if (SetProperty(ref mergeChanges, value))
            {
                if (!value)
                {
                    ViewModelMain.Current.NodeGraphManager.EndUpdatePropertyValue();
                }
            }
        }
    }

    public object ComputedValue
    {
        get
        {
            return computedValue;
        }
        set
        {
            SetProperty(ref computedValue, value);
        }
    }

    public bool IsInput
    {
        get => isInput;
        set
        {
            if (SetProperty(ref isInput, value))
            {
                OnPropertyChanged(nameof(ShowInputField));
            }
        }
    }

    public bool IsFunc
    {
        get => isFunc;
        set => SetProperty(ref isFunc, value);
    }

    public bool IsVisible
    {
        get => isVisible;
        set => SetProperty(ref isVisible, value);
    }


    public INodePropertyHandler? ConnectedOutput
    {
        get => connectedOutput;
        set
        {
            if (SetProperty(ref connectedOutput, value))
            {
                OnPropertyChanged(nameof(ShowInputField));
                ConnectedOutputChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public bool ShowInputField
    {
        get => IsInput && ConnectedOutput == null;
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

    public bool SocketEnabled
    {
        get => socketEnabledEnabled;
        set => SetProperty(ref socketEnabledEnabled, value);
    }

    public string? Errors
    {
        get => errors;
        set => SetProperty(ref errors, value);
    }

    public NodePropertyViewModel(INodeHandler node, Type propertyType)
    {
        Node = node;
        PropertyType = propertyType;
        var targetType = propertyType;

        if (propertyType.IsAssignableTo(typeof(Delegate)))
        {
            targetType = propertyType.GetMethod("Invoke").ReturnType;
        }

        if (targetType.IsEnum)
        {
            targetType = typeof(Enum);
        }

        if (Application.Current.Styles.TryGetResource($"{targetType.Name}SocketBrush", App.Current.ActualThemeVariant,
                out object brush))
        {
            if (brush is IBrush brushValue)
            {
                SocketBrush = brushValue;
            }
        }

        if (SocketBrush == null)
        {
            if (Application.Current.Styles.TryGetResource($"DefaultSocketBrush", App.Current.ActualThemeVariant,
                    out object defaultBrush))
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

        if (IsShaderType(propertyType))
        {
            propertyType = type.GetMethod("Invoke").ReturnType.BaseType.GenericTypeArguments[0];
        }

        string typeName = propertyType.Name;

        string name = $"{typeName}PropertyViewModel";

        if (propertyType == typeof(IReadOnlyDocument))
        {
            name = "DocumentPropertyViewModel";
        }

        Type viewModelType = Type.GetType($"PixiEditor.ViewModels.Nodes.Properties.{name}");
        if (viewModelType == null)
        {
            if (propertyType.IsEnum)
            {
                return new GenericEnumPropertyViewModel(node, type, propertyType);
            }

            return new GenericPropertyViewModel(node, type);
        }

        return (NodePropertyViewModel)Activator.CreateInstance(viewModelType, node, type);
    }

    public void UpdateComputedValue()
    {
        ViewModelMain.Current.NodeGraphManager.GetComputedPropertyValue(this);
    }

    public void InternalSetComputedValue(object value)
    {
        computedValue = value;
        OnPropertyChanged(nameof(ComputedValue));
    }

    public void InternalSetValue(object? value)
    {
        var oldValue = _value;
        if (SetProperty(ref _value, value, nameof(Value)))
        {
            ValueChanged?.Invoke(this, new NodePropertyValueChangedArgs(oldValue, value));
        }
    }

    private static bool IsShaderType(Type type)
    {
        return type.IsAssignableTo(typeof(ShaderExpressionVariable));
    }
}

internal abstract class NodePropertyViewModel<T> : NodePropertyViewModel
{
    public new T Value
    {
        get
        {
            if (base.Value == null)
                return default;

            if (base.Value is T value)
                return value;

            return default;
        }
        set => base.Value = value;
    }

    public NodePropertyViewModel(NodeViewModel node, Type valueType) : base(node, valueType)
    {
    }
}
