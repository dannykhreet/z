using System;
using System.Windows.Input;

namespace EZGO.Maui.Controls.ViewCells
{
    public partial class ActionListViewCell : ViewCell
    {
        public static readonly BindableProperty ActionSolvedCommandProperty = BindableProperty.Create(nameof(ActionSolvedCommand), typeof(ICommand), typeof(ActionListViewCell));

        public ICommand ActionSolvedCommand
        {
            get => (ICommand)GetValue(ActionSolvedCommandProperty);
            set => SetValue(ActionSolvedCommandProperty, value);
        }

        public ActionListViewCell()
        {
            InitializeComponent();
        }
    }
}
