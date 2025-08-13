using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.Filters;
using System.Windows.Input;
using Color = Microsoft.Maui.Graphics.Color;

namespace EZGO.Maui.Controls;

public partial class ExtSearchBar : StackLayout
{
    private CancellationTokenSource cts;

    public ExtSearchBar()
    {
        InitializeComponent();

        if (DeviceInfo.Platform == DevicePlatform.iOS)
            TagsRect = new Rect(1, .01, .1, .08);
        else if (DeviceInfo.Platform == DevicePlatform.Android)
            TagsRect = new Rect(1, .0, .1, .08);

        FilterButton.IsVisible = false;
        FilterButton.Command = new Command(() =>
        {
            if (IsButtonClicked)
                FilterButton.TextColor = ResourceHelper.GetApplicationResource<Color>("DarkGreyColor1");
            else
                FilterButton.TextColor = ResourceHelper.GetApplicationResource<Color>("GreenColor");

            IsButtonClicked = !IsButtonClicked;
            OnPropertyChanged(nameof(IsButtonClicked));
        });
    }

    #region Properties

    private int _textChangedDelay;
    public int TextChangedDelay
    {
        get { return _textChangedDelay; }
        set { _textChangedDelay = value; }
    }

    #endregion

    #region Bindable Properties

    public static readonly BindableProperty IsButtonClickedProperty = BindableProperty.Create(
        nameof(IsButtonClicked),
        typeof(bool),
        typeof(ExtSearchBar),
        propertyChanged: OnIsButtonClickedPropertyChanged);

    public bool IsButtonClicked
    {
        get => (bool)GetValue(IsButtonClickedProperty);
        set => SetValue(IsButtonClickedProperty, value);
    }

    public static readonly BindableProperty TagsRectProperty = BindableProperty.Create(
        nameof(TagsRect),
        typeof(Rect),
        typeof(ExtSearchBar));

    public Rect TagsRect
    {
        get => (Rect)GetValue(TagsRectProperty);
        set => SetValue(TagsRectProperty, value);
    }

    public static BindableProperty FilterProperty = BindableProperty.Create(nameof(Filter), typeof(IFilterControl), declaringType: typeof(ExtSearchBar), propertyChanged: OnFilterPropertyChanged);

