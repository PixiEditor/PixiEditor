using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class SyncedTypeInputProperty
{
    private InputProperty internalInputProperty;
    public InputProperty InternalProperty => internalInputProperty;
    public SyncedTypeInputProperty Other { get; set; }

    public event Action ConnectionChanged;
    public event Action BeforeTypeChange;
    public event Action AfterTypeChange;

    public SyncedTypeInputProperty(Node node, string internalPropertyName, string displayName,
        SyncedTypeInputProperty other)
    {
        Other = other;
        internalInputProperty = new InputProperty(node, internalPropertyName, displayName, null, typeof(object));
    }

    internal void BeginListeningToConnectionChanges()
    {
        internalInputProperty.ConnectionChanged += UpdateType;
    }

    private void UpdateType()
    {
        IOutputProperty? target = null;
        if (Other.InternalProperty.Connection != null && internalInputProperty.Connection == null)
        {
            target = Other.InternalProperty.Connection;
        }
        else if (Other.InternalProperty.Connection == null && internalInputProperty.Connection != null)
        {
            target = internalInputProperty.Connection;
        }
        else if (Other.InternalProperty.Connection != null && internalInputProperty.Connection != null)
        {
            target = Other.InternalProperty.Connection;
        }

        Type newType = target?.ValueType ?? typeof(object);
        if (internalInputProperty.ValueType != newType)
        {
            BeforeTypeChange?.Invoke();
            internalInputProperty.ConnectionChanged -= UpdateType;
            var connection = internalInputProperty.Connection;
            internalInputProperty.Connection?.DisconnectFrom(internalInputProperty);
            internalInputProperty.Connection = null;
            internalInputProperty = new InputProperty(internalInputProperty.Node,
                internalInputProperty.InternalPropertyName, internalInputProperty.DisplayName,
                internalInputProperty.NonOverridenValue, newType);
            connection?.ConnectTo(internalInputProperty);
            internalInputProperty.ConnectionChanged += UpdateType;
            AfterTypeChange();
            Other?.UpdateType();
        }
    }
}
