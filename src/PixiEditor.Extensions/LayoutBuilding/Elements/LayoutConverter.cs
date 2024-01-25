using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;
using PixiEditor.Extensions.Helpers;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public static class LayoutConverter
{
    private static int int32Size = sizeof(int);

    public static ILayoutElement<Control> Deserialize(Span<byte> layoutSpan)
    {
        int offset = 0;
        int controlId = BitConverter.ToInt32(layoutSpan[..int32Size]);
        offset += int32Size;

        int propertiesCount = BitConverter.ToInt32(layoutSpan[offset..(offset + int32Size)]);
        offset += int32Size;

        List<object> properties = DeserializeProperties(layoutSpan, propertiesCount, ref offset);

        int childrenCount = BitConverter.ToInt32(layoutSpan[offset..(offset + int32Size)]);
        offset += int32Size;

        List<ILayoutElement<Control>> children = DeserializeChildren(layoutSpan, childrenCount, ref offset);

        return BuildLayoutElement(controlId, properties, children);
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

    private static List<ILayoutElement<Control>> DeserializeChildren(Span<byte> layoutSpan, int childrenCount, ref int offset)
    {
        var children = new List<ILayoutElement<Control>>();
        for (int i = 0; i < childrenCount; i++)
        {
            children.Add(Deserialize(layoutSpan[offset..]));
        }

        return children;
    }

    private static ILayoutElement<Control> BuildLayoutElement(int controlId, List<object> properties, List<ILayoutElement<Control>> children)
    {
        Func<IDeserializable> factory = GlobalControlFactory.Map[controlId];
        var element = factory();
        
        if(element is not ILayoutElement<Control> layoutElement)
            throw new Exception("Element is not ILayoutElement<Control>");
        
        element.DeserializeProperties(properties);

        if (element is ISingleChildLayoutElement<Control> singleChildLayoutElement)
        {
            singleChildLayoutElement.Child = children[0];
        }
        else if (element is IMultiChildLayoutElement<Control> multiChildLayoutElement)
        {
            multiChildLayoutElement.Children = children;
        }
        
        return layoutElement;
    }
}
