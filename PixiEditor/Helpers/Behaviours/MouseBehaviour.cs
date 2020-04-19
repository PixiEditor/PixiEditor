using PixiEditor.Views;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.Zoombox;

namespace PixiEditor.Helpers.Behaviours {

    public class MouseBehaviour : System.Windows.Interactivity.Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty MouseYProperty = DependencyProperty.Register(
            "MouseY", typeof(double), typeof(MouseBehaviour), new PropertyMetadata(default(double)));

        public double MouseY
        {
            get { return (double)GetValue(MouseYProperty); }
            set { SetValue(MouseYProperty, value); }
        }

        public static readonly DependencyProperty MouseXProperty = DependencyProperty.Register(
            "MouseX", typeof(double), typeof(MouseBehaviour), new PropertyMetadata(default(double)));

        public double MouseX
        {
            get { return (double)GetValue(MouseXProperty); }
            set { SetValue(MouseXProperty, value); }
        }



        public FrameworkElement RelativeTo
        {
            get { return (FrameworkElement)GetValue(RelativeToProperty); }
            set { SetValue(RelativeToProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RelativeTo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RelativeToProperty =
            DependencyProperty.Register("RelativeTo", typeof(FrameworkElement), typeof(MouseBehaviour), new PropertyMetadata(default(FrameworkElement)));



        protected override void OnAttached()
        {
            AssociatedObject.MouseMove += AssociatedObjectOnMouseMove;
        }

        private void AssociatedObjectOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            if(RelativeTo == null)
            {
                RelativeTo = AssociatedObject;
            }
            var pos = mouseEventArgs.GetPosition(RelativeTo);
            MouseX = pos.X;
            MouseY = pos.Y;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseMove -= AssociatedObjectOnMouseMove;
        }
    }
}