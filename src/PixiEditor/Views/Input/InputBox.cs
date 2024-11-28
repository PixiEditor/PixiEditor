using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace PixiEditor.Views.Input;

internal class InputBox : TextBox
{
    public ICommand SubmitCommand
    {
        get { return (ICommand)GetValue(SubmitCommandProperty); }
        set { SetValue(SubmitCommandProperty, value); }
    }


    public static readonly StyledProperty<ICommand> SubmitCommandProperty =
        AvaloniaProperty.Register<InputBox, ICommand>(nameof(SubmitCommand));

    public object SubmitCommandParameter
    {
        get { return (object)GetValue(SubmitCommandParameterProperty); }
        set { SetValue(SubmitCommandParameterProperty, value); }
    }


    public static readonly StyledProperty<object> SubmitCommandParameterProperty =
        AvaloniaProperty.Register<InputBox, object>(nameof(SubmitCommandParameter));

    public event EventHandler<InputBoxEventArgs> OnSubmit;

    protected override Type StyleKeyOverride => typeof(TextBox);

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        OnSubmit?.Invoke(this, new InputBoxEventArgs(Text));
        //TODO: Keyboard.ClearFocus();
        //Keyboard.ClearFocus();

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
