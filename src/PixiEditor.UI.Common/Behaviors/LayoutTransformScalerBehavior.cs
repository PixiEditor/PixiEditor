using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Xaml.Interactivity;

namespace PixiEditor.UI.Common.Behaviors;

public class LayoutTransformScalerBehavior : Behavior<LayoutTransformControl>
{
    public static double GlobalScaling { get; private set; } = 1.0;
    public static event Action<double> GlobalScalingChanged;

    override protected void OnAttached()
    {
        base.OnAttached();
        GlobalScalingChanged += OnGlobalScalingChanged;
        AssociatedObject.LayoutTransform = new ScaleTransform(GlobalScaling, GlobalScaling);
    }

    private void OnGlobalScalingChanged(double newScale)
    {
        if (AssociatedObject != null)
        {
            AssociatedObject.LayoutTransform = new ScaleTransform(newScale, newScale);
        }
    }

    public static void SetGlobalScaling(double newValue)
    {
        GlobalScalingChanged?.Invoke(newValue);
        GlobalScaling = newValue;
    }
}
