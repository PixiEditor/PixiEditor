using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class SyncedTypeOutputProperty
{
    private OutputProperty internalOutputProperty;
    public OutputProperty InternalProperty => internalOutputProperty;
    public SyncedTypeInputProperty Other { get; set; }

    public object Value
    {
        get => internalOutputProperty.Value;
        set => internalOutputProperty.Value = value;
    }

    public event Action BeforeTypeChange;
    public event Action AfterTypeChange;

    public SyncedTypeOutputProperty(Node node, string internalPropertyName, string displayName,
        SyncedTypeInputProperty other)
    {
        Other = other;
        internalOutputProperty = new OutputProperty(node, internalPropertyName, displayName, null, typeof(object));
        Other.AfterTypeChange += UpdateType;
    }

    private void UpdateType()
    {
        if (Other == null)
            return;

        Type newType = Other.InternalProperty?.ValueType ?? typeof(object);
        if (internalOutputProperty.ValueType != newType)
        {
            BeforeTypeChange?.Invoke();
            var connections = new List<IInputProperty>(internalOutputProperty.Connections);
            for (int i = internalOutputProperty.Connections.Count - 1; i >= 0; i--)
            {
                var connectionNode = internalOutputProperty.Connections.ElementAt(i);
                internalOutputProperty.DisconnectFrom(connectionNode);
            }

            internalOutputProperty = new OutputProperty(internalOutputProperty.Node,
                internalOutputProperty.InternalPropertyName, internalOutputProperty.DisplayName,
                newType.IsValueType ? Activator.CreateInstance(newType) : null, newType);

            foreach (var input in connections)
            {
                if (GraphUtils.IsLoop(input, internalOutputProperty) ||
                    !GraphUtils.CheckTypeCompatibility(input, internalOutputProperty))
                {
                    continue;
                }

                internalOutputProperty.ConnectTo(input);
            }

            AfterTypeChange();
        }
    }
}
