using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PixiEditor.Views
{
    /// <summary>
    /// Interaction logic for EditableTextBlock.xaml
    /// </summary>
    public partial class EditableTextBlock : UserControl
    {

        public EditableTextBlock()
        {
            InitializeComponent();
        }

        public Visibility TextBlockVisibility
        {
            get { return (Visibility)GetValue(TextBlockVisibilityProperty); }
            set { SetValue(TextBlockVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextBlockVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextBlockVisibilityProperty =
            DependencyProperty.Register("TextBlockVisibility", typeof(Visibility), typeof(EditableTextBlock), new PropertyMetadata(Visibility.Visible));

      

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(EditableTextBlock), new PropertyMetadata(default(string)));       




        public bool IsEditing
        {
            get { return (bool)GetValue(EnableEditingProperty); }
            set { SetValue(EnableEditingProperty, value);}
        }

        // Using a DependencyProperty as the backing store for EnableEditing.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableEditingProperty =
            DependencyProperty.Register("IsEditing", typeof(bool), typeof(EditableTextBlock), new PropertyMetadata(OnIsEditingChanged));

        private static void OnIsEditingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if((bool)e.NewValue == true)
            {
                EditableTextBlock tb = (EditableTextBlock)d;
                tb.EnableEditing();
                
            }
        }


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public void EnableEditing()
        {
            ShortcutController.BlockShortcutExecution = true;
            TextBlockVisibility = Visibility.Hidden;
            IsEditing = true;
        }

        private void DisableEditing()
        {
            TextBlockVisibility = Visibility.Visible;
            ShortcutController.BlockShortcutExecution = false;
            IsEditing = false;
        }


        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                EnableEditing();
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                DisableEditing();
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            DisableEditing();
        }

        private void textBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            DisableEditing();
        }
    }
}
