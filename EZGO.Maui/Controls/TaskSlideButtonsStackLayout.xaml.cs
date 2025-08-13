using System.Windows.Input;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Classes;
using EZGO.Maui.Controls.Buttons;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Models.Audits;

namespace EZGO.Maui.Controls;

public partial class TaskSlideButtonsStackLayout : Grid
{
    public static readonly BindableProperty DeepLinkCommandProperty = BindableProperty.Create(
     nameof(DeepLinkCommand),
     typeof(ICommand),
     typeof(TaskSlideButtonsStackLayout));

    public static readonly BindableProperty OpenScoreCommandProperty = BindableProperty.Create(
     nameof(OpenScoreCommand),
     typeof(ICommand),
     typeof(TaskSlideButtonsStackLayout));

    public static readonly BindableProperty SkipButtonCommandProperty = BindableProperty.Create(
      nameof(SkipButtonCommand),
      typeof(ICommand),
      typeof(TaskSlideButtonsStackLayout));

    public static readonly BindableProperty StepsCommandProperty = BindableProperty.Create(
      nameof(StepsCommand),
      typeof(ICommand),
      typeof(TaskSlideButtonsStackLayout));

    public static readonly BindableProperty ActionButtonCommandProperty = BindableProperty.Create(
    nameof(ActionButtonCommand),
    typeof(ICommand),
    typeof(TaskSlideButtonsStackLayout));

    public static readonly BindableProperty ThumbsUpButtonCommandProperty = BindableProperty.Create(
        nameof(ThumbsUpButtonCommand),
        typeof(ICommand),
        typeof(TaskSlideButtonsStackLayout));

    public static readonly BindableProperty ThumbsDownButtonCommandProperty = BindableProperty.Create(
      nameof(ThumbsDownButtonCommand),
      typeof(ICommand),
      typeof(TaskSlideButtonsStackLayout));

    public static readonly BindableProperty TaskStatusProperty = BindableProperty.Create(
     nameof(TaskStatus),
     typeof(TaskStatusEnum),
     typeof(TaskSlideButtonsStackLayout));

    public static readonly BindableProperty ScoreTypeProperty = BindableProperty.Create(
    nameof(ScoreType),
    typeof(ScoreTypeEnum),
    typeof(TaskSlideButtonsStackLayout));

    public static readonly BindableProperty ActionBubbleCountProperty = BindableProperty.Create(
   nameof(ActionBubbleCount),
   typeof(int),
   typeof(TaskSlideButtonsStackLayout));

    public static readonly BindableProperty HasExtraInformationProperty = BindableProperty.Create(
   nameof(HasExtraInformation),
   typeof(bool),
   typeof(TaskSlideButtonsStackLayout));

    public static readonly BindableProperty DeeplinkButtonVisibleProperty = BindableProperty.Create(
  nameof(DeeplinkButtonVisible),
  typeof(bool),
  typeof(TaskSlideButtonsStackLayout), defaultValue: false);

    public static readonly BindableProperty HasStepsOrWorkInstructionsProperty = BindableProperty.Create(
 nameof(HasStepsOrWorkInstructions),
 typeof(bool),
 typeof(TaskSlideButtonsStackLayout));

    public static readonly BindableProperty ScoreProperty = BindableProperty.Create(
  nameof(Score),
  typeof(int?),
  typeof(TaskSlideButtonsStackLayout),
  defaultValue: null);

    public static readonly BindableProperty ColorCalculatorProperty = BindableProperty.Create(
  nameof(ColorCalculator),
  typeof(IScoreColorCalculator),
  typeof(TaskSlideButtonsStackLayout),
  defaultValue: null);

    public static readonly BindableProperty HasPictureProofProperty = BindableProperty.Create(
nameof(HasPictureProof),
typeof(bool),
typeof(TaskSlideButtonsStackLayout));

    public static readonly BindableProperty IsThumbsUpBadgeVisibleProperty = BindableProperty.Create(
nameof(IsThumbsUpBadgeVisible),
typeof(bool),
typeof(TaskSlideButtonsStackLayout));

    public static readonly BindableProperty IsThumbsDownBadgeVisibleProperty = BindableProperty.Create(
nameof(IsThumbsDownBadgeVisible),
typeof(bool),
typeof(TaskSlideButtonsStackLayout));

    public static readonly BindableProperty IsScoreBadgeVisibleProperty = BindableProperty.Create(
nameof(IsScoreBadgeVisible),
typeof(bool),
typeof(TaskSlideButtonsStackLayout), defaultValue: false);


    public static readonly BindableProperty IsDeepLinkValidProperty = BindableProperty.Create(
nameof(IsDeepLinkValid),
typeof(bool),
typeof(TaskSlideButtonsStackLayout), defaultValue: true);

