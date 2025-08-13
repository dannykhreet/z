using System.Windows.Input;
using EZGO.Maui.Classes;
using EZGO.Maui.Core.Models.Audits;

namespace EZGO.Maui.Controls.DataTemplates;

public partial class GridItemTemplate : Grid
{
	#region SkipCommand

    public static readonly BindableProperty SkipCommandProperty = BindableProperty.Create(nameof(SkipCommand), typeof(ICommand), typeof(GridItemTemplate));

    public ICommand SkipCommand
    {
        get => (ICommand)GetValue(SkipCommandProperty);
        set => SetValue(SkipCommandProperty, value);
    }

    #endregion

    #region OkCommand

    public static readonly BindableProperty OkCommandProperty = BindableProperty.Create(nameof(OkCommand), typeof(ICommand), typeof(GridItemTemplate));

    public ICommand OkCommand
    {
        get => (ICommand)GetValue(OkCommandProperty);
        set => SetValue(OkCommandProperty, value);
    }

    #endregion

    #region NotOkCommand

    public static readonly BindableProperty NotOkCommandProperty = BindableProperty.Create(nameof(NotOkCommand), typeof(ICommand), typeof(GridItemTemplate));


    public ICommand NotOkCommand
    {
        get => (ICommand)GetValue(NotOkCommandProperty);
        set => SetValue(NotOkCommandProperty, value);
    }

    #endregion

    #region ActionCommand

    public static readonly BindableProperty ActionCommandProperty = BindableProperty.Create(nameof(ActionCommand), typeof(ICommand), typeof(GridItemTemplate));

