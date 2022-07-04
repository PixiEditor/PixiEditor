using System;
using System.Windows.Input;

namespace PixiEditor.Helpers
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> execute;
        private readonly Predicate<T> canExecute;

        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(T parameter)
        {
            return canExecute == null ? true : canExecute(parameter);
        }

        public bool CanExecute(object parameter)
        {
            if (canExecute == null) return true;
            if(parameter != null && parameter is not T)
            {
                throw new ArgumentException("Provided parameter type does not match RelayCommand parameter type");
            }

            return CanExecute((T)parameter);
        }

        public void Execute(T parameter)
        {
            execute(parameter);
        }

        public void Execute(object parameter)
        {
            if (parameter != null && parameter is not T)
            {
                throw new ArgumentException("Provided parameter type does not match RelayCommand parameter type");
            }

            Execute((T)parameter);
        }
    }

    public class RelayCommand : RelayCommand<object>
    {
        public RelayCommand(Action<object> execute, Predicate<object> canExecute) : base(execute, canExecute) { }

        public RelayCommand(Action<object> execute) : base(execute) { }
    }
}
