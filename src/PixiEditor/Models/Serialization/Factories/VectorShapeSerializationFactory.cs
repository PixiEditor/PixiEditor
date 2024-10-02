using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.Models.Serialization.Factories;

public abstract class VectorShapeSerializationFactory<T> : SerializationFactory<byte[], T> where T : ShapeVectorData
{
    public override byte[] Serialize(T original)
    {
        ByteBuilder builder = new ByteBuilder();
        builder.AddMatrix3X3(original.TransformationMatrix);
        builder.AddColor(original.StrokeColor);
        builder.AddColor(original.FillColor);
        builder.AddInt(original.StrokeWidth);
        
        AddSpecificData(builder, original);
        
        return builder.Build();
    }
    
    protected abstract void AddSpecificData(ByteBuilder builder, T original);

    public override bool TryDeserialize(object serialized, out T original)
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
        int strokeWidth = extractor.GetInt();
        
        return DeserializeVectorData(extractor, matrix, strokeColor, fillColor, strokeWidth, out original);
    }
    
    protected abstract bool DeserializeVectorData(ByteExtractor extractor, Matrix3X3 matrix, Color strokeColor, Color fillColor, int strokeWidth, out T original);
}
