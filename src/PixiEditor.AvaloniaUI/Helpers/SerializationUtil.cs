using ChunkyImageLib;
using PixiEditor.AvaloniaUI.Models.Serialization;
using PixiEditor.AvaloniaUI.Models.Serialization.Factories;
using PixiEditor.DrawingApi.Core;
using PixiEditor.Numerics;
using PixiEditor.Parser;
using PixiEditor.Parser.Skia;

namespace PixiEditor.AvaloniaUI.Helpers;

public static class SerializationUtil
{
    public static object SerializeObject(object? value, SerializationConfig config, IReadOnlyList<SerializationFactory> allFactories)
    {
        if (value is null)
        {
            return null;
        }

        var factory = allFactories.FirstOrDefault(x => x.OriginalType == value.GetType());
        
        if (factory != null)
        {
            return factory.Serialize(value);
        }
        
        if (value.GetType().IsValueType || value is string)
        {
            return value;
        }

        throw new ArgumentException($"Type {value.GetType()} is not serializable and appropriate serialization factory was not found.");
    }
}