    public static readonly BindableProperty DeepLinkCompletionIsRequiredProperty = BindableProperty.Create(
nameof(DeepLinkCompletionIsRequired),
typeof(bool),
typeof(TaskSlideButtonsStackLayout), defaultValue: false);

    public bool DeepLinkCompletionIsRequired
    {
        get => (bool)GetValue(DeepLinkCompletionIsRequiredProperty);
        set
        {
            SetValue(DeepLinkCompletionIsRequiredProperty, value);
            OnPropertyChanged();
        }
    }

    public static readonly BindableProperty CompletedDeeplinkIdProperty = BindableProperty.Create(
nameof(CompletedDeeplinkId),
typeof(int?),
typeof(TaskSlideButtonsStackLayout), defaultValue: null);

    public int? CompletedDeeplinkId
    {
        get => (int?)GetValue(CompletedDeeplinkIdProperty);
        set
        {
            SetValue(CompletedDeeplinkIdProperty, value);
            OnPropertyChanged();
        }
    }

    public static readonly BindableProperty IsBlockedProperty = BindableProperty.Create(
nameof(IsBlocked),
typeof(bool),
typeof(TaskSlideButtonsStackLayout));

    public bool IsBlocked
    {
        get => (bool)GetValue(IsBlockedProperty);
        set
        {
            SetValue(IsBlockedProperty, value);
            OnPropertyChanged();
        }
    }


    public bool IsDeepLinkValid
    {
        get => (bool)GetValue(IsDeepLinkValidProperty);
        set
        {
            SetValue(IsDeepLinkValidProperty, value);
            OnPropertyChanged();
        }
    }

    public bool IsScoreBadgeVisible
    {
        get => (bool)GetValue(IsScoreBadgeVisibleProperty);
        set
        {
            SetValue(IsScoreBadgeVisibleProperty, value);
            OnPropertyChanged();
        }
    }

    public bool IsThumbsUpBadgeVisible
    {
        get => (bool)GetValue(IsThumbsUpBadgeVisibleProperty);
        set
        {
            SetValue(IsThumbsUpBadgeVisibleProperty, value);
            OnPropertyChanged();
        }
    }

    public bool IsThumbsDownBadgeVisible
    {
        get => (bool)GetValue(IsThumbsDownBadgeVisibleProperty);
        set
        {
            SetValue(IsThumbsDownBadgeVisibleProperty, value);
            OnPropertyChanged();
        }
    }


    public bool HasPictureProof
    {
        get => (bool)GetValue(HasPictureProofProperty);
        set
        {
            SetValue(HasPictureProofProperty, value);
            OnPropertyChanged();
        }
    }

    public IScoreColorCalculator ColorCalculator
    {
        get => (IScoreColorCalculator)GetValue(ColorCalculatorProperty);
        set
        {
            SetValue(ColorCalculatorProperty, value);
            OnPropertyChanged();
        }
    }

    public int? Score
    {
        get => (int?)GetValue(ScoreProperty);
        set
        {
            SetValue(ScoreProperty, value);
            OnPropertyChanged();
        }
    }

    public bool HasExtraInformation
    {
        get => (bool)GetValue(HasExtraInformationProperty);
        set
        {
            SetValue(HasExtraInformationProperty, value);
            OnPropertyChanged();
        }
    }

    public bool HasStepsOrWorkInstructions
    {
        get => (bool)GetValue(HasStepsOrWorkInstructionsProperty);
        set
        {
            SetValue(HasStepsOrWorkInstructionsProperty, value);
            OnPropertyChanged();
        }
    }

    public bool DeeplinkButtonVisible
    {
        get => (bool)GetValue(DeeplinkButtonVisibleProperty);
        set
        {
            SetValue(DeeplinkButtonVisibleProperty, value);
            OnPropertyChanged();
        }
    }


    public int ActionBubbleCount
    {
        get => (int)GetValue(ActionBubbleCountProperty);
        set
        {
            SetValue(ActionBubbleCountProperty, value);
            OnPropertyChanged();
        }
    }


    public TaskStatusEnum TaskStatus
    {
        get => (TaskStatusEnum)GetValue(TaskStatusProperty);
        set
        {
            SetValue(TaskStatusProperty, value);
            OnPropertyChanged();
        }
    }


    public ScoreTypeEnum ScoreType
    {
        get => (ScoreTypeEnum)GetValue(ScoreTypeProperty);
        set
        {
            SetValue(ScoreTypeProperty, value);
            OnPropertyChanged();
        }
    }


    public ICommand SkipButtonCommand
    {
        get => (ICommand)GetValue(SkipButtonCommandProperty);
        set
        {
            SetValue(SkipButtonCommandProperty, value);
            OnPropertyChanged();
        }
    }

