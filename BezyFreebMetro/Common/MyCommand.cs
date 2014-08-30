using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BezyFreebMetro.Common
{
    public class MyCommand : ICommand
    {
        public Action<object> _Action;
        public MyCommand(Action<object> action)
        {
            _Action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _Action(parameter);
        }
    }
}
