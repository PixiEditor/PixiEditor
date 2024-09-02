using System.Collections.ObjectModel;
using PixiEditor.Models.Events;

namespace PixiEditor.Models.Handlers;

public interface INodePropertyHandler
{
    public string PropertyName { get; set; }
    public string DisplayName { get; set; }
    public object Value { get; set; }
    public bool IsInput { get; }
    public INodePropertyHandler? ConnectedOutput { get; set; }
    public ObservableCollection<INodePropertyHandler> ConnectedInputs { get; }

    public event NodePropertyValueChanged ValueChanged;
    public INodeHandler Node { get; set; }
}
