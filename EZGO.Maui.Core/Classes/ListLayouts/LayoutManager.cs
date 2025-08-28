using System;
using System.Windows.Input;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Interfaces.Utils;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Classes.ListLayouts
{
    public class LayoutManager : NotifyPropertyChanged
    {
        public IListLayout CurrentLayout { get; set; }

        public ListViewLayout ListLayout { get; set; }

        public ICommand ListViewLayoutCommand { get; set; }

        public event EventHandler LayoutChanged;

        public LayoutManager()
        {
            ListViewLayoutCommand = new Command<object>(SetListViewLayout);
        }

        private void SetListViewLayout(object obj)
        {
            if (obj is ListViewLayout listViewLayout)
            {
                if (listViewLayout == ListLayout) return;

                if (listViewLayout == ListViewLayout.Grid)
                    CurrentLayout = new GridLayout();
                else
                    CurrentLayout = new LinearLayout();

                ListLayout = listViewLayout;
                Settings.AppSettings.ListViewLayout = listViewLayout;
                LayoutChanged?.Invoke(this, null);
            }
        }

        public void SetCurrentLayout()
        {
            SetListViewLayout(Settings.AppSettings.ListViewLayout);
        }
    }
}
