namespace PixiEditor.AvaloniaUI.Models.Serialization.Factories;

public abstract class SerializationFactory
{
    public abstract Type OriginalType { get; }
    public abstract Type SerializedType { get; }
    public abstract object Serialize(object original);
    public abstract object Deserialize(object serialized);
}

public abstract class SerializationFactory<TSerializable, TOriginal> : SerializationFactory
{
    protected SerializationFactory(SerializationConfig config)
    {
        Config = config;
    }

    public SerializationConfig Config { get; }
    
    
    public abstract TSerializable Serialize(TOriginal original);
    public abstract TOriginal Deserialize(TSerializable serialized);
    
    public override object Serialize(object original)
    {
        return Serialize((TOriginal)original);
    }
    
    public override object Deserialize(object serialized)
    {
        return Deserialize((TSerializable)serialized);
    }
    
    public override Type OriginalType => typeof(TOriginal);
    public override Type SerializedType => typeof(TSerializable);
}
