﻿using System.Collections.ObjectModel;

namespace PixiEditor.AvaloniaUI.Models.Handlers;

public interface INodePropertyHandler
{
    public string PropertyName { get; set; }
    public string DisplayName { get; set; }
    public object Value { get; set; }
    public bool IsInput { get; }
    public INodePropertyHandler? ConnectedOutput { get; set; }
    public ObservableCollection<INodePropertyHandler> ConnectedInputs { get; }
    public INodeHandler Node { get; set; }
}