    private static void OnFilterPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var filter = newValue as IFilterControl;
        var obj = bindable as ExtSearchBar;
        if (obj != null)
        {
            obj.Filter = filter;
            obj.TagsFilter.Filter = filter;
        }
    }

    public IFilterControl Filter
    {
        get => (IFilterControl)GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }

    public static BindableProperty IsSearchBarVisibleProperty = BindableProperty.Create(
        nameof(IsSearchBarVisible),
        typeof(bool),
        typeof(ExtSearchBar),
        defaultValue: default(bool),
        defaultBindingMode: BindingMode.TwoWay
    );

    public bool IsSearchBarVisible
    {
        get { return (bool)GetValue(IsSearchBarVisibleProperty); }
        set { SetValue(IsSearchBarVisibleProperty, value); }
    }

    #region HasFilterIcon 
    public static BindableProperty HasFilterIconProperty =
       BindableProperty.Create(
           nameof(HasFilterIcon),
           typeof(bool),
           typeof(ExtSearchBar),
           defaultValue: default(bool),
           defaultBindingMode: BindingMode.OneWay
       );

    public bool HasFilterIcon
    {
        get { return (bool)GetValue(HasFilterIconProperty); }
        set { SetValue(HasFilterIconProperty, value); }
    }
    #endregion

    #region Placeholder 
    public static BindableProperty PlaceholderProperty =
       BindableProperty.Create(
           nameof(Placeholder),
           typeof(string),
           typeof(ExtSearchBar),
           defaultValue: default(string),
           defaultBindingMode: BindingMode.OneWay
       );

    public string Placeholder
    {
        get { return (string)GetValue(PlaceholderProperty); }
        set { SetValue(PlaceholderProperty, value); }
    }
    #endregion

    #region Text 
    public static BindableProperty TextProperty =
       BindableProperty.Create(
           nameof(Text),
           typeof(string),
           typeof(SearchBar),
           defaultValue: default(string),
           defaultBindingMode: BindingMode.OneWayToSource,
           propertyChanged: OnTextChangedExternally
       );

    public string Text
    {
        get { return (string)GetValue(TextProperty); }
        set
        {
            if (!IsButtonClicked)
                SetValue(TextProperty, value);
        }
    }
    #endregion

    #region TextTag
    public static BindableProperty TextTagProperty =
       BindableProperty.Create(
           nameof(TextTag),
           typeof(string),
           typeof(SearchBar),
           defaultValue: default(string),
           defaultBindingMode: BindingMode.OneWayToSource
       );

    public string TextTag
    {
        get { return (string)GetValue(TextTagProperty); }
        set
        {
            SetValue(TextTagProperty, value);
        }
    }
    #endregion

    #region TextSearch
    public static BindableProperty TextSearchProperty =
       BindableProperty.Create(
           nameof(TextSearch),
           typeof(string),
           typeof(SearchBar),
           defaultValue: default(string),
           defaultBindingMode: BindingMode.OneWayToSource
       );

    public string TextSearch
    {
        get { return (string)GetValue(TextSearchProperty); }
        set
        {
            SetValue(TextSearchProperty, value);
        }
    }
    #endregion

    #region TextChangedCommand 
    public static BindableProperty TextChangedCommandProperty =
       BindableProperty.Create(
           nameof(TextChangedCommand),
           typeof(Command),
           typeof(SearchBar),
           defaultValue: default(Command),
           defaultBindingMode: BindingMode.OneWay
       );

    public Command TextChangedCommand
    {
        get { return (Command)GetValue(TextChangedCommandProperty); }
        set { SetValue(TextChangedCommandProperty, value); }
    }
    #endregion

    #region FilterCommand
    public static BindableProperty FilterCommandProperty =
        BindableProperty.Create(
            nameof(FilterCommand),
            typeof(ICommand),
            typeof(SearchBar),
            defaultValue: null,
            defaultBindingMode: BindingMode.OneWay
        );

    public ICommand FilterCommand
    {
        get { return (ICommand)GetValue(FilterCommandProperty); }
        set { SetValue(FilterCommandProperty, value); }
    }
    #endregion

    #region FilterCommandParameter
    public static BindableProperty FilterCommandParameterProperty =
        BindableProperty.Create(
            nameof(FilterCommandParameter),
            typeof(object),
            typeof(SearchBar),
            defaultValue: null,
            defaultBindingMode: BindingMode.OneWay
        );

    public object FilterCommandParameter
    {
        get { return (object)GetValue(FilterCommandParameterProperty); }
        set { SetValue(FilterCommandParameterProperty, value); }
    }
    #endregion

    #region SearchCommand 
    public static BindableProperty SearchCommandProperty =
       BindableProperty.Create(
           nameof(SearchCommand),
           typeof(Command),
           typeof(SearchBar),
           defaultValue: default(Command),
           defaultBindingMode: BindingMode.OneWay
       );

    public Command SearchCommand
    {
        get { return (Command)GetValue(SearchCommandProperty); }
        set { SetValue(SearchCommandProperty, value); }
    }
    #endregion

    #region IsCancelButtonVisible 
    public static BindableProperty IsCancelButtonVisibleProperty =
       BindableProperty.Create(
           nameof(IsCancelButtonVisible),
           typeof(bool),
           typeof(ExtSearchBar),
           defaultValue: false,
           defaultBindingMode: BindingMode.OneWay
       );

    public bool IsCancelButtonVisible
    {
        get { return (bool)GetValue(IsCancelButtonVisibleProperty); }
        set { SetValue(IsCancelButtonVisibleProperty, value); }
    }
    #endregion

    #endregion

    #region Methods
    private static void OnTextChangedExternally(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (ExtSearchBar)bindable;
        control.Text = (string)newValue;
    }
    public void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {

        Text = e.NewTextValue;
        if (IsButtonClicked)
        {
            TagsFilter.searchText_TextChanged(e.NewTextValue);
        }
        else
        {
            DebounceExecuteCommand(e.NewTextValue);
        }
    }

    private async void DebounceExecuteCommand(string newText)
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
        var token = cts.Token;

        try
        {
            var millisDelay = TextChangedDelay > 0 ? TextChangedDelay : 650;

            await Task.Delay(millisDelay, token);
            if (!token.IsCancellationRequested && TextChangedCommand?.CanExecute(newText) == true)
            {
                TextChangedCommand.Execute(newText);
            }
        }
        catch (TaskCanceledException) { }
    }

    public static void OnIsButtonClickedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as ExtSearchBar;

        if (newValue is bool isTagOpen)
        {
            if (isTagOpen)
            {
                obj.TextSearch = obj.searchText.Text;
                obj.searchText.Text = obj.TextTag;
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                    obj.TagsRect = new Rect(1, .27, .45, .85);
                else if (DeviceInfo.Platform == DevicePlatform.Android)
                    obj.TagsRect = new Rect(1, .0, .45, .85);
            }
            else
            {
                obj.TextTag = obj.searchText.Text;
                obj.searchText.Text = obj.TextSearch;
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                    obj.TagsRect = new Rect(1, .045, .45, .08);
                else if (DeviceInfo.Platform == DevicePlatform.Android)
                    obj.TagsRect = new Rect(1, .0, .45, .08);
            }
        }
    }

    private void IconFilter_Tapped(object sender, EventArgs e)
    {
        IsSearchBarVisible = true;
        FilterButton.IsVisible = CompanyFeatures.CompanyFeatSettings.TagsEnabled;
        HasFilterIcon = false;
        searchText.Text = Text;
        searchText.Focus();
        if (DeviceInfo.Platform == DevicePlatform.iOS)
            TagsRect = new Rect(1, .045, .44, .08);
        else if (DeviceInfo.Platform == DevicePlatform.Android)
            TagsRect = new Rect(1, .0, .45, .08);
        if (FilterCommand?.CanExecute(FilterCommandParameter) ?? false)
            FilterCommand?.Execute(FilterCommandParameter);
    }

    void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
    {
        IsButtonClicked = false;
        FilterButton.TextColor = ResourceHelper.GetApplicationResource<Color>("DarkGreyColor1");
        HasFilterIcon = true;
        if (DeviceInfo.Platform == DevicePlatform.iOS)
            TagsRect = new Rect(1, .01, .1, .08);
        else if (DeviceInfo.Platform == DevicePlatform.Android)
            TagsRect = new Rect(1, .0, .1, .08);
        Text = string.Empty;
        TextSearch = string.Empty;
        TextTag = string.Empty;
        searchText.Text = Text;
        searchText.Unfocus();
        IsSearchBarVisible = false;
        FilterButton.IsVisible = false;
    }
    #endregion
}

