
using System.ComponentModel;
using EZGO.Maui.Core.ViewModels;

namespace EZGO.Maui.Views.Actions;

public partial class ActionReportPage : ContentPage
{

    public ActionReportPage()
    {
        InitializeComponent();
    }
    private ActionReportViewModel _viewModel;

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _viewModel = BindingContext as ActionReportViewModel;

        if (_viewModel != null)
        {

            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        var newViewModel = BindingContext as ActionReportViewModel;

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
        if (e.PropertyName == nameof(_viewModel.SpanCount))
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (_viewModel.SpanCount > 0 && GridLayout != null)
                {
                    GridLayout.SpanCount = _viewModel.SpanCount;
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