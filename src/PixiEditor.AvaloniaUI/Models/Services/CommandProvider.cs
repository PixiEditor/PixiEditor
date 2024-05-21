using System.Windows.Input;
using Avalonia.Media;
using PixiEditor.AvaloniaUI.Models.Commands;
using PixiEditor.AvaloniaUI.Models.Commands.Commands;
using PixiEditor.AvaloniaUI.Models.Commands.Evaluators;
using XAMLCommand = PixiEditor.AvaloniaUI.Models.Commands.XAML.Command;

namespace PixiEditor.AvaloniaUI.Models.Services;

internal class CommandProvider
{
    private readonly CommandController _controller;

    public CommandProvider(CommandController controller)
    {
        _controller = controller;
    }

    public Command GetCommand(string name) => _controller.Commands[name];

    public CanExecuteEvaluator GetCanExecute(string name) => _controller.CanExecuteEvaluators[name];

    public bool CanExecute(string name, Command command, object argument) =>
        _controller.CanExecuteEvaluators[name].CallEvaluate(command, argument);

    public IconEvaluator GetIconEvaluator(string name) => _controller.IconEvaluators[name];

    public IImage GetIcon(string name, Command command, object argument) =>
        _controller.IconEvaluators[name].CallEvaluate(command, argument);

    public ICommand GetICommand(string name, bool useProvidedArgument = false) => XAMLCommand.GetICommand(_controller.Commands[name], useProvidedArgument);
}
