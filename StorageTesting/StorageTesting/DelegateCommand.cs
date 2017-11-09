using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StorageTesting
{
    public class DelegateCommand : ICommand
    {
        private readonly Action _action;

        public DelegateCommand(Action ExecuteAction)
        {
            _action = ExecuteAction;
        }

        public void Execute(object parameter = null)
        {
            _action();
        }

        public bool CanExecute(object parameter = null)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged; 
    }
}
