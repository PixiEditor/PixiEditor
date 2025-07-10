using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Properties;
using PixiEditor.Extensions.FlyUI.Exceptions;
using PixiEditor.Extensions.Helpers;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class LayoutBuilder
{
    private static int int32Size = sizeof(int);

    public Dictionary<int, ILayoutElement<Control>> ManagedElements = new();
    public ElementMap ElementMap { get; }
    public LayoutBuilder(ElementMap elementMap)
    {
        this.ElementMap = elementMap;
    }

    public ILayoutElement<Control> Deserialize(Span<byte> layoutSpan, DuplicateResolutionTactic duplicatedIdTactic)
    {
        int offset = 0;
        return DeserializeInternal(layoutSpan, duplicatedIdTactic, ref offset);
    }

    private ILayoutElement<Control> DeserializeInternal(Span<byte> layoutSpan, DuplicateResolutionTactic duplicatedIdTactic, ref int offset)
    {
        int uniqueId = BitConverter.ToInt32(layoutSpan[offset..(offset + int32Size)]);
        offset += int32Size;

        int controlTypeIdLength = BitConverter.ToInt32(layoutSpan[offset..(offset + int32Size)]);
        offset += int32Size;

        string controlTypeId = Encoding.UTF8.GetString(layoutSpan[offset..(offset + controlTypeIdLength)]);
        offset += controlTypeIdLength;

        int propertiesCount = BitConverter.ToInt32(layoutSpan[offset..(offset + int32Size)]);
        offset += int32Size;

        List<object> properties = DeserializeProperties(layoutSpan, propertiesCount, ref offset, ElementMap);

        int childrenCount = BitConverter.ToInt32(layoutSpan[offset..(offset + int32Size)]);
        offset += int32Size;

        List<ILayoutElement<Control>> children = DeserializeChildren(layoutSpan, childrenCount, ref offset, duplicatedIdTactic);

        return BuildLayoutElement(uniqueId, controlTypeId, properties, children, duplicatedIdTactic);
    }

    private static List<object> DeserializeProperties(Span<byte> layoutSpan, int propertiesCount, ref int offset, ElementMap map)
    {
        var properties = new List<object>();
        for (int i = 0; i < propertiesCount; i++)
        {
            int propertyType = layoutSpan[offset];
            offset++;

            Type type = ByteMap.GetTypeFromByteId((byte)propertyType);
            if (type == typeof(string))
            {
                int stringBytesLength = BitConverter.ToInt32(layoutSpan[offset..(offset + int32Size)]);
                offset += int32Size;
                string value = Encoding.UTF8.GetString(layoutSpan[offset..(offset + stringBytesLength)]);
                offset += stringBytesLength;
                properties.Add(value);
            }
            else if (type == typeof(byte[]))
            {
                int wellKnownStructNameLength = BitConverter.ToInt32(layoutSpan[offset..(offset + int32Size)]);
                offset += int32Size;
                
                string wellKnownStructName = Encoding.UTF8.GetString(layoutSpan[offset..(offset + wellKnownStructNameLength)]);
                offset += wellKnownStructNameLength;
                
                int structSize = BitConverter.ToInt32(layoutSpan[offset..(offset + int32Size)]);
                offset += int32Size;
                
                byte[] value = layoutSpan[offset..(offset + structSize)].ToArray();
                
                offset += structSize;
                
                map.WellKnownStructs.TryGetValue(wellKnownStructName, out Type? structType);
                if (structType == null)
                {
                    throw new Exception($"Struct type {wellKnownStructName} not found in map");
                }
                
                IStructProperty prop = (IStructProperty)Activator.CreateInstance(structType);
                prop.Deserialize(value);
                
                properties.Add(prop);
            }
            else if (type == null)
            {
                properties.Add(null);
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
            children.Add(DeserializeInternal(layoutSpan, duplicatedIdTactic, ref offset));
        }

        return children;
    }

    private ILayoutElement<Control> BuildLayoutElement(int uniqueId, string controlId, List<object> properties,
        List<ILayoutElement<Control>> children, DuplicateResolutionTactic duplicatedIdTactic)
    {
        Type typeToSpawn = ElementMap.ControlMap[controlId];
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
        var parameterValues = parameters.Select(x => x.HasDefaultValue ? x.DefaultValue : TryGetDefault(x)).ToArray(); 
        return (ILayoutElement<Control>)Activator.CreateInstance(typeToSpawn, parameterValues);
    }

    private static object? TryGetDefault(ParameterInfo x)
    {
        if (x.ParameterType == typeof(string))
        {
            return string.Empty;
        }
        
        return Activator.CreateInstance(x.ParameterType);
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
