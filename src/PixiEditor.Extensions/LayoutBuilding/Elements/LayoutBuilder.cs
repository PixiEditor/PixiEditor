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

    Dictionary<int, ILayoutElement<Control>> managedElements = new();
    public LayoutBuilder(Dictionary<int, ILayoutElement<Control>> managedElements)
    {
        this.managedElements = managedElements;
    }

    public ILayoutElement<Control> Deserialize(Span<byte> layoutSpan, DuplicateResolutionTactic duplicatedIdTactic)
    {
        int offset = 0;

        int uniqueId = BitConverter.ToInt32(layoutSpan[offset..(offset + int32Size)]);
        offset += int32Size;

        int controlTypeId = BitConverter.ToInt32(layoutSpan[offset..(offset + int32Size)]);
        offset += int32Size;

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

    private ILayoutElement<Control> BuildLayoutElement(int uniqueId, int controlId, List<object> properties,
        List<ILayoutElement<Control>> children, DuplicateResolutionTactic duplicatedIdTactic)
    {
        Func<ILayoutElement<Control>> factory = GlobalControlFactory.Map[controlId];
        var element = factory();
        
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

        if (!managedElements.TryAdd(uniqueId, layoutElement))
        {
            if (duplicatedIdTactic == DuplicateResolutionTactic.ThrowException)
            {
                throw new DuplicateIdElementException(uniqueId);
            }

            if (duplicatedIdTactic == DuplicateResolutionTactic.ReplaceRemoveChildren)
            {
                if (managedElements[uniqueId] is IChildHost childrenDeserializable)
                {
                    RemoveChildren(childrenDeserializable);
                }

                managedElements[uniqueId] = layoutElement;
            }
        }

        return layoutElement;
    }

    private void RemoveChildren(IChildHost childHost)
    {
        foreach (var child in childHost)
        {
            managedElements.Remove(child.UniqueId);
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
