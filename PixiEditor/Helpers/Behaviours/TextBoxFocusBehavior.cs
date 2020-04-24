using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace PixiEditor.Helpers.Behaviours
{
    class TextBoxFocusBehavior : Behavior<TextBox>
    {

        public bool FillSize
        {
            get { return (bool)GetValue(FillSizeProperty); }
            set { SetValue(FillSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FillSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FillSizeProperty =
            DependencyProperty.Register("FillSize", typeof(bool), typeof(TextBoxFocusBehavior), new PropertyMetadata(false));





        public FocusNavigationDirection NextFocusDirection
        {
            get { return (FocusNavigationDirection)GetValue(NextFocusDirectionProperty); }
            set { SetValue(NextFocusDirectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NextFocusDirection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NextFocusDirectionProperty =
            DependencyProperty.Register("NextFocusDirection", typeof(FocusNavigationDirection), typeof(TextBoxFocusBehavior), 
                new PropertyMetadata(FocusNavigationDirection.Up));







        private string _oldText; //Value of textbox before editing
        private bool _valueConverted = false; //This bool is used to avoid double convertion if enter is hitted

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.LostKeyboardFocus += AssociatedObject_LostKeyboardFocus;
            AssociatedObject.GotKeyboardFocus += AssociatedObject_GotKeyboardFocus;
            AssociatedObject.KeyUp += AssociatedObject_KeyUp;
            AssociatedObject.GotMouseCapture += AssociatedObject_GotMouseCapture;
        }

        private void AssociatedObject_GotMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
        {
            AssociatedObject.SelectAll(); //Selects all text on mouse click
        }
        
        //Converts number to proper format if enter is clicked and moves focus to next object
        private void AssociatedObject_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter) return;

            ConvertValue();
            AssociatedObject.MoveFocus(new TraversalRequest(NextFocusDirection));
        }

        private void AssociatedObject_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            AssociatedObject.SelectAll();
            if (FillSize)
            {
                _valueConverted = false;
                _oldText = AssociatedObject.Text; //Sets old value when keyboard is focused on object
            }
        }

        private void AssociatedObject_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            ConvertValue();            
        }

        /// <summary>
        /// Converts number from textbox to format "number px" ex. "15 px"
        /// </summary>
        private void ConvertValue()
        {
            if (_valueConverted == true || FillSize == false) return;
            if (int.TryParse(Regex.Replace(AssociatedObject.Text, "\\p{L}", ""), out _) == true)
            {
                AssociatedObject.Text = string.Format("{0} {1}", AssociatedObject.Text, "px");
            }
            else //If text in textbox isn't number, set it to old value
            {
                AssociatedObject.Text = _oldText;
            }
            _valueConverted = true;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.LostKeyboardFocus -= AssociatedObject_LostKeyboardFocus;
            AssociatedObject.GotKeyboardFocus -= AssociatedObject_GotKeyboardFocus;
            AssociatedObject.KeyUp -= AssociatedObject_KeyUp;
            AssociatedObject.GotMouseCapture -= AssociatedObject_GotMouseCapture;

        }
    }
}
