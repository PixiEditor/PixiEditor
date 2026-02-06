using MessagePack;
using PixiEditor.Helpers;
using PixiEditor.Parser;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Models.Serialization.Factories;

public abstract class SerializationFactory
{
    public abstract Type OriginalType { get; }
    public abstract string DeserializationId { get; } 
    public SerializationConfig Config { get; set; }
    public ResourceStorage Storage { get; set; }
    public ResourceStorageLocator? ResourceLocator { get; set; }

    public abstract object Serialize(object original);
    public abstract object Deserialize(object rawData, (string serializerName, string serializerVersion) serializerData);
    
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

    protected bool IsFilePreVersion((string serializerName, string serializerVersion) serializerData, Version minSupportedVersion)
    {
        return SerializationUtil.IsFilePreVersion(serializerData, minSupportedVersion);
    }
}

public abstract class SerializationFactory<TSerializable, TOriginal> : SerializationFactory
{
    public abstract TSerializable Serialize(TOriginal original);
    public abstract bool TryDeserialize(object serialized, out TOriginal original,
        (string serializerName, string serializerVersion) serializerData);
    
    public override object Serialize(object original)
    {
        SerializedObject serialized = new SerializedObject
        {
            SerializationId = DeserializationId,
            Data = Serialize((TOriginal)original)
        };

        return serialized;
    }
    
    public override object Deserialize(object rawData, (string serializerName, string serializerVersion) serializerData)
    {
        return TryDeserialize(rawData, out TOriginal original, serializerData) ? original : default;
    }

    protected string DeserializeStringCompatible(
        ByteExtractor extractor,
        (string serializerName, string serializerVersion) serializerData)
    {
        if (serializerData.serializerName != "PixiEditor")
        {
            return extractor.GetString();
        }

        if (IsFilePreVersion(serializerData, new Version(2, 0, 0, 87)))
        {
            return extractor.GetStringLegacyDontUse();
        }

        return extractor.GetString();
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
