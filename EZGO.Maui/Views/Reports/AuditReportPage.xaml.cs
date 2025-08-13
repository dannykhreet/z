using System.ComponentModel;
using System.Reflection.PortableExecutable;
using EZGO.Maui.Core.ViewModels;

namespace EZGO.Maui.Views
{
    public partial class AuditReportPage : ContentPage
    {
        public AuditReportPage()
        {
            InitializeComponent();
        }
        private AuditReportViewModel _viewModel;

        public void RecalculateViewElementsPositions()
        {
            Top5DeviationsListView.AutoFitMode = Syncfusion.Maui.ListView.AutoFitMode.DynamicHeight;
            DoWeFollowUpOnDeviationsListView.AutoFitMode = Syncfusion.Maui.ListView.AutoFitMode.DynamicHeight;
            HowOftenAuditsListView.ItemSize = 145;
            HowDidWePerformListView.ItemSize = 145;
            Q3Header.Padding = 10;
            Q4Header.Padding = 10;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _viewModel = BindingContext as AuditReportViewModel;

            if (_viewModel != null)
            {

                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            var newViewModel = BindingContext as AuditReportViewModel;

            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }

            _viewModel = newViewModel;

            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }
        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_viewModel.SpanCountAudits))
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (_viewModel.SpanCountAudits > 0 && GridLayout != null)
                    {
                        GridLayout.SpanCount = _viewModel.SpanCountAudits;
                    }
                });
            }
            if (e.PropertyName == nameof(_viewModel.SpanCountAuditsAvg))
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (_viewModel.SpanCountAuditsAvg > 0 && GridLayout2 != null)
                    {
                        GridLayout2.SpanCount = _viewModel.SpanCountAuditsAvg;
                    }
                });
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
        }
    }
}
