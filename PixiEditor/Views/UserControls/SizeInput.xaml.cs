using PixiEditor.Models.Enums;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PixiEditor.Views
{
    /// <summary>
    ///     Interaction logic for SizeInput.xaml.
    /// </summary>
    public partial class SizeInput : UserControl
    {
        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register(nameof(Size), typeof(int), typeof(SizeInput), new PropertyMetadata(1, InputSizeChanged));

        public static readonly DependencyProperty MaxSizeProperty =
            DependencyProperty.Register(nameof(MaxSize), typeof(int), typeof(SizeInput), new PropertyMetadata(int.MaxValue));

        public static readonly DependencyProperty BehaveLikeSmallEmbeddedFieldProperty =
            DependencyProperty.Register(nameof(BehaveLikeSmallEmbeddedField), typeof(bool), typeof(SizeInput), new PropertyMetadata(true));

        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register(nameof(Unit), typeof(SizeUnit), typeof(SizeInput), new PropertyMetadata(SizeUnit.Pixel));

        public SizeInput()
        {
            InitializeComponent();
        }

        private void SizeInput_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            textBox.Focus();
        }

        public int Size
        {
            get => (int)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        public int MaxSize
        {
            get => (int)GetValue(MaxSizeProperty);
            set => SetValue(MaxSizeProperty, value);
        }

        public bool BehaveLikeSmallEmbeddedField
        {
            get => (bool)GetValue(BehaveLikeSmallEmbeddedFieldProperty);
            set => SetValue(BehaveLikeSmallEmbeddedFieldProperty, value);
        }
                
        public void FocusAndSelect()
        {
            textBox.Focus();
            textBox.SelectAll();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!textBox.IsFocused)
                textBox.Focus();
            Point pos = Mouse.GetPosition(textBox);
            int charIndex = textBox.GetCharacterIndexFromPoint(pos, true);
            var charRect = textBox.GetRectFromCharacterIndex(charIndex);
            double middleX = (charRect.Left + charRect.Right) / 2;
            if (pos.X > middleX)
                textBox.CaretIndex = charIndex + 1;
            else
                textBox.CaretIndex = charIndex;
            e.Handled = true;
        }

        public SizeUnit Unit
        {
          get => (SizeUnit)GetValue(UnitProperty);
          set => SetValue(UnitProperty, value);
        }

        private static void InputSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            int newValue = (int)e.NewValue;
            int maxSize = (int)d.GetValue(MaxSizeProperty);

            if (newValue > maxSize)
            {
                d.SetValue(SizeProperty, maxSize);

                return;
            }
            else if (newValue <= 0)
            {
                d.SetValue(SizeProperty, 1);

                return;
            }
        }

        private void Border_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int step = e.Delta / 100;

            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                Size += step * 2;
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (step < 0)
                {
                    Size /= 2;
                }
                else
                {
                    Size *= 2;
                }
            }
            else
            {
                Size += step;
            }
        }
    }
}
