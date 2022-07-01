using PixiEditor.Models.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PixiEditor.Views.UserControls
{
    public class InputBox : TextBox
    {
        public ICommand SubmitCommand
        {
            get { return (ICommand)GetValue(SubmitCommandProperty); }
            set { SetValue(SubmitCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SubmitCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SubmitCommandProperty =
            DependencyProperty.Register("SubmitCommand", typeof(ICommand), typeof(InputBox));

        public object SubmitCommandParameter
        {
            get { return (object)GetValue(SubmitCommandParameterProperty); }
            set { SetValue(SubmitCommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SubmitCommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SubmitCommandParameterProperty =
            DependencyProperty.Register("SubmitCommandParameter", typeof(object), typeof(InputBox), new PropertyMetadata(null));

        public event EventHandler<InputBoxEventArgs> OnSubmit;

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            OnSubmit?.Invoke(this, new InputBoxEventArgs(Text));
            Keyboard.ClearFocus();

            base.OnLostFocus(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            if(SubmitCommand != null && SubmitCommand.CanExecute(SubmitCommandParameter))
            {
                SubmitCommand.Execute(SubmitCommandParameter);
            }

            OnSubmit?.Invoke(this, new InputBoxEventArgs(Text));

            e.Handled = true;
        }
    }
}
