using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Properties;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public class ControlDefinition
{
    public string ControlTypeId { get; set; }
    public List<(object value, Type type)> Properties { get; set; } = new();
    public List<ControlDefinition> Children { get; set; } = new();
    public int UniqueId { get; set; }
    internal List<string> QueuedEvents => _buildQueuedEvents;

    private List<string> _buildQueuedEvents = new List<string>();

    public ControlDefinition(int uniqueId, string controlTypeId)
    {
        ControlTypeId = controlTypeId;
        UniqueId = uniqueId;
    }

    public void AddProperty<T>(T value)
    {
        InternalAddProperty(value);
    }

    private void InternalAddProperty(object value)
    {
        if (value is string s)
        {
            AddStringProperty(s);
        }
        else if (value is Enum enumProp)
        {
            var enumValue = Convert.ChangeType(value, enumProp.GetTypeCode());
            Properties.Add((enumValue, enumValue.GetType()));
        }
        else if (value is IStructProperty structProperty)
        {
            Properties.Add((value, typeof(byte[])));
        }
        else
        {
            Properties.Add((value, value.GetType()));
        }
    }

    private void AddStringProperty(string value)
    {
        Properties.Add((value, typeof(string)));
    }

    public void AddChild(ControlDefinition child)
    {
        Children.Add(child);
    }

    public Span<byte> Serialize()
    {
        return Serialize(new List<byte>()).ToArray();
    }

    // DO NOT REMOVE, used by reflection-based layout compiler, using Serialize with Span<byte> throws error.
    public byte[] SerializeBytes()
    {
        return Serialize(new List<byte>()).ToArray();
    }

    private List<byte> Serialize(List<byte> bytes)
    {
        // TODO: Make it more efficient

        byte[] uniqueIdBytes = BitConverter.GetBytes(UniqueId);
        bytes.AddRange(uniqueIdBytes);
        byte[] idLengthBytes = BitConverter.GetBytes(ControlTypeId.Length);
        bytes.AddRange(idLengthBytes);
        byte[] idBytes = Encoding.UTF8.GetBytes(ControlTypeId);
        bytes.AddRange(idBytes);
        bytes.AddRange(BitConverter.GetBytes(Properties.Count));
        bytes.AddRange(SerializeProperties());
        bytes.AddRange(BitConverter.GetBytes(Children.Count));
        SerializeChildren(bytes);

        return bytes;
    }

    private void SerializeChildren(List<byte> bytes)
    {
        foreach (ControlDefinition child in Children)
        {
            child.Serialize(bytes);
        }
    }

    private List<byte> SerializeProperties()
    {
        var result = new List<byte>();
        foreach (var property in Properties)
        {
            result.Add(ByteMap.GetTypeByteId(property.type));
            if (property.type == typeof(string))
            {
                result.AddRange(BitConverter.GetBytes(property.value is string s ? s.Length : 0));
            }

            result.AddRange(property.value switch
            {
                int i => BitConverter.GetBytes(i),
                float f => BitConverter.GetBytes(f),
                bool b => BitConverter.GetBytes(b),
                double d => BitConverter.GetBytes(d),
                long l => BitConverter.GetBytes(l),
                short s => BitConverter.GetBytes(s),
                byte b => new byte[] { b },
                char c => BitConverter.GetBytes(c),
                string s => Encoding.UTF8.GetBytes(s),
                IStructProperty structProperty => GetWellKnownStructBytes(structProperty),
                null => [],
                _ => throw new Exception($"Unknown unmanaged type: {property.value.GetType()}")
            });
        }

        return result;
    }

    private static List<byte> GetWellKnownStructBytes(IStructProperty structProperty)
    {
        List<byte> bytes = new List<byte>(BitConverter.GetBytes(structProperty.GetType().Name.Length));
        bytes.AddRange(Encoding.UTF8.GetBytes(structProperty.GetType().Name));

        byte[] structBytes = structProperty.Serialize();
        
        bytes.AddRange(BitConverter.GetBytes(structBytes.Length));
        bytes.AddRange(structBytes);
        
        return bytes;
    }

    internal void AddEvent(string eventName)
    {
        _buildQueuedEvents.Add(eventName);
    }
}
