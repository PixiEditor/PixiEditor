using System.Text;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

public class CompiledControl
{
    public string ControlTypeId { get; set; }
    public List<(object value, Type type)> Properties { get; set; } = new();
    public List<CompiledControl> Children { get; set; } = new();
    public int UniqueId { get; set; }
    internal List<string> QueuedEvents => _buildQueuedEvents;

    private List<string> _buildQueuedEvents = new List<string>();
    public CompiledControl(int uniqueId, string controlTypeId)
    {
        ControlTypeId = controlTypeId;
        UniqueId = uniqueId;
    }

    public void AddProperty<T>(T value) where T : unmanaged
    {
        Properties.Add((value, typeof(T)));
    }

    public void AddProperty(string value, Type type)
    {
        Properties.Add((value, type));
    }

    public void AddChild(CompiledControl child)
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
        foreach (CompiledControl child in Children)
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
                null => Array.Empty<byte>(),
                _ => throw new Exception($"Unknown unmanaged type: {property.value.GetType()}")
            });
        }

        return result;
    }

    internal void AddEvent(string eventName)
    {
        _buildQueuedEvents.Add(eventName);
    }
}
