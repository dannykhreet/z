using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;

namespace EZGO.Maui.Core.ViewModels.Checklists.Commands
{
    public class FilterCommand : ICommand
    {
        public FilterCommand(BaseViewModel baseViewModel)
        {
            BaseViewModel = baseViewModel;
        }
        public event EventHandler CanExecuteChanged;
        public BaseViewModel BaseViewModel { get; set; }

        public static bool IsExecuted { get; set; } = false;

        public bool CanExecute(object parameter)
        {
            if (IsExecuted)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public async void Execute(object parameter)
        {
            var param = parameter as TaskStatusEnum?;
            if (param == null) return;
            if (!CanExecute(parameter)) return;

            IsExecuted = true;

            BaseViewModel.ApplyFilter(param);

            if (MainThread.GetMainThreadSynchronizationContextAsync().IsCompleted)
                IsExecuted = false;
        }
    }
}
