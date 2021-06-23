using System.Windows.Media;

namespace PixiEditor.Views.UserControls
{
    public class SwitchItem
    {
        public Brush Background { get; set; }
        public object Value { get; set; }

        public BitmapScalingMode ScalingMode { get; set; } = BitmapScalingMode.HighQuality;

        public SwitchItem(Brush background, object value, BitmapScalingMode scalingMode = BitmapScalingMode.HighQuality)
        {
            Background = background;
            Value = value;
            ScalingMode = scalingMode;
        }
        public SwitchItem()
        {
        }
    }
}
