using System.Collections.ObjectModel;
using PixiEditor.Models.Events;

namespace PixiEditor.Models.Handlers;

public interface INodePropertyHandler
{
    public bool IsVisible { get; set; }
    public string PropertyName { get; set; }
    public string DisplayName { get; set; }
    public object Value { get; set; }
    public object ComputedValue { get; set; }
    public bool IsInput { get; }
    public INodePropertyHandler? ConnectedOutput { get; set; }
    public ObservableCollection<INodePropertyHandler> ConnectedInputs { get; }

    public event NodePropertyValueChanged ValueChanged;
    public event EventHandler ConnectedOutputChanged;
    public INodeHandler Node { get; set; }
    public Type PropertyType { get; }
    public bool SocketEnabled { get; set; }
    public void UpdateComputedValue();
    public void InternalSetComputedValue(object value);
    internal void InternalSetValue(object isVisible);
}
