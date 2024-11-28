using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiEditor.Helpers.Extensions;

namespace PixiEditor.Helpers;

public static class ResourceLoader
{
    public static Stream LoadResourceStream(Uri uri)
    {
        return AssetLoader.Open(uri);
    }

    public static T GetResource<T>(string key)
    {
        if (Application.Current.Styles.TryGetResource(key, null, out object resource))
        {
            return (T)resource;
        }

        return default!;
    }

    public static T GetResource<T>(string key, ThemeVariant? themeVariant)
    {
        if (Application.Current.Styles.TryGetResource(key, themeVariant, out object resource))
        {
            return (T)resource;
        }

        return default!;
    }

    public static Paint? GetPaint(string key, PaintStyle style = PaintStyle.Fill, ThemeVariant? themeVariant = null)
    {
        if (Application.Current.Styles.TryGetResource(key, themeVariant, out object paint))
        {
            if (paint is SolidColorBrush solidColorBrush)
            {
                return new Paint() { Color = solidColorBrush.Color.ToColor(), Style = style, IsAntiAliased = true };
            }

            throw new InvalidOperationException("Invalid paint style");
        }

        return null;
    }
}
