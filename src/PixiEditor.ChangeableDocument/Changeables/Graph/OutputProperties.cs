using System.Collections;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class OutputProperties : IReadOnlyOutputProperties
{
    public Dictionary<string, OutputProperty> Properties { get; } = new();

    public void Add(OutputProperty property)
    {
        if (!Properties.TryAdd(property.InternalPropertyName, property))
            throw new ArgumentException(
                $"Property with name {property.InternalPropertyName} already exists in this collection.");
    }

    public void Add<T>(OutputProperty<T> property)
    {
        if (!Properties.TryAdd(property.InternalPropertyName, property))
            throw new ArgumentException(
                $"Property with name {property.InternalPropertyName} already exists in this collection.");
    }

    public IEnumerator<IOutputProperty> GetEnumerator()
    {
        return Properties.Values.GetEnumerator();
    }

    public OutputProperty? TryGetProperty(string propertyName)
    {
        Properties.TryGetValue(propertyName, out var prop);
        return prop;
    }

    public int Count => Properties.Count;

    IOutputProperty? IReadOnlyOutputProperties.TryGetProperty(string propertyName)
    {
        return TryGetProperty(propertyName);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public interface IReadOnlyOutputProperties : IEnumerable<IOutputProperty>
{
    public IOutputProperty? TryGetProperty(string propertyName);
    int Count { get; }
}
