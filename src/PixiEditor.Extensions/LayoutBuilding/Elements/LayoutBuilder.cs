using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;
using PixiEditor.Extensions.Helpers;
using PixiEditor.Extensions.LayoutBuilding.Elements.Exceptions;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public class LayoutBuilder
{
    private static int int32Size = sizeof(int);

    public Dictionary<int, ILayoutElement<Control>> ManagedElements = new();
    private ElementMap elementMap;
    public LayoutBuilder(ElementMap elementMap)
    {
        this.elementMap = elementMap;
    }

    public ILayoutElement<Control> Deserialize(Span<byte> layoutSpan, DuplicateResolutionTactic duplicatedIdTactic)
    {
        int offset = 0;

        int uniqueId = BitConverter.ToInt32(layoutSpan[offset..(offset + int32Size)]);
        offset += int32Size;

        int controlTypeIdLength = BitConverter.ToInt32(layoutSpan[offset..(offset + int32Size)]);
        offset += int32Size;

        string controlTypeId = Encoding.UTF8.GetString(layoutSpan[offset..(offset + controlTypeIdLength)]);
        offset += controlTypeIdLength;

        int propertiesCount = BitConverter.ToInt32(layoutSpan[offset..(offset + int32Size)]);
        offset += int32Size;

        List<object> properties = DeserializeProperties(layoutSpan, propertiesCount, ref offset);

        int childrenCount = BitConverter.ToInt32(layoutSpan[offset..(offset + int32Size)]);
        offset += int32Size;

        List<ILayoutElement<Control>> children = DeserializeChildren(layoutSpan, childrenCount, ref offset, duplicatedIdTactic);

        return BuildLayoutElement(uniqueId, controlTypeId, properties, children, duplicatedIdTactic);
    }

    private static List<object> DeserializeProperties(Span<byte> layoutSpan, int propertiesCount, ref int offset)
    {
        var properties = new List<object>();
        for (int i = 0; i < propertiesCount; i++)
        {
            int propertyType = layoutSpan[offset];
            offset++;

            Type type = ByteMap.GetTypeFromByteId((byte)propertyType);
            if (type == typeof(string))
            {
                int stringLength = BitConverter.ToInt32(layoutSpan[offset..(offset + int32Size)]);
                offset += int32Size;
                string value = Encoding.UTF8.GetString(layoutSpan[offset..(offset + stringLength)]);
                offset += stringLength;
                properties.Add(value);
            }
            else
            {
                var property = SpanUtility.Read(type, layoutSpan, ref offset);
                properties.Add(property);
            }
        }

        return properties;
    }

    private List<ILayoutElement<Control>> DeserializeChildren(Span<byte> layoutSpan, int childrenCount, ref int offset, DuplicateResolutionTactic duplicatedIdTactic)
    {
        var children = new List<ILayoutElement<Control>>();
        for (int i = 0; i < childrenCount; i++)
        {
            children.Add(Deserialize(layoutSpan[offset..], duplicatedIdTactic));
        }

        return children;
    }

    private ILayoutElement<Control> BuildLayoutElement(int uniqueId, string controlId, List<object> properties,
        List<ILayoutElement<Control>> children, DuplicateResolutionTactic duplicatedIdTactic)
    {
        Type typeToSpawn = elementMap.ControlMap[controlId];
        var element = CreateInstance(typeToSpawn);
        
        if(element is not { } layoutElement)
            throw new Exception("Element is not ILayoutElement<Control>");

        element.UniqueId = uniqueId;

        if (element is IPropertyDeserializable deserializableProperties)
        {
            deserializableProperties.DeserializeProperties(properties);
        }

        if (element is IChildHost customChildrenDeserializable)
        {
            customChildrenDeserializable.DeserializeChildren(children);
        }

        if (!ManagedElements.TryAdd(uniqueId, layoutElement))
        {
            if (duplicatedIdTactic == DuplicateResolutionTactic.ThrowException)
            {
                throw new DuplicateIdElementException(uniqueId);
            }

            if (duplicatedIdTactic == DuplicateResolutionTactic.ReplaceRemoveChildren)
            {
                if (ManagedElements[uniqueId] is IChildHost childrenDeserializable)
                {
                    RemoveChildren(childrenDeserializable);
                }

                ManagedElements[uniqueId] = layoutElement;
            }
        }

        return layoutElement;
    }

    private ILayoutElement<Control> CreateInstance(Type typeToSpawn)
    {
        var constructor = typeToSpawn.GetConstructor(Type.EmptyTypes);
        if (constructor != null)
        {
            return (ILayoutElement<Control>)Activator.CreateInstance(typeToSpawn);
        }

        var constructorWithParams = typeToSpawn.GetConstructors()[0];
        var parameters = constructorWithParams.GetParameters();
        var parameterValues = parameters.Select(x => x.DefaultValue).ToArray();
        return (ILayoutElement<Control>)Activator.CreateInstance(typeToSpawn, parameterValues);
    }

    private void RemoveChildren(IChildHost childHost)
    {
        foreach (var child in childHost)
        {
            ManagedElements.Remove(child.UniqueId);
            if (child is IChildHost childChildrenDeserializable)
            {
                RemoveChildren(childChildrenDeserializable);
            }
        }
    }
}

public enum DuplicateResolutionTactic
{
    ThrowException,
    ReplaceRemoveChildren,
}