    public ICommand ActionCommand
    {
        get => (ICommand)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    #endregion

    #region DeepLinkCommand

    public static readonly BindableProperty DeepLinkCommandProperty = BindableProperty.Create(nameof(DeepLinkCommand), typeof(ICommand), typeof(GridItemTemplate));

    public ICommand DeepLinkCommand
    {
        get => (ICommand)GetValue(DeepLinkCommandProperty);
        set => SetValue(DeepLinkCommandProperty, value);
    }

    public static readonly BindableProperty DetailCommandProperty = BindableProperty.Create(nameof(DetailCommand), typeof(ICommand), typeof(GridItemTemplate));

    public ICommand DetailCommand
    {
        get => (ICommand)GetValue(DetailCommandProperty);
        set => SetValue(DetailCommandProperty, value);
    }

    public static readonly BindableProperty AreAdditionalButtonsVisibleProperty = BindableProperty.Create(nameof(AreAdditionalButtonsVisible), typeof(bool), typeof(GridItemTemplate), defaultValue: true, propertyChanged: OnAreAdditionalButtonsVisible);

    private static void OnAreAdditionalButtonsVisible(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as GridItemTemplate;
        obj.RecalculateViewElementsPositions();
    }

    public bool AreAdditionalButtonsVisible
    {
        get => (bool)GetValue(AreAdditionalButtonsVisibleProperty);
        set { SetValue(AreAdditionalButtonsVisibleProperty, value); OnPropertyChanged(); }
    }

    #endregion

    #region Score

    public static readonly BindableProperty OpenScorePopupProperty = BindableProperty.Create(nameof(OpenScorePopup), typeof(ICommand), typeof(GridItemTemplate));

    public ICommand OpenScorePopup
    {
        get => (ICommand)GetValue(OpenScorePopupProperty);
        set => SetValue(OpenScorePopupProperty, value);
    }

    public static readonly BindableProperty IsScoreButtonVisibleProperty = BindableProperty.Create(nameof(IsScoreButtonVisible), typeof(bool), typeof(GridItemTemplate));

    public bool IsScoreButtonVisible
    {
        get => (bool)GetValue(IsScoreButtonVisibleProperty);
        set => SetValue(IsScoreButtonVisibleProperty, value);
    }

    public static readonly BindableProperty ScoreColorCalculatorProperty = BindableProperty.Create(nameof(ScoreColorCalculator), typeof(IScoreColorCalculator), typeof(GridItemTemplate));

    public IScoreColorCalculator ScoreColorCalculator
    {
        get => (IScoreColorCalculator)GetValue(ScoreColorCalculatorProperty);
        set => SetValue(ScoreColorCalculatorProperty, value);
    }

    #endregion

    public GridItemTemplate()
    {
        InitializeComponent();
        DrawPictureProofIndicators();
    }

    public void DrawPictureProofIndicators()
    {
        var result = double.TryParse(ResourceHelper.GetValueFromResources<string>("RoundButtonSize"), out double size);
        if (!result)
            return;

        var lineWidth = 2;
        var verticalLineHeight = 15;
        var margin = 5;

        thumbsDownVerticalBoxView.WidthRequest = lineWidth;
        thumbsDownVerticalBoxView.HeightRequest = verticalLineHeight;
        thumbsDownVerticalBoxView.TranslationX = size / 2;
        thumbsDownVerticalBoxView.TranslationY = size + 5;

        thumbsUpVerticalBoxView.HeightRequest = verticalLineHeight;
        thumbsUpVerticalBoxView.WidthRequest = lineWidth;
        thumbsUpVerticalBoxView.TranslationX = size / 2;
        thumbsUpVerticalBoxView.TranslationY = size + 5;

        if (Core.Classes.Settings.IsRightToLeftLanguage)
        {
            thumbsDownVerticalBoxView.TranslationX = -thumbsDownVerticalBoxView.TranslationX - margin;
            thumbsUpVerticalBoxView.TranslationX = -thumbsUpVerticalBoxView.TranslationX - margin;
        }

        thumbsDownHorizontalBoxView.HeightRequest = lineWidth;
        thumbsDownHorizontalBoxView.WidthRequest = size * 0.33;
        thumbsDownHorizontalBoxView.TranslationX = size / 2;
        thumbsDownHorizontalBoxView.TranslationY = thumbsDownVerticalBoxView.TranslationY + thumbsDownVerticalBoxView.HeightRequest - lineWidth;

        thumbsUpHorizontalBoxView.HeightRequest = lineWidth;
        thumbsUpHorizontalBoxView.WidthRequest = size * 0.33;
        thumbsUpHorizontalBoxView.TranslationX = (size / 2) - thumbsUpHorizontalBoxView.WidthRequest + lineWidth;
        thumbsUpHorizontalBoxView.TranslationY = thumbsUpVerticalBoxView.TranslationY + thumbsUpVerticalBoxView.HeightRequest - lineWidth;

        if (Core.Classes.Settings.IsRightToLeftLanguage)
        {
            thumbsUpHorizontalBoxView.TranslationX = -thumbsUpHorizontalBoxView.TranslationX - margin;
            thumbsDownHorizontalBoxView.TranslationX = -thumbsDownHorizontalBoxView.TranslationX - lineWidth - margin;
        }

        icon.TranslationY = thumbsDownVerticalBoxView.TranslationY + thumbsDownVerticalBoxView.HeightRequest - lineWidth - icon.FontSize / 2;
        icon.TranslationX = thumbsDownVerticalBoxView.TranslationX + thumbsDownHorizontalBoxView.WidthRequest + icon.FontSize / 2;

        if (Core.Classes.Settings.IsRightToLeftLanguage)
        {
            icon.TranslationX = thumbsDownVerticalBoxView.TranslationX - thumbsDownHorizontalBoxView.WidthRequest - icon.FontSize / 2;
        }
    }

    public void RecalculateViewElementsPositions()
    {
        MachineStatus.IsVisible = !AreAdditionalButtonsVisible;
        DeepLinkButton.IsVisible = AreAdditionalButtonsVisible;
        ActionButton.IsVisible = AreAdditionalButtonsVisible;
    }
}