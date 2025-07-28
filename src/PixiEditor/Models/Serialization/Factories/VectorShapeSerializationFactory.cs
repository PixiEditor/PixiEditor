using System.Reflection;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Models.Serialization.Factories.Paintables;

namespace PixiEditor.Models.Serialization.Factories;

public abstract class VectorShapeSerializationFactory<T> : SerializationFactory<byte[], T> where T : ShapeVectorData
{
    private static List<SerializationFactory>? paintableFactories = null;
    private static List<SerializationFactory> PaintableFactories => paintableFactories ??= GatherPaintableFactories();

    private static List<SerializationFactory> GatherPaintableFactories()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(IPaintableSerializationFactory));
        Type[] types = assembly.GetTypes();
        List<SerializationFactory> factories = new();

        foreach (Type type in types)
        {
            if (type.IsAssignableTo(typeof(IPaintableSerializationFactory)) &&
                type is { IsAbstract: false, IsInterface: false })
            {
                factories.Add((SerializationFactory)Activator.CreateInstance(type));
            }
        }

        return factories;
    }

    public override byte[] Serialize(T original)
    {
        ByteBuilder builder = new ByteBuilder();
        builder.AddMatrix3X3(original.TransformationMatrix);
        AddPaintable(original.Stroke, builder);
        builder.AddBool(original.Fill);
        AddPaintable(original.FillPaintable, builder);
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

        bool fileIsPrePaintables = serializerData.serializerName == "PixiEditor"
                                   && Version.TryParse(serializerData.serializerVersion, out Version version)
                                   && version is { Major: 2, Minor: 0, Build: 0, Revision: < 62 };

        ByteExtractor extractor = new ByteExtractor(data);

        Matrix3X3 matrix = extractor.GetMatrix3X3();
        Paintable strokeColor = TryGetPaintable(extractor, fileIsPrePaintables, serializerData);
        bool fill = TryGetBool(extractor, serializerData);
        Paintable fillColor = TryGetPaintable(extractor, fileIsPrePaintables, serializerData);
        float strokeWidth;
        // Previous versions of the serializer saved stroke as int, and serializer data didn't exist
        if (string.IsNullOrEmpty(serializerData.serializerVersion) &&
            string.IsNullOrEmpty(serializerData.serializerName))
        {
            strokeWidth = extractor.GetInt();
        }
        else
        {
            strokeWidth = extractor.GetFloat();
        }

        return DeserializeVectorData(extractor, matrix, strokeColor, fill, fillColor, strokeWidth, serializerData,
            out original);
    }

    protected abstract bool DeserializeVectorData(ByteExtractor extractor, Matrix3X3 matrix, Paintable strokePaintable,
        bool fill, Paintable fillPaintable, float strokeWidth,
        (string serializerName, string serializerVersion) serializerData,
        out T original);

    private bool TryGetBool(ByteExtractor extractor, (string serializerName, string serializerVersion) serializerData)
    {
        // Previous versions didn't have fill bool
        if (serializerData.serializerName == "PixiEditor")
        {
            if (Version.TryParse(serializerData.serializerVersion, out Version version) &&
                version is { Major: 2, Minor: 0, Build: 0, Revision: < 35 })
            {
                return true;
            }
        }

        return extractor.GetBool();
    }

    private Paintable TryGetPaintable(ByteExtractor extractor, bool fileIsPrePaintables,
        (string serializerName, string serializerVersion) serializerData)
    {
        if (fileIsPrePaintables)
        {
            return new ColorPaintable(extractor.GetColor());
        }

        string paintableType = DeserializeStringCompatible(extractor, serializerData);

        SerializationFactory factory = PaintableFactories.FirstOrDefault(f => f.DeserializationId == paintableType);
        if (factory == null)
        {
            throw new InvalidOperationException($"No factory found for paintable type {paintableType}");
        }

        factory.Config = Config;
        factory.ResourceLocator = ResourceLocator;

        return ((IPaintableSerializationFactory)factory).TryDeserialize(extractor);
    }

    private void AddPaintable(Paintable paintable, ByteBuilder builder)
    {
        SerializationFactory factory = PaintableFactories.FirstOrDefault(f => f.OriginalType == paintable.GetType());
        if (factory == null)
        {
            throw new InvalidOperationException($"No factory found for paintable type {paintable.GetType()}");
        }

        factory.Config = Config;
        factory.Storage = Storage;

        builder.AddString(factory.DeserializationId);
        ((IPaintableSerializationFactory)factory).Serialize(paintable, builder);
    }
}
