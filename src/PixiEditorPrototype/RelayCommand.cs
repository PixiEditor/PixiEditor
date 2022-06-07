using System;
using System.Windows.Input;

namespace PixiEditorPrototype;

internal class RelayCommand : ICommand
{
    public event EventHandler? CanExecuteChanged;

    private readonly Action<object?> execute;
    private readonly Func<object?, bool>? canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool> canExecute)
    {
        this.execute = execute;
        this.canExecute = canExecute;
    }

    public RelayCommand(Action<object?> execute)
    {
        this.execute = execute;
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool CanExecute(object? parameter)
    {
        return canExecute?.Invoke(parameter) ?? true;
    }

    public void Execute(object? parameter)
    {
        execute.Invoke(parameter);
    }
}