    public ICommand ThumbsUpButtonCommand
    {
        get => (ICommand)GetValue(ThumbsUpButtonCommandProperty);
        set
        {
            SetValue(ThumbsUpButtonCommandProperty, value);
            OnPropertyChanged();
        }
    }

    public ICommand ThumbsDownButtonCommand
    {
        get => (ICommand)GetValue(ThumbsDownButtonCommandProperty);
        set
        {
            SetValue(ThumbsDownButtonCommandProperty, value);
            OnPropertyChanged();
        }
    }

    public ICommand ActionButtonCommand
    {
        get => (ICommand)GetValue(ActionButtonCommandProperty);
        set
        {
            SetValue(ActionButtonCommandProperty, value);
            OnPropertyChanged();
        }
    }

    public ICommand StepsCommand
    {
        get => (ICommand)GetValue(StepsCommandProperty);
        set
        {
            SetValue(StepsCommandProperty, value);
            OnPropertyChanged();
        }
    }

    public ICommand OpenScoreCommand
    {
        get => (ICommand)GetValue(OpenScoreCommandProperty);
        set
        {
            SetValue(OpenScoreCommandProperty, value);
            OnPropertyChanged();
        }
    }

    public ICommand DeepLinkCommand
    {
        get => (ICommand)GetValue(DeepLinkCommandProperty);
        set
        {
            SetValue(DeepLinkCommandProperty, value);
            OnPropertyChanged();
        }
    }

    public TaskSlideButtonsStackLayout()
    {
        InitializeComponent();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);


        //Setting up picture proof indicators
        var lineWidth = 2;
        var verticalLineHeight = 20;
        var margin = 5;

        var largeRoundButtonSize = 65;
        var cameraIconWidth = 22;

        var xMargin = largeRoundButtonSize / 2;
        var yMargin = largeRoundButtonSize + 5;
        thumbsDownVerticalBoxView.HeightRequest = verticalLineHeight;
        thumbsUpVerticalBoxView.HeightRequest = verticalLineHeight;
        thumbsDownVerticalBoxView.WidthRequest = lineWidth;
        thumbsUpVerticalBoxView.WidthRequest = lineWidth;

        thumbsDownVerticalBoxView.TranslationX = xMargin;
        thumbsDownVerticalBoxView.TranslationY = yMargin;
        thumbsUpVerticalBoxView.TranslationX = xMargin;
        thumbsUpVerticalBoxView.TranslationY = yMargin;

        if (Settings.IsRightToLeftLanguage)
        {
            thumbsDownVerticalBoxView.TranslationX = -thumbsDownVerticalBoxView.TranslationX - margin;
            thumbsUpVerticalBoxView.TranslationX = -thumbsUpVerticalBoxView.TranslationX - margin;
        }

        thumbsUpHorizontalBoxView.HeightRequest = lineWidth;
        thumbsUpHorizontalBoxView.WidthRequest = thumbsUpButton.WidthRequest * 0.75;
        thumbsUpHorizontalBoxView.TranslationX = -thumbsUpButton.WidthRequest * 0.25;
        thumbsUpHorizontalBoxView.TranslationY = thumbsUpVerticalBoxView.TranslationY + thumbsUpVerticalBoxView.HeightRequest - lineWidth;

        thumbsDownHorizontalBoxView.HeightRequest = lineWidth;
        thumbsDownHorizontalBoxView.WidthRequest = thumbsDownButton.WidthRequest * 0.75;
        thumbsDownHorizontalBoxView.TranslationX = thumbsDownButton.WidthRequest / 2;
        thumbsDownHorizontalBoxView.TranslationY = thumbsDownVerticalBoxView.TranslationY + thumbsDownVerticalBoxView.HeightRequest - lineWidth;

        if (Settings.IsRightToLeftLanguage)
        {
            thumbsUpHorizontalBoxView.TranslationX = -thumbsUpHorizontalBoxView.TranslationX - lineWidth - margin;
            thumbsDownHorizontalBoxView.TranslationX = -thumbsDownHorizontalBoxView.TranslationX - lineWidth - margin;
        }

        icon.TranslationY = thumbsDownVerticalBoxView.TranslationY + thumbsDownVerticalBoxView.HeightRequest - lineWidth - cameraIconWidth / 2;
        icon.TranslationX = actionButton.WidthRequest / 2 - cameraIconWidth / 2;

        if (Settings.IsRightToLeftLanguage)
        {
            icon.TranslationX = -icon.TranslationX - lineWidth - margin;
        }
    }

    public ScoreButton GetScoreButton => scoreButton;

    public ActionButton GetActionButton => actionButton;
}
