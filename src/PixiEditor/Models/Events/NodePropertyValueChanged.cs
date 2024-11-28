using PixiEditor.Models.Handlers;

namespace PixiEditor.Models.Events;

public delegate void NodePropertyValueChanged(INodePropertyHandler property, NodePropertyValueChangedArgs args);

public record NodePropertyValueChangedArgs(object? OldValue, object? NewValue);
