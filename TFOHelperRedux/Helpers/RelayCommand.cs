using System.Windows.Input;

namespace TFOHelperRedux.Helpers
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        // ✅ Основной конструктор (с параметром)
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // ✅ Дополнительный конструктор (без параметра)
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            _execute = _ => execute();
            _canExecute = _ => canExecute?.Invoke() ?? true;
        }

        public bool CanExecute(object? parameter)
            => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object? parameter)
            => _execute(parameter);

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
