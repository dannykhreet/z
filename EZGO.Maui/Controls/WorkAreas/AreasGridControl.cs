using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using EZGO.Maui.Core.Models.Areas;

namespace EZGO.Maui.Controls.WorkAreas
{
    public partial class AreasGridControl : ContentView
    {
        #region Items Property

        public readonly static BindableProperty ItemsProperty = BindableProperty.Create(nameof(ItemsProperty), typeof(ObservableCollection<BasicWorkAreaModel>), typeof(AreasGridControl), propertyChanged: OnItemsPropertyChanged);

        private static void OnItemsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            try
            {
                var obj = bindable as AreasGridControl;
                if (obj != null)
                {
                    BindableLayout.SetItemsSource(obj.AreaGrid, obj.Items);
                    obj.InitializeAreaGrid();
                    obj.HasItems = obj.Items?.Count > 0;
                    //obj.Placeholder.IsVisible = false;
                    obj.AreaGrid.IsVisible = true;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
                Debugger.Break();
            }
        }

        public ObservableCollection<BasicWorkAreaModel> Items
        {
            get => (ObservableCollection<BasicWorkAreaModel>)GetValue(ItemsProperty);
            set
            {
                SetValue(ItemsProperty, value);
                OnPropertyChanged();
            }
        }

        #endregion

        #region SelectWorkArea Property

        public readonly static BindableProperty SelectWorkAreaCommandProperty = BindableProperty.Create(nameof(SelectWorkAreaCommand), typeof(ICommand), typeof(AreasGridControl));

        public ICommand SelectWorkAreaCommand
        {
            get => (ICommand)GetValue(SelectWorkAreaCommandProperty);
            set
            {
                SetValue(SelectWorkAreaCommandProperty, value);
                OnPropertyChanged();
            }
        }

        #endregion

        #region GridMargin Property

        public readonly static BindableProperty GridMarginProperty = BindableProperty.Create(nameof(GridMarginProperty), typeof(Thickness), typeof(AreasGridControl), defaultValue: new Thickness(20, 0), propertyChanged: OnGridMarginPropertyChanged);

        private static void OnGridMarginPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var obj = bindable as AreasGridControl;
            //obj.Placeholder.Margin = obj.GridMargin;
            if (obj != null)
                obj.AreaScrollView.Margin = obj.GridMargin;
        }

        public Thickness GridMargin
        {
            get => (Thickness)GetValue(GridMarginProperty);
            set
            {
                SetValue(GridMarginProperty, value);
                OnPropertyChanged();
            }
        }

        #endregion

        #region GridRowSpacing Property

        public readonly static BindableProperty GridRowSpacingProperty = BindableProperty.Create(nameof(GridRowSpacingProperty), typeof(double), typeof(AreasGridControl), defaultValue: 10.0, propertyChanged: OnGridRowSpacingPropertyChanged);

        private static void OnGridRowSpacingPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var obj = bindable as AreasGridControl;
            if (obj != null)
                obj.AreaGrid.RowSpacing = obj.GridRowSpacing;
        }

        public double GridRowSpacing
        {
            get => (double)GetValue(GridRowSpacingProperty);
            set
            {
                SetValue(GridRowSpacingProperty, value);
                OnPropertyChanged();
            }
        }

        #endregion

        #region RefreshCommand Property

        public readonly static BindableProperty RefreshCommandProperty = BindableProperty.Create(nameof(RefreshCommandProperty), typeof(ICommand), typeof(AreasGridControl), propertyChanged: OnRefreshCommandPropertyChanged);

        private static void OnRefreshCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var obj = bindable as AreasGridControl;
            if (obj != null)
                obj.pullToRefresh.RefreshCommand = obj.RefreshCommand;
        }

        public ICommand RefreshCommand
        {
            get => (ICommand)GetValue(RefreshCommandProperty);
            set
            {
                SetValue(RefreshCommandProperty, value);
                OnPropertyChanged();
            }
        }

        #endregion

        #region IsRefreshing Property

        public readonly static BindableProperty IsRefreshingProperty = BindableProperty.Create(nameof(IsRefreshingProperty), typeof(bool), typeof(AreasGridControl), propertyChanged: OnIsRefreshingPropertyChanged);

        private static void OnIsRefreshingPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var obj = bindable as AreasGridControl;
            if (obj != null)
                obj.pullToRefresh.IsRefreshing = obj.IsRefreshing;
        }

        public bool IsRefreshing
        {
            get => (bool)GetValue(IsRefreshingProperty);
            set
            {
                SetValue(IsRefreshingProperty, value);
                OnPropertyChanged();
            }
        }

        #endregion

        public bool HasItems { get; set; } = true;
    }
}

