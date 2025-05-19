using System.Collections;
using PixiEditor.Extensions.CommonApi.Utilities;

namespace PixiEditor.Extensions.CommonApi.FlyUI.Events;

public class ElementEventArgs
{
    public object Sender { get; set; }

    public static ElementEventArgs Deserialize(byte[] data)
    {
        if (data == null) return new ElementEventArgs();

        ByteReader reader = new ByteReader(data);
        string eventType = reader.ReadString();
        ElementEventArgs eventArgs = eventType switch // TODO: more generic implementation
        {
            nameof(ToggleEventArgs) => new ToggleEventArgs(reader.ReadBool()),
            nameof(TextEventArgs) => new TextEventArgs(reader.ReadString()),
            nameof(NumberEventArgs) => new NumberEventArgs(reader.ReadDouble()),
            nameof(ElementEventArgs) => new ElementEventArgs(),
            _ => throw new NotSupportedException($"Event type '{eventType}' is not supported.")
        };

        return eventArgs;
    }

    public byte[] Serialize()
    {
        ByteWriter writer = new ByteWriter();
        writer.WriteString(GetType().Name);
        SerializeArgs(writer);

        return writer.ToArray();
    }

    protected virtual void SerializeArgs(ByteWriter writer)
    {
        // Default implementation does nothing. Override in derived classes to serialize specific properties.
    }
}

public class ElementEventArgs<TEventArgs> : ElementEventArgs where TEventArgs : ElementEventArgs
{
}
