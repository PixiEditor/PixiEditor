using System.Windows;

namespace PixiEditor.Helpers.UI
{
    public static class TreeViewItemHelper
    {
        public static GridLength GetIndent(DependencyObject obj)
        {
            return (GridLength)obj.GetValue(IndentProperty);
        }

        public static void SetIndent(DependencyObject obj, GridLength value)
        {
            obj.SetValue(IndentProperty, value);
        }


        public static readonly DependencyProperty IndentProperty =
            DependencyProperty.RegisterAttached("Indent", typeof(GridLength), typeof(TreeViewItemHelper), new PropertyMetadata(new GridLength(0)));
    }
}
