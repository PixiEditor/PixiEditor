using System.Windows.Media;

namespace PixiEditor.Views.UserControls
{
    public class SwitchItem
    {
        public string Content { get; set; } = "";
        public Brush Background { get; set; }
        public object Value { get; set; }

        public BitmapScalingMode ScalingMode { get; set; } = BitmapScalingMode.HighQuality;

        public SwitchItem(Brush background, object value, string content, BitmapScalingMode scalingMode = BitmapScalingMode.HighQuality)
        {
            Background = background;
            Value = value;
            ScalingMode = scalingMode;
            Content = content;
        }
        public SwitchItem()
        {
        }
    }
}
