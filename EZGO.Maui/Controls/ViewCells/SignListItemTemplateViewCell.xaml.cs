using System.Globalization;
using System.Windows.Input;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.DateFormats;
using EZGO.Maui.Core.Classes.Stages;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Models.Stages;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels;
using EZGO.Maui.Core.ViewModels.Shared;

namespace EZGO.Maui.Controls.ViewCells;

public partial class SignListItemTemplateViewCell : Grid
{
	public StageTemplateModel CurrentStage { get; set; }

	public static readonly BindableProperty IsListVisibleProperty = BindableProperty.Create(nameof(IsListVisible), typeof(bool), typeof(SignListItemTemplateViewCell), propertyChanged: OnIsListVisiblePropertyChanged);

	public bool IsListVisible
	{
		get => (bool)GetValue(IsListVisibleProperty);
		set => SetValue(IsListVisibleProperty, value);
	}

	public static readonly BindableProperty SaveStageCommandProperty = BindableProperty.Create(nameof(SaveStageCommand), typeof(ICommand), typeof(SignListItemTemplateViewCell));

	public ICommand SaveStageCommand
	{
		get => (ICommand)GetValue(SaveStageCommandProperty);
		set => SetValue(SaveStageCommandProperty, value);
	}

	public static readonly BindableProperty StageTemplateIdProperty = BindableProperty.Create(nameof(StageTemplateId), typeof(int?), typeof(SignListItemTemplateViewCell), propertyChanged: OnStageIdPropertyChanged);

	public int? StageTemplateId
	{
		get => (int?)GetValue(StageTemplateIdProperty);
		set => SetValue(StageTemplateIdProperty, value);
	}

	private static void OnStageIdPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var obj = bindable as SignListItemTemplateViewCell;
		obj.CurrentStage = obj.Stages?.GetStageTemplate(obj.StageTemplateId);

		obj.SetSignedGrid();
	}

	private static void OnIsListVisiblePropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var obj = bindable as SignListItemTemplateViewCell;
		obj.CurrentStage = obj.Stages?.GetStageTemplate(obj.StageTemplateId);

		obj.SetSignedGrid();
	}

	private void SetSignedGrid()
	{
		MainThread.BeginInvokeOnMainThread(() =>
		{
			if (CurrentStage != null)
			{
				if (CurrentStage.IsLocked || IsSyncing || !CurrentStage.IsCompleted)
				{
					BackgroundBox.BackgroundColor = ResourceHelper.GetApplicationResource<Color>("LightGreyColor");
					SignStageLabel.TextColor = ResourceHelper.GetApplicationResource<Color>("StageGreyColor");
				}
				else
				{
					BackgroundBox.BackgroundColor = ResourceHelper.GetApplicationResource<Color>("GreenColor");
					SignStageLabel.TextColor = ResourceHelper.GetApplicationResource<Color>("WhiteColor");
				}

				if (CurrentStage.Signatures != null && CurrentStage.Signatures.Count > 0)
				{
					var signature = CurrentStage.Signatures[0];
					StageLabel.Text = CurrentStage.Name;
					DateLabel.Text = signature.SignedAt?.ToLocalTime().ToString(BaseDateFormats.DateTimeMonthShortNameFormat, CultureInfo.CurrentUICulture);
					SignedByLabel.Text = signature.SignedBy;
					UserImage.UserId = (int)signature.SignedById;
					Signed.IsVisible = true;
					NotYetSigned.IsVisible = false;
					CheckIcon.IsVisible = true;
					if (CurrentStage.Signatures.Count == 2)
					{
						UserImage2.UserId = (int)CurrentStage.Signatures[1].SignedById;
						UserImage2.IsVisible = true;
						SignedByLabel.Text += $" {TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextAnd)} {CurrentStage.Signatures[1].SignedBy}";
					}

					NotesIcon.IsVisible = !CurrentStage.ShiftNotes.IsNullOrEmpty();
					return;
				}
			}
			Signed.IsVisible = false;
			NotYetSigned.IsVisible = true;
		});
	}

	public static readonly BindableProperty StagesProperty = BindableProperty.Create(nameof(Stages), typeof(StagesControl), typeof(SignListItemTemplateViewCell), propertyChanged: OnStagesPropertyChanged);

	public StagesControl Stages
	{
		get => (StagesControl)GetValue(StagesProperty);
		set => SetValue(StagesProperty, value);
	}

	private static void OnStagesPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var obj = bindable as SignListItemTemplateViewCell;
		obj.CurrentStage = obj.Stages?.GetStageTemplate(obj.StageTemplateId);

		obj.SetSignedGrid();
	}

	public SignListItemTemplateViewCell()
	{
		InitializeComponent();
		SetSignedGrid();

		MessagingCenter.Subscribe<ChecklistSlideViewModel>(this, Constants.RecalculateAmountsMessage, (sender) =>
		{
			SetSignedGrid();
		});

		MessagingCenter.Subscribe<StagesControl>(this, Constants.TasksChanged, (sender) =>
		{
			SetSignedGrid();
		});

		MessagingCenter.Subscribe<StageSignViewModel>(this, Constants.StageSigned, (sender) =>
		{
			SetSignedGrid();
		});
	}

	void SignItemTemplate_Clicked(System.Object sender, System.EventArgs e)
	{
		var obj = sender as SignListItemTemplateViewCell;
		obj.SetSignedGrid();
	}


	public static readonly BindableProperty IsSyncingProperty = BindableProperty.Create(nameof(IsSyncing), typeof(bool), typeof(SignListItemTemplateViewCell), propertyChanged: OnIsSyncingPropertyChanged);

	public bool IsSyncing
	{
		get => (bool)GetValue(IsSyncingProperty);
		set
		{
			SetValue(IsSyncingProperty, value);
			OnPropertyChanged();
		}
	}

	private static void OnIsSyncingPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var obj = bindable as SignListItemTemplateViewCell;
		obj.IsSyncing = (bool)newValue;
		obj.SetSignedGrid();
	}
}