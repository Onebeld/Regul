using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Onebeld.Extensions
{
    public class Command<T> : Command, ICommand
    {
        private readonly Action<T> _action;
        private bool _busy;
        private Func<T, Task> _acb;

        private bool Busy
        {
            get => _busy;
            set
            {
                _busy = value;
                
            }
        }

        public Command(Action<T> action) => _action = action;
        public Command(Func<T, Task> acb) => _acb = acb;

        public override event EventHandler CanExecuteChanged;
        public override bool CanExecute(object parameter) => !_busy;
        public override async void Execute(object parameter)
        {
            if (Busy) return;

            try
            {
                Busy = true;
                if (_action != null) _action((T) parameter);
                else await _acb((T) parameter);
            }
            finally
            {
                Busy = false;
            }
        }
    }
    
    public abstract class Command : ICommand
    {
        public static Command Create(Action action) => new Command<object>(_ => action());
        public static Command Create<TArg>(Action<TArg> cb) => new Command<TArg>(cb);
        public static Command CreateFromTask(Func<Task> cb) => new Command<object>(_ => cb());

        public abstract bool CanExecute(object parameter);
        public abstract void Execute(object parameter);
        public abstract event EventHandler CanExecuteChanged;
    }
}