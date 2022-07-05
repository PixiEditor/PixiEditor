using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace PixiEditor.Helpers.Behaviours
{
    internal class MouseBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty MouseYProperty = DependencyProperty.Register(nameof(MouseY), typeof(double), typeof(MouseBehavior), new PropertyMetadata(default(double)));

        public static readonly DependencyProperty MouseXProperty = DependencyProperty.Register(nameof(MouseX), typeof(double), typeof(MouseBehavior), new PropertyMetadata(default(double)));


        public static readonly DependencyProperty RelativeToProperty =
            DependencyProperty.Register(nameof(RelativeTo),
                typeof(FrameworkElement),
                typeof(MouseBehavior),
                new PropertyMetadata(default(FrameworkElement)));

        public double MouseY
        {
            get => (double)GetValue(MouseYProperty);
            set => SetValue(MouseYProperty, value);
        }

        public double MouseX
        {
            get => (double)GetValue(MouseXProperty);
            set => SetValue(MouseXProperty, value);
        }

        public FrameworkElement RelativeTo
        {
            get => (FrameworkElement)GetValue(RelativeToProperty);
            set => SetValue(RelativeToProperty, value);
        }

        protected override void OnAttached()
        {
            AssociatedObject.MouseMove += AssociatedObjectOnMouseMove;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseMove -= AssociatedObjectOnMouseMove;
        }

        private void AssociatedObjectOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            if (RelativeTo == null)
            {
                RelativeTo = AssociatedObject;
            }

            Point pos = mouseEventArgs.GetPosition(RelativeTo);
            MouseX = pos.X;
            MouseY = pos.Y;
        }
    }
}

#if PUBLISH
#error Hi
#endif
