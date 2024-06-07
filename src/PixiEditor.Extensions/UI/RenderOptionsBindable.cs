using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace PixiEditor.Extensions.UI;

public class RenderOptionsBindable
{
    public static readonly AttachedProperty<BitmapInterpolationMode> BitmapInterpolationModeProperty =
        AvaloniaProperty.RegisterAttached<RenderOptionsBindable, Visual, BitmapInterpolationMode>("BitmapInterpolationMode");

    public static void SetBitmapInterpolationMode(Visual obj, BitmapInterpolationMode value)
    {
        obj.SetValue(BitmapInterpolationModeProperty, value);
        RenderOptions.SetBitmapInterpolationMode(obj, value);
    }

    public static BitmapInterpolationMode GetBitmapInterpolationMode(Visual obj) => obj.GetValue(BitmapInterpolationModeProperty);
}
