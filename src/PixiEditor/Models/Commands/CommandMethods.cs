using PixiEditor.Models.Commands.Commands;
using PixiEditor.Models.Commands.Evaluators;

namespace PixiEditor.Models.Commands;

internal class CommandMethods
{
    public CanExecuteEvaluator CanExecuteEvaluator => _canExecute;
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
        var log = CommandController.Current?.Log;
        ToLog(log, null);

        if (!CanExecute(parameter))
        {
            ToLog(log, false);
            return;
        }
        ToLog(log, true);

        _execute(parameter);
    }

    public bool CanExecute(object parameter) => _canExecute.CallEvaluate(_command, parameter);
    
    
    private void ToLog(CommandLog.CommandLog? log, bool? canExecute)
    {
        if (log != null && _command != null)
        {
            log.Log(_command, canExecute);
        }
    }
}
