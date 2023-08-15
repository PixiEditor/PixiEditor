using System.Threading.Tasks;
using PixiEditor.AvaloniaUI.Models.Commands.Commands;
using PixiEditor.AvaloniaUI.Models.Commands.Evaluators;

namespace PixiEditor.AvaloniaUI.Models.Commands;

internal class CommandMethods
{
    private readonly Command _command;
    private readonly Func<object, Task> _execute;
    private readonly CanExecuteEvaluator _canExecute;

    public CommandMethods(Command command, Func<object, Task> execute, CanExecuteEvaluator canExecute)
    {
        _execute = execute;
        _canExecute = canExecute;
        _command = command;
    }

    public async Task Execute(object parameter)
    {
        if (CanExecute(parameter))
        {
            await _execute(parameter);
        }
    }

    public bool CanExecute(object parameter) => _canExecute.CallEvaluate(_command, parameter);
}
