using Drawie.Backend.Core.ColorsImpl.Paintables;

namespace PixiEditor.Models.Serialization.Factories.Paintables;

public interface IPaintableSerializationFactory
{
    public Paintable TryDeserialize(ByteExtractor extractor);
    public void Serialize(Paintable paintable, ByteBuilder builder);
}
