using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Controllers.Shortcuts;

namespace PixiEditor.Views
{
    /// <summary>
    ///     Interaction logic for EditableTextBlock.xaml.
    /// </summary>
    public partial class EditableTextBlock : UserControl
    {
        // Using a DependencyProperty as the backing store for TextBlockVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextBlockVisibilityProperty =
            DependencyProperty.Register(
                "TextBlockVisibility",
                typeof(Visibility),
                typeof(EditableTextBlock),
                new PropertyMetadata(Visibility.Visible));

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(EditableTextBlock),
                new PropertyMetadata(default(string)));

        // Using a DependencyProperty as the backing store for EnableEditing.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableEditingProperty =
            DependencyProperty.Register(
                "IsEditing",
                typeof(bool),
                typeof(EditableTextBlock),
                new PropertyMetadata(OnIsEditingChanged));

        public event EventHandler<TextChangedEventArgs> OnSubmit;

        public EditableTextBlock()
        {
            InitializeComponent();
        }

        public Visibility TextBlockVisibility
        {
            get => (Visibility)GetValue(TextBlockVisibilityProperty);
            set => SetValue(TextBlockVisibilityProperty, value);
        }

        public bool IsEditing
        {
            get => (bool)GetValue(EnableEditingProperty);
            set => SetValue(EnableEditingProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public void EnableEditing()
        {
            ShortcutController.BlockShortcutExection("EditableTextBlock");
            TextBlockVisibility = Visibility.Hidden;
            IsEditing = true;
            Dispatcher.BeginInvoke(
                DispatcherPriority.Input,
                new Action(delegate()
                {
                    textBox.Focus();         // Set Logical Focus
                    Keyboard.Focus(textBox); // Set Keyboard Focus
                }));
            textBox.SelectAll();
        }

        public void DisableEditing()
        {
            TextBlockVisibility = Visibility.Visible;
            ShortcutController.UnblockShortcutExecution("EditableTextBlock");
            IsEditing = false;
            OnSubmit?.Invoke(this, new TextChangedEventArgs(textBox.Text, Text));
        }

        private static void OnIsEditingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                EditableTextBlock tb = (EditableTextBlock)d;
                tb.EnableEditing();
            }
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                EnableEditing();
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DisableEditing();
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            DisableEditing();
        }

        private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            DisableEditing();
        }

        public class TextChangedEventArgs : EventArgs
        {
            public string NewText { get; set; }

            public string OldText { get; set; }

            public TextChangedEventArgs(string newText, string oldText)
            {
                NewText = newText;
                OldText = oldText;
            }
        }
    }
}