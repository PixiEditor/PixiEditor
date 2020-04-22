using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace PixiEditor.Helpers.Behaviours
{
    class TextBoxNumericFinisherBehavior : Behavior<TextBox>
    {
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
            AssociatedObject.MoveFocus(new System.Windows.Input.TraversalRequest(System.Windows.Input.FocusNavigationDirection.Next));
        }

        private void AssociatedObject_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            _valueConverted = false;
            _oldText = AssociatedObject.Text; //Sets old value when keyboard is focused on object
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
            if (_valueConverted == true) return;
            if (int.TryParse(AssociatedObject.Text, out _) == true)
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
