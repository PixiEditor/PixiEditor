using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;
using System;

namespace PixiEditor.Helpers.Behaviours
{
    public class MouseBehaviour : Behavior<Control>
    {
        public static readonly StyledProperty<double> MouseYProperty = AvaloniaProperty.Register<MouseBehaviour, double>(
            nameof(MouseY), default);

        public static readonly StyledProperty<double> MouseXProperty = AvaloniaProperty.Register<MouseBehaviour, double>(
            nameof(MouseX), default);

        // Using a DependencyProperty as the backing store for RelativeTo.  This enables animation, styling, binding, etc...
        public static readonly StyledProperty<Control> RelativeToProperty =
            AvaloniaProperty.Register<MouseBehaviour, Control>(nameof(MouseY), default);

        public double MouseY
        {
            get => (double) GetValue(MouseYProperty);
            set => SetValue(MouseYProperty, value);
        }

        public double MouseX
        {
            get => (double) GetValue(MouseXProperty);
            set => SetValue(MouseXProperty, value);
        }


        public Control RelativeTo
        {
            get => (Control) GetValue(RelativeToProperty);
            set => SetValue(RelativeToProperty, value);
        }


        protected override void OnAttached()
        {
            AssociatedObject.PointerMoved += AssociatedObjectOnMouseMove;
        }

        private void AssociatedObjectOnMouseMove(object sender, PointerEventArgs e)
        {
            if (RelativeTo == null) RelativeTo = AssociatedObject;
            var pos = e.GetPosition(RelativeTo);
            MouseX = pos.X;
            MouseY = pos.Y;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PointerMoved -= AssociatedObjectOnMouseMove;
        }
    }
}