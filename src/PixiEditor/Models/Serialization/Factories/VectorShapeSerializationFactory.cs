using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;

namespace PixiEditor.Models.Serialization.Factories;

public abstract class VectorShapeSerializationFactory<T> : SerializationFactory<byte[], T> where T : ShapeVectorData
{
    public override byte[] Serialize(T original)
    {
        ByteBuilder builder = new ByteBuilder();
        builder.AddMatrix3X3(original.TransformationMatrix);
        builder.AddColor(original.StrokeColor);
        builder.AddColor(original.FillColor);
        builder.AddFloat(original.StrokeWidth);
        
        AddSpecificData(builder, original);
        
        return builder.Build();
    }
    
    protected abstract void AddSpecificData(ByteBuilder builder, T original);

    public override bool TryDeserialize(object serialized, out T original,
        (string serializerName, string serializerVersion) serializerData)
    {
        if (serialized is not byte[] data)
        {
            original = null;
            return false;
        }
        
        ByteExtractor extractor = new ByteExtractor(data);
        
        Matrix3X3 matrix = extractor.GetMatrix3X3();
        Color strokeColor = extractor.GetColor();
        Color fillColor = extractor.GetColor();
        float strokeWidth;
        // Previous versions of the serializer saved stroke as int, and serializer data didn't exist
        if (string.IsNullOrEmpty(serializerData.serializerVersion) && string.IsNullOrEmpty(serializerData.serializerName))
        {
            strokeWidth = extractor.GetInt();
        }
        else
        {
            strokeWidth = extractor.GetFloat();
        }

        return DeserializeVectorData(extractor, matrix, strokeColor, fillColor, strokeWidth, serializerData, out original);
    }
    
    protected abstract bool DeserializeVectorData(ByteExtractor extractor, Matrix3X3 matrix, Color strokeColor,
        Color fillColor, float strokeWidth, (string serializerName, string serializerVersion) serializerData,
        out T original);
}
