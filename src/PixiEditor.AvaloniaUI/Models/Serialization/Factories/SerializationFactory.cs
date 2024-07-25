using MessagePack;

namespace PixiEditor.AvaloniaUI.Models.Serialization.Factories;

public abstract class SerializationFactory
{
    public abstract Type OriginalType { get; }
    public abstract string DeserializationId { get; } 
    public SerializationConfig Config { get; set; }

    public abstract object Serialize(object original);
    public abstract object Deserialize(object rawData);
    
    protected int ExtractInt(object value)
    {
        return value switch
        {
            byte b => b,
            int i => i,
            long l => (int)l,
            _ => throw new InvalidOperationException("Value is not an integer.")
        };
    }

    protected T[] ExtractArray<T>(object value)
    {
        return value switch
        {
            T[] arr => arr,
            object[] objArr => objArr.Cast<T>().ToArray(),
            _ => throw new InvalidOperationException("Value is not an array.")
        };
    }
}

public abstract class SerializationFactory<TSerializable, TOriginal> : SerializationFactory
{
    public abstract TSerializable Serialize(TOriginal original);
    public abstract bool TryDeserialize(object serialized, out TOriginal original);
    
    public override object Serialize(object original)
    {
        SerializedObject serialized = new SerializedObject
        {
            SerializationId = DeserializationId,
            Data = Serialize((TOriginal)original)
        };

        return serialized;
    }
    
    public override object Deserialize(object rawData)
    {
        return TryDeserialize(rawData, out TOriginal original) ? original : default;
    }
    
    public override Type OriginalType => typeof(TOriginal);
}

[MessagePackObject]
class SerializedObject
{
    [Key(0)]
    public string SerializationId { get; set; }
    
    [Key(1)]
    public object Data { get; set; }
}
