using System.Windows.Input;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.OpenFields;
#if ANDROID
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
#endif
using Microsoft.Maui.Layouts;

namespace EZGO.Maui.Controls.ChecklistsOpenFields;

public partial class OpenChecklistFields : ContentView
{

    public OpenChecklistFields()
    {
        InitializeComponent();
        SetTempalte();
        listView.HeaderSize = HeaderSize;
#if ANDROID
        Microsoft.Maui.Handlers.EntryHandler.Mapper.
        AppendToMapping("NoUnderline", (h, v)
         => h.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToAndroid()));
#endif
    }

    #region Bindable Properties

    public readonly static BindableProperty PropertyListProperty = BindableProperty.Create(
        nameof(PropertyList),
        typeof(List<UserValuesPropertyModel>),
        typeof(OpenChecklistFields),
        propertyChanged: OnPropertyValuesChanged);

    public readonly static BindableProperty SpanCountProperty = BindableProperty.Create(
        nameof(SpanCount),
        typeof(int),
        typeof(OpenChecklistFields),
        defaultValue: 1,
        propertyChanged: OnSpanCountChanged);

    public readonly static BindableProperty IsFromSlideViewProperty = BindableProperty.Create(
        nameof(IsFromSlideView),
        typeof(bool),
        typeof(OpenChecklistFields),
        defaultValue: false,
        propertyChanged: OnIsFromSlideViewPropertyChanged);

    public readonly static BindableProperty TapCommandProperty = BindableProperty.Create(
        nameof(TapCommand),
        typeof(ICommand),
        typeof(OpenChecklistFields));

    public int SpanCount
    {
        get => (int)GetValue(SpanCountProperty);
        set => SetValue(SpanCountProperty, value < 1 ? 1 : value);
    }

    public bool IsFromSlideView
    {
        get => (bool)GetValue(IsFromSlideViewProperty);
        set => SetValue(IsFromSlideViewProperty, value);
    }

    public static readonly BindableProperty CanMeasuredHeightProperty = BindableProperty.Create(nameof(CanMeasuredHeight), typeof(bool), typeof(OpenChecklistFields), defaultValue: false);
    public bool CanMeasuredHeight
    {
        get => (bool)GetValue(CanMeasuredHeightProperty);
        set => SetValue(CanMeasuredHeightProperty, value);
    }

    public ICommand TapCommand
    {
        get => (ICommand)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    public List<UserValuesPropertyModel> PropertyList
    {
        get => (List<UserValuesPropertyModel>)GetValue(PropertyListProperty);
        set
        {
            SetValue(PropertyListProperty, value);
            OnPropertyChanged();
        }
    }

    public static readonly BindableProperty ItemHeightProperty = BindableProperty.Create(nameof(ItemHeight), typeof(int), typeof(OpenChecklistFields), defaultValue: 60);

    public int ItemHeight
    {
        get => (int)GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    #endregion

    public double HeaderSize => IsFromSlideView ? 80 : 0;

    public bool HasProperties => PropertyList?.Any(x => x != null) ?? false;

    private static void OnSpanCountChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var openFields = (OpenChecklistFields)bindable;
        openFields.layoutManager.SpanCount = openFields.SpanCount;
    }

    private static void OnIsFromSlideViewPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var openFields = (OpenChecklistFields)bindable;
        openFields.listView.HeaderSize = openFields.HeaderSize;
        // TODO
        //openFields.enterValueLabel.IsVisible = openFields.IsFromSlideView;
    }

    private static void OnPropertyValuesChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var openFields = (OpenChecklistFields)bindable;
        CalculateOpenFieldsPositions(openFields);
        openFields.listView.ItemsSource = openFields.PropertyList;

        if (openFields.CanMeasuredHeight && openFields.PropertyList != null)
        {
            int propertyListAmount = openFields.PropertyList.Count;
            float checkListAmount = propertyListAmount % 2;
            if (checkListAmount != 0)
            {
                propertyListAmount += 1;
            }

            double heighteightRequest;
            //it is done like that because maui dosen't accept openFields.SpanCount-it's just dosen't work
            if (propertyListAmount > 2)
            {
                heighteightRequest = (openFields.ItemHeight + openFields.OpenFieldsSpacing + openFields.HeaderSize) * propertyListAmount / 2;
            }
            else
            {
                heighteightRequest = openFields.ItemHeight + openFields.OpenFieldsSpacing + openFields.HeaderSize;
            }
            openFields.listView.HeightRequest = heighteightRequest;
        }
    }

    private static void CalculateOpenFieldsPositions(OpenChecklistFields openFields)
    {
        const float maxTitlePercentage = 0.7f;

        if (openFields.PropertyList.IsNullOrEmpty() || openFields.listView.Width <= 0) return;

        if (openFields.IsCompleted)
        {
            openFields._maxTitleWidth = 0;

            openFields.PropertyList.ForEach(x =>
            {
                var titleWidth = x.IsRequired
                    ? openFields.CalculateBasis(x.Title + " *")
                    : openFields.CalculateBasis(x.Title);

                openFields.SetMaxTitleWitdh(titleWidth);
            });

            float widthT;
            if (openFields.openFieldsWidth > 0)
            {
                widthT = (float)(openFields._maxTitleWidth > openFields.listView.Width
                    ? 0.75f
                    : openFields._maxTitleWidth / openFields.listView.Width);

                float titlePercentage = Math.Clamp(widthT, 0f, maxTitlePercentage);
                float valuePercentage = 1f - titlePercentage;

                titlePercentage = double.IsNaN(titlePercentage) ? 0.7f : titlePercentage;
                valuePercentage = double.IsNaN(valuePercentage) ? 0.3f : valuePercentage;

                openFields.TitlePercentage = new FlexBasis(titlePercentage, true);
                openFields.ValuePercentage = new FlexBasis(valuePercentage, true);
                openFields.OnPropertyChanged();
            }
        }
        else
        {
            openFields._maxTitleWidth = 0;

            openFields.PropertyList.ForEach(x =>
            {
                var titleWidth = x.IsRequired
                    ? openFields.CalculateBasis(x.Title + " *")
                    : openFields.CalculateBasis(x.Title);

                openFields.SetMaxTitleWitdh(titleWidth);
            });

            float widthT;
            if (openFields.openFieldsWidth > 0)
            {
                widthT = (float)(openFields._maxTitleWidth > openFields.listView.Width
                    ? 0.8f
                    : openFields._maxTitleWidth / (openFields.listView.Width / openFields.SpanCount));

                if (widthT > 0.7f)
                    widthT = 0.7f;

                float multiplierSpanCount = openFields.SpanCount == 2 ? 0.2f : 0.45f;
                float countWidthPercentage = widthT * multiplierSpanCount;

                float titlePercentage = widthT + countWidthPercentage;

                titlePercentage = Math.Clamp(titlePercentage, 0f, maxTitlePercentage);
                float valuePercentage = 1f - titlePercentage;

                titlePercentage = double.IsNaN(titlePercentage) ? 0.7f : titlePercentage;
                valuePercentage = double.IsNaN(valuePercentage) ? 0.3f : valuePercentage;

                openFields.TitlePercentage = new FlexBasis(titlePercentage, true);
                openFields.ValuePercentage = new FlexBasis(valuePercentage, true);
                openFields.OnPropertyChanged();
            }
        }
    }

    public static readonly BindableProperty OpenFieldsSpacingProperty = BindableProperty.Create(nameof(OpenFieldsSpacing), typeof(double), typeof(OpenChecklistFields), defaultValue: 0d);

    public double OpenFieldsSpacing
    {
        get => (double)GetValue(OpenFieldsSpacingProperty);
        set => SetValue(OpenFieldsSpacingProperty, value);
    }

    public static readonly BindableProperty IsCompletedProperty = BindableProperty.Create(nameof(IsCompleted), typeof(bool), typeof(OpenChecklistFields), propertyChanged: OnIsCompletedPropertyChanged, defaultValue: false);

    private static void OnIsCompletedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as OpenChecklistFields;

        obj.SetTempalte();
    }

    public bool IsCompleted
    {
        get => (bool)GetValue(IsCompletedProperty);
        set
        {
            SetValue(IsCompletedProperty, value);
            OnPropertyChanged();
        }
    }

    private void SetTempalte()
    {
        DataTemplate currentTempalte;
        if (IsCompleted)
        {
            currentTempalte = (DataTemplate)Resources["CompletedFields"];
        }
        else
        {
            currentTempalte = (DataTemplate)Resources["OpenFieldsTemplate"];
        }

        listView.ItemTemplate = currentTempalte;
    }

    private double CalculateBasis(string text)
    {
        var textMeterService = DependencyService.Get<ITextMeter>();

        var result = textMeterService.MeasureTextSize(text, 20, "RobotoRegular");

        return result.Item1;
    }

    private void SetMaxTitleWitdh(double width)
    {
        if (width > _maxTitleWidth)
        {
            _maxTitleWidth = width;
        }
    }

    public static readonly BindableProperty TitlePercentageProperty = BindableProperty.Create(nameof(TitlePercentage), typeof(FlexBasis), typeof(OpenChecklistFields), defaultValue: new FlexBasis(0.75f, true));

    public FlexBasis TitlePercentage
    {
        get => (FlexBasis)GetValue(TitlePercentageProperty);
        set => SetValue(TitlePercentageProperty, value);
    }

    public static readonly BindableProperty ValuePercentageProperty = BindableProperty.Create(nameof(ValuePercentage), typeof(FlexBasis), typeof(OpenChecklistFields), defaultValue: new FlexBasis(0.25f, true));

    public FlexBasis ValuePercentage
    {
        get => (FlexBasis)GetValue(ValuePercentageProperty);
        set => SetValue(ValuePercentageProperty, value);
    }

    private double _maxTitleWidth = 0;

    private double openFieldsWidth;
    private double openFieldsHeight;

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (openFieldsHeight != height || openFieldsWidth != width)
        {
            openFieldsWidth = width;
            openFieldsHeight = height;

            CalculateOpenFieldsPositions(this);

        }
    }
}
