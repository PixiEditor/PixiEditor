using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PixiEditor.Models.Events;

namespace PixiEditor.Views.UserControls;

internal class InputBox : TextBox
{
    public ICommand SubmitCommand
    {
        get { return (ICommand)GetValue(SubmitCommandProperty); }
        set { SetValue(SubmitCommandProperty, value); }
    }


    public static readonly DependencyProperty SubmitCommandProperty =
        DependencyProperty.Register(nameof(SubmitCommand), typeof(ICommand), typeof(InputBox));

    public object SubmitCommandParameter
    {
        get { return (object)GetValue(SubmitCommandParameterProperty); }
        set { SetValue(SubmitCommandParameterProperty, value); }
    }


    public static readonly DependencyProperty SubmitCommandParameterProperty =
        DependencyProperty.Register(nameof(SubmitCommandParameter), typeof(object), typeof(InputBox), new PropertyMetadata(null));

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

        if (SubmitCommand != null && SubmitCommand.CanExecute(SubmitCommandParameter))
        {
            SubmitCommand.Execute(SubmitCommandParameter);
        }

        OnSubmit?.Invoke(this, new InputBoxEventArgs(Text));

        e.Handled = true;
    }
}
