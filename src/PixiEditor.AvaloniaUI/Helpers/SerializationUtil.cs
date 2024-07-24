using ChunkyImageLib;
using PixiEditor.DrawingApi.Core;
using PixiEditor.Numerics;
using PixiEditor.Parser;
using PixiEditor.Parser.Skia;

namespace PixiEditor.AvaloniaUI.Helpers;

public static class SerializationUtil
{
    public static object SerializeObject(object? value, ImageEncoder encoder)
    {
        if (value is null)
        {
            return null;
        }

        if (value is Surface surface)
        {
            byte[] result = encoder.Encode(surface.ToByteArray(), surface.Size.X, surface.Size.Y);
            IImageContainer container =
                new ImageContainer { ImageBytes = result, ResourceOffset = 0, ResourceSize = result.Length };
            return container;
        }

        if (value.GetType().IsValueType || value is string)
        {
            return value;
        }

        if (value is ISerializable serializable)
        {
            return serializable.Serialize();
        }

        throw new ArgumentException($"Type {value.GetType()} is not serializable and does not implement ISerializable");
    }
}
