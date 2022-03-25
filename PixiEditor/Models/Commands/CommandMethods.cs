namespace PixiEditor.Models.Commands;

public class CommandMethods
{
    private readonly Action<object> _execute;
    private readonly Predicate<object> _canExecute;

    public CommandMethods(Action<object> execute, Predicate<object> canExecute)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public void Execute(object parameter)
    {
        if (CanExecute(parameter))
        {
            _execute(parameter);
        }
    }

    public bool CanExecute(object parameter) => _canExecute(parameter);
}
