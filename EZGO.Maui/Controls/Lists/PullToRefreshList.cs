using Syncfusion.Maui.ListView;
using Syncfusion.Maui.PullToRefresh;
using EZGO.Maui.Core.Extensions;
using System.Windows.Input;
using Syncfusion.Maui.DataSource;
using EZGO.Maui.Core.Enumerations;
using ListViewLayout = EZGO.Maui.Core.Enumerations.ListViewLayout;
using EZGO.Maui.Classes;
using EZGO.Maui.Converters;
using EZGO.Maui.Behaviors;

namespace EZGO.Maui.Controls.Lists;

public class PullToRefreshList<T> : ContentView, IDisposable
{
    #region Bindable Properties

    #region ListItemSource
    public static readonly BindableProperty ListItemsSourceProperty = BindableProperty.Create(
        nameof(ListItemsSource),
        typeof(object),
        typeof(PullToRefreshList<T>),
        propertyChanged: OnListItemsSourcePropertyChanged);

    private static void OnListItemsSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as PullToRefreshList<T>;
        if (obj?.List == null)
            return;

        obj.List.ItemsSource = obj?.ListItemsSource;
        if (obj.AreHeaderAndFooterVisibleWhenEmpty)
        {
            obj.List.IsVisible = true;
            if (!(obj.ListItemsSource as List<T>).IsNullOrEmpty())
            {
                obj.Empty.IsVisible = false;
            }
        }
    }

    public object ListItemsSource
    {
        get => (object)GetValue(ListItemsSourceProperty);
        set
        {
            SetValue(ListItemsSourceProperty, value);
            OnPropertyChanged();
        }
    }
    public static readonly BindableProperty HeaderItemTemplateProperty = BindableProperty.Create(
        nameof(HeaderItemTemplate),
        typeof(DataTemplate),
        typeof(PullToRefreshList<T>),
        propertyChanged: OnListViewLayoutPropertyChanged);

    public DataTemplate HeaderItemTemplate
    {
        get => (DataTemplate)GetValue(HeaderItemTemplateProperty);
        set => SetValue(HeaderItemTemplateProperty, value);
    }
    

    #endregion

    #region GridItemTemplate

    public static readonly BindableProperty GridItemTemplateProperty = BindableProperty.Create(
        nameof(GridItemTemplate),
        typeof(DataTemplate),
        typeof(PullToRefreshList<T>),
        propertyChanged: OnListViewLayoutPropertyChanged);

    public DataTemplate GridItemTemplate
    {
        get => (DataTemplate)GetValue(GridItemTemplateProperty);
        set => SetValue(GridItemTemplateProperty, value);
    }
   

    #endregion

    #region LinearItemTemplate

    public static readonly BindableProperty LinearItemTemplateProperty = BindableProperty.Create(nameof(LinearItemTemplate), typeof(DataTemplate), typeof(PullToRefreshList<T>), propertyChanged: OnLinearItemTemplatePropertyChanged);

    public DataTemplate LinearItemTemplate
    {
        get => (DataTemplate)GetValue(LinearItemTemplateProperty);
        set => SetValue(LinearItemTemplateProperty, value);
    }

    private static void OnLinearItemTemplatePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as PullToRefreshList<T>;
        obj.LinearItemTemplate = newValue as DataTemplate;
        obj.SetTemplate();
    }


    #endregion

    #region LoadMoreCommand

    public static readonly BindableProperty LoadMoreCommandProperty = BindableProperty.Create(nameof(LoadMoreCommand), typeof(ICommand), typeof(PullToRefreshList<T>));

    public ICommand LoadMoreCommand
    {
        get => (ICommand)GetValue(LoadMoreCommandProperty);
        set => SetValue(LoadMoreCommandProperty, value);
    }

    #endregion

    #region ListLayout

    public static readonly BindableProperty ListViewLayoutProperty = BindableProperty.Create(nameof(ListViewLayout), typeof(ListViewLayout), typeof(PullToRefreshList<T>), defaultValue: ListViewLayout.Grid, propertyChanged: OnListViewLayoutPropertyChanged);

    private static void OnListViewLayoutPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as PullToRefreshList<T>;
        obj.SetTemplate();
    }

    private void SetTemplate()
    {
        List.ItemSpacing = ListItemSpacing;
        List.ItemSize = ListItemSize;
        if (ListViewLayout == ListViewLayout.Grid)
        {
            List.ItemTemplate = GridItemTemplate;
            List.HeaderTemplate = null;
        }
        else
        {
            List.ItemTemplate = LinearItemTemplate;
            List.HeaderTemplate = HeaderItemTemplate;
            List.ItemSpacing = ListItemSpacingForList;
            List.ItemSize = ListItemSizeForList;
        }
    }

    public ListViewLayout ListViewLayout
    {
        get => (ListViewLayout)GetValue(ListViewLayoutProperty);
        set => SetValue(ListViewLayoutProperty, value);
    }

    #endregion

    #region SpanCount

    public static readonly BindableProperty SpanCountProperty = BindableProperty.Create(
        nameof(SpanCount),
        typeof(int),
        typeof(PullToRefreshList<T>),
        propertyChanged: OnSpanCountPropertyChanged);

    private static void OnSpanCountPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as PullToRefreshList<T>;
        obj.List.ItemsLayout = new GridLayout
        {
            SpanCount = obj.SpanCount < 1 ? 1 : obj.SpanCount,
        };
    }

    public int SpanCount
    {
        get => (int)GetValue(SpanCountProperty);
        set
        {
            SetValue(SpanCountProperty, value);
            OnPropertyChanged();
        }
    }

    #endregion

    #region ListItemSpacing

    public static readonly BindableProperty ListItemSpacingProperty = BindableProperty.Create(
        nameof(ListItemSpacing),
        typeof(int),
        typeof(PullToRefreshList<T>),
        propertyChanged: OnListItemSpacingPropertyChanged);

    private static void OnListItemSpacingPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as PullToRefreshList<T>;
        obj.List.ItemSpacing = obj.ListItemSpacing;
    }

    public int ListItemSpacing
    {
        get => (int)GetValue(ListItemSpacingProperty);
        set
        {
            SetValue(ListItemSpacingProperty, value);
            OnPropertyChanged();
        }
    }

    public static readonly BindableProperty ListItemSpacingForListProperty = BindableProperty.Create(
        nameof(ListItemSpacingForList),
        typeof(int),
        typeof(PullToRefreshList<T>));

    public int ListItemSpacingForList
    {
        get => (int)GetValue(ListItemSpacingForListProperty);
        set
        {
            SetValue(ListItemSpacingForListProperty, value);
            OnPropertyChanged();
        }
    }

    #endregion

    #region ListItemSize

    public static readonly BindableProperty ListItemSizeProperty = BindableProperty.Create(
        nameof(ListItemSize),
        typeof(int),
        typeof(PullToRefreshList<T>),
        propertyChanged: OnListItemSizePropertyChanged);

    private static void OnListItemSizePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as PullToRefreshList<T>;
        obj.List.ItemSize = obj.ListItemSize;
    }
    public int ListItemSize
    {
        get => (int)GetValue(ListItemSizeProperty);
        set => SetValue(ListItemSizeProperty, value);
    }

     public static readonly BindableProperty ListItemSizeForListProperty = BindableProperty.Create(
        nameof(ListItemSizeForList),
        typeof(int),
        typeof(PullToRefreshList<T>),
        propertyChanged: OnListItemForSizePropertyChanged);

    private static void OnListItemForSizePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as PullToRefreshList<T>;
        obj.List.ItemSize = obj.ListItemSizeForList;
    }
    public int ListItemSizeForList
    {
        get => (int)GetValue(ListItemSizeForListProperty);
        set => SetValue(ListItemSizeForListProperty, value);
    }


    #endregion

    #region ListBehaviour

    public static readonly BindableProperty NrOfItemsVisibleProperty = BindableProperty.Create(nameof(NrOfItemsVisible), typeof(double), typeof(PullToRefreshList<T>), propertyChanged: OnNrOfItemsVisiblePropertyChanged);

    private static void OnNrOfItemsVisiblePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as PullToRefreshList<T>;
        obj.List.Behaviors.Add(new ScalableItemSizeListViewBehavior { NumberOfItemsVisible = obj.NrOfItemsVisible });
    }

    public double NrOfItemsVisible
    {
        get => (double)GetValue(NrOfItemsVisibleProperty);
        set
        {
            SetValue(NrOfItemsVisibleProperty, value);
            OnPropertyChanged();
        }
    }

    #endregion

    #region HasItems

    public static readonly BindableProperty HasItemsProperty = BindableProperty.Create(nameof(HasItems), typeof(bool), typeof(PullToRefreshList<T>), defaultValue: true);

    public bool HasItems
    {
        get => (bool)GetValue(HasItemsProperty);
        set => SetValue(HasItemsProperty, value);
    }

    #endregion

    #region ListDataSource

    public static readonly BindableProperty ListDataSourceProperty = BindableProperty.Create(nameof(ListDataSource), typeof(DataSource), typeof(PullToRefreshList<T>), defaultBindingMode: BindingMode.OneWayToSource);

    public DataSource ListDataSource
    {
        get => (DataSource)GetValue(ListDataSourceProperty);
        set => SetValue(ListDataSourceProperty, value);
    }

    #endregion

    #region ListItemCommand

    public static readonly BindableProperty ListItemCommandProperty = BindableProperty.Create(nameof(ListItemCommand), typeof(ICommand), typeof(PullToRefreshList<T>));

    public ICommand ListItemCommand
    {
        get => (ICommand)GetValue(ListItemCommandProperty);
        set => SetValue(ListItemCommandProperty, value);
    }

    #endregion

    #region RefreshCommand

    public static readonly BindableProperty RefreshCommandProperty = BindableProperty.Create(nameof(RefreshCommand), typeof(ICommand), typeof(PullToRefreshList<T>));

    public ICommand RefreshCommand
    {
        get => (ICommand)GetValue(RefreshCommandProperty);
        set => SetValue(RefreshCommandProperty, value);
    }

    #endregion

    #region HeaderTemplate

    public static readonly BindableProperty HeaderTemplateProperty = BindableProperty.Create(nameof(HeaderTemplate), typeof(DataTemplate), typeof(PullToRefreshList<T>));

    public DataTemplate HeaderTemplate
    {
        get => (DataTemplate)GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    #endregion

    #region FotterTemplate

    public static readonly BindableProperty FooterTemplateProperty = BindableProperty.Create(nameof(FooterTemplate), typeof(DataTemplate), typeof(PullToRefreshList<T>));

    public DataTemplate FooterTemplate
    {
        get => (DataTemplate)GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }

    #endregion

    #region HeaderSize

    public static readonly BindableProperty HeaderSizeProperty = BindableProperty.Create(nameof(HeaderSize), typeof(double), typeof(PullToRefreshList<T>));

    public double HeaderSize
    {
        get => (double)GetValue(HeaderSizeProperty);
        set => SetValue(HeaderSizeProperty, value);
    }

    #endregion

    #region FooterSize

    public static readonly BindableProperty FooterSizeProperty = BindableProperty.Create(nameof(FooterSize), typeof(double), typeof(PullToRefreshList<T>));

    public double FooterSize
    {
        get => (double)GetValue(FooterSizeProperty);
        set => SetValue(FooterSizeProperty, value);
    }

    #endregion

    #region IsRefreshing

    public static readonly BindableProperty IsRefreshingProperty = BindableProperty.Create(nameof(IsRefreshing), typeof(bool), typeof(PullToRefreshList<T>), propertyChanged: OnIsRefreshingPropertyChanged);

    private static void OnIsRefreshingPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as PullToRefreshList<T>;
        obj.PullToRefresh.IsRefreshing = obj.IsRefreshing;
    }



    private bool _disposedValue;
    public void Dispose()
    {
        Dispose(true);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                //List?.Dispose();
                List = null;
                //PullToRefresh?.Dispose();
                PullToRefresh = null;
                Content = null;
            }

            _disposedValue = true;
        }
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

    #region AreHeaderAndFooterVisibleWhenEmpty

    public static readonly BindableProperty AreHeaderAndFooterVisibleWhenEmptyProperty = BindableProperty.Create(nameof(AreHeaderAndFooterVisibleWhenEmpty), typeof(bool), typeof(PullToRefreshList<T>), defaultValue: false);

    public bool AreHeaderAndFooterVisibleWhenEmpty
    {
        get => (bool)GetValue(AreHeaderAndFooterVisibleWhenEmptyProperty);
        set => SetValue(AreHeaderAndFooterVisibleWhenEmptyProperty, value);
    }

    #endregion

    #region CachingStrategy

    public static readonly BindableProperty ListCachingStrategyProperty = BindableProperty.Create(nameof(ListCachingStrategy), typeof(CachingStrategy), typeof(PullToRefreshList<T>), defaultValue: CachingStrategy.RecycleTemplate);

    public CachingStrategy ListCachingStrategy
    {
        get => (CachingStrategy)GetValue(ListCachingStrategyProperty);
        set => SetValue(ListCachingStrategyProperty, value);
    }

    #endregion

    #region SelectedItem

    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(T), typeof(PullToRefreshList<T>), defaultValue: null);

    public T SelectedItem
    {
        get => (T)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    #endregion

    #region SelectedItemTemplate

    public static readonly BindableProperty SelectedItemTemplateProperty = BindableProperty.Create(nameof(SelectedItemTemplate), typeof(DataTemplate), typeof(PullToRefreshList<T>));

    public DataTemplate SelectedItemTemplate
    {
        get => (DataTemplate)GetValue(SelectedItemTemplateProperty);
        set => SetValue(SelectedItemTemplateProperty, value);
    }

    #endregion

    #region SelectionModeTemplate

    public static readonly BindableProperty SelectionListModeProperty = BindableProperty.Create(nameof(SelectionListMode), typeof(Syncfusion.Maui.ListView.SelectionMode), typeof(PullToRefreshList<T>), defaultValue: Syncfusion.Maui.ListView.SelectionMode.None);

    public Syncfusion.Maui.ListView.SelectionMode SelectionListMode
    {
        get => (Syncfusion.Maui.ListView.SelectionMode)GetValue(SelectionListModeProperty);
        set => SetValue(SelectionListModeProperty, value);
    }

    #endregion

    #region SelectionBackgroundColor

    public static readonly BindableProperty SelectionBackgroundColorProperty = BindableProperty.Create(nameof(SelectionBackgroundColor), typeof(Color), typeof(PullToRefreshList<T>), defaultValue: Colors.White);

    public Color SelectionBackgroundColor
    {
        get => (Color)GetValue(SelectionBackgroundColorProperty);
        set => SetValue(SelectionBackgroundColorProperty, value);
    }

    #endregion

    #region LoadMoreOption

    public static readonly BindableProperty LoadMoreOptionProperty = BindableProperty.Create(nameof(LoadMoreOption), typeof(LoadMoreOption), typeof(PullToRefreshList<T>), defaultValue: LoadMoreOption.None);

    public LoadMoreOption LoadMoreOption
    {
        get => (LoadMoreOption)GetValue(LoadMoreOptionProperty);
        set => SetValue(LoadMoreOptionProperty, value);
    }

    #endregion

    #region AutoFitMode

    public static readonly BindableProperty AutoFitModeProperty = BindableProperty.Create(nameof(AutoFitMode), typeof(AutoFitMode), typeof(PullToRefreshList<T>), defaultValue: AutoFitMode.None);

    public AutoFitMode AutoFitMode
    {
        get => (AutoFitMode)GetValue(AutoFitModeProperty);
        set => SetValue(AutoFitModeProperty, value);
    }

    #endregion

    #endregion

    public PullToRefreshList()
    {
        // Initialize PullToRefresh
        PullToRefresh = new SfPullToRefresh
        {
            PullingThreshold = 100,
            RefreshViewHeight = 30,
            RefreshViewThreshold = 30,
            RefreshViewWidth = 30,
            ProgressColor= Color.FromArgb("#0003E8"),
            BackgroundColor = ResourceHelper.GetValueFromResources<Color>(nameof(BackgroundColor)),
            RefreshCommand = RefreshCommand,
            IsRefreshing = IsRefreshing,
        };
        PullToRefresh.SetBinding(SfPullToRefresh.RefreshCommandProperty, new Binding(nameof(RefreshCommand)));
        PullToRefresh.BindingContext = this;

        var grid = new Grid();

        // Initialize & Binding ListView
        List = new SfListView
        {
            IsVisible = HasItems,
            Padding = new Thickness(10),
            LoadMorePosition = LoadMorePosition.End,
            ItemSpacing = 10,
        };
        List.SetBinding(SfListView.DataSourceProperty, new Binding(nameof(ListDataSource), mode: BindingMode.OneWayToSource));
        List.SetBinding(SfListView.TapCommandProperty, new Binding(nameof(ListItemCommand)));
        List.SetBinding(SfListView.HeaderTemplateProperty, new Binding(nameof(HeaderTemplate)));
        List.SetBinding(SfListView.HeaderSizeProperty, new Binding(nameof(HeaderSize)));
        List.SetBinding(SfListView.FooterTemplateProperty, new Binding(nameof(FooterTemplate)));
        List.SetBinding(SfListView.FooterSizeProperty, new Binding(nameof(FooterSize)));
        List.SetBinding(SfListView.IsVisibleProperty, new Binding(nameof(HasItems)));
        List.SetBinding(SfListView.LoadMoreCommandProperty, new Binding(nameof(LoadMoreCommand)));
        List.SetBinding(SfListView.CachingStrategyProperty, new Binding(nameof(ListCachingStrategy)));
        List.SetBinding(SfListView.SelectedItemProperty, new Binding(nameof(SelectedItem)));
        List.SetBinding(SfListView.SelectedItemTemplateProperty, new Binding(nameof(SelectedItemTemplate)));
        List.SetBinding(SfListView.SelectionModeProperty, new Binding(nameof(SelectionListMode)));
        List.SetBinding(SfListView.SelectionBackgroundProperty, new Binding(nameof(SelectionBackgroundColor)));
        List.SetBinding(SfListView.LoadMoreOptionProperty, new Binding(nameof(LoadMoreOption)));
        List.SetBinding(SfListView.BackgroundColorProperty, new Binding(nameof(BackgroundColor)));
        List.SetBinding(SfListView.AutoFitModeProperty, new Binding(nameof(AutoFitMode)));
        List.BindingContext = this;

        // Empty View
        Empty = new EmptyView();
        Empty.SetBinding(EmptyView.IsVisibleProperty, nameof(HasItems), converter: new ReverseBooleanConverter());
        Empty.SetBinding(EmptyView.BackgroundColorProperty, nameof(BackgroundColor));
        Empty.BindingContext = this;

        grid.Children.Add(List);
        grid.Children.Add(Empty);
        PullToRefresh.PullableContent = grid;
        Content = PullToRefresh;
    }

    public SfListView List { get; set; }
    private EmptyView Empty { get; set; }
    private SfPullToRefresh PullToRefresh { get; set; }
}
