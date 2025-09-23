using System.Collections;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class InputProperties : IReadOnlyInputProperties
{
    public Dictionary<string, InputProperty> Properties { get; } = new();

    public void Add(InputProperty property)
    {
        if (!Properties.TryAdd(property.InternalPropertyName, property))
            throw new ArgumentException($"Property with name {property.InternalPropertyName} already exists.");
    }

    public void Add<T>(InputProperty<T> property)
    {
        if (!Properties.TryAdd(property.InternalPropertyName, property))
            throw new ArgumentException($"Property with name {property.InternalPropertyName} already exists.");
    }

    public bool Remove(InputProperty property)
    {
        return Properties.Remove(property.InternalPropertyName);
    }

    public IEnumerator<IInputProperty> GetEnumerator()
    {
        return Properties.Values.GetEnumerator();
    }

    public int Count => Properties.Count;

    public InputProperty? TryGetProperty(string internalName)
    {
        Properties.TryGetValue(internalName, out var prop);
        return prop;
    }

    IInputProperty IReadOnlyInputProperties.TryGetProperty(string internalName)
    {
        return TryGetProperty(internalName);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IInputProperty TryGetPropertyc { get; set; }
}

public interface IReadOnlyInputProperties : IEnumerable<IInputProperty>
{
    int Count { get; }
    public IInputProperty TryGetProperty(string internalName);
}

