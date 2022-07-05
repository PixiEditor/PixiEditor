using PixiEditor.Models.Commands.Evaluators;

namespace PixiEditor.Models.Commands;

internal class CommandMethods
{
    private readonly Command _command;
    private readonly Action<object> _execute;
    private readonly CanExecuteEvaluator _canExecute;

    public CommandMethods(Command command, Action<object> execute, CanExecuteEvaluator canExecute)
    {
        _execute = execute;
        _canExecute = canExecute;
        _command = command;
    }

    public void Execute(object parameter)
    {
        if (CanExecute(parameter))
        {
            _execute(parameter);
        }
    }

    public bool CanExecute(object parameter) => _canExecute.CallEvaluate(_command, parameter);
}
