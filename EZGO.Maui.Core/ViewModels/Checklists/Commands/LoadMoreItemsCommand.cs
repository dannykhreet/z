using EZGO.Maui.Core.ViewModels.Tasks;
using Syncfusion.Maui.DataSource.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels.Checklists.Commands
{
    public class LoadMoreItemsCommand : ICommand
    {
        public LoadMoreItemsCommand(TaskSlideViewModel taskSlideViewModel)
        {
            TaskSlideViewModel = taskSlideViewModel;
        }

        public TaskSlideViewModel TaskSlideViewModel { get; set; }
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            //if (TaskSlideViewModel.FilteredTasks.Count == TaskSlideViewModel.CurrentIndex + 5)
            //{
            //    return true;
            //}
            return false;
        }
        //TODO implement loading items on scroll without reloading whole view / fix loading item on tap from taskLIstViewModel
        //ItemSwippedEventArgs
        public void Execute(object parameter)
        {
            if (!CanExecute(parameter)) return;
            var tasks = parameter as TaskSlideViewModel;

            //TaskSlideViewModel.UnfilteredTasks.Skip(TaskSlideViewModel.CurrentIndex).Take(10).ForEach(x=>TaskSlideViewModel.FilteredTasks.Add(x));
        }
    }
}
