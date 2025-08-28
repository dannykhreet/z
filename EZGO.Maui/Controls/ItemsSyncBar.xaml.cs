using System.Reflection.Metadata;
using System.Windows.Input;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Utils;

namespace EZGO.Maui.Controls;

public partial class ItemsSyncBar : Grid
{
	private static Color GreenColor => ResourceHelper.GetApplicationResource<Color>("GreenColor");

	private static Color NewAppDarkBlueColor => ResourceHelper.GetApplicationResource<Color>("NewAppDarkBlueColor");
	private static string SaveYourChangesText { get => TranslateExtension.GetValueFromDictionary("CHECKLIST_SAVE_CHANGES"); }
	private static string SyncYourProgressAndUpdateText { get => TranslateExtension.GetValueFromDictionary("CHECKLIST_SYNC_AND_UPDATE_CHANGES"); }
	private static string SaveYourProgressText { get => TranslateExtension.GetValueFromDictionary("CHECKLIST_SAVE_PROGRESS"); }
	private static string SeeUpdatesFromOthersText { get => TranslateExtension.GetValueFromDictionary("CHECKLIST_UPDATE_CHANGES"); }

	public static readonly BindableProperty SyncButtonCommandProperty = BindableProperty.Create(nameof(SyncButtonCommand), typeof(ICommand), typeof(ItemsSyncBar));

	public ICommand SyncButtonCommand
	{
		get => (ICommand)GetValue(SyncButtonCommandProperty);
		set
		{
			SetValue(SyncButtonCommandProperty, value);
			OnPropertyChanged();
		}
	}

	public static readonly BindableProperty AnyLocalChangesProperty = BindableProperty.Create(nameof(AnyLocalChanges), typeof(bool), typeof(ItemsSyncBar), propertyChanged: OnAnyLocalChanges);

	private static void OnAnyLocalChanges(BindableObject bindable, object oldValue, object newValue)
	{
		var control = bindable as ItemsSyncBar;
		if (control != null)
		{
			control.AnyLocalChanges = (bool)newValue;
		}
	}

	public bool AnyLocalChanges
	{
		get => (bool)GetValue(AnyLocalChangesProperty);
		set
		{
			SetValue(AnyLocalChangesProperty, value);
			SetBar();
			OnPropertyChanged();
		}
	}

	public static readonly BindableProperty AnyRemoteChangesProperty = BindableProperty.Create(nameof(AnyRemoteChanges), typeof(bool), typeof(ItemsSyncBar), propertyChanged: OnAnyRemoteChanges);

	public bool AnyRemoteChanges
	{
		get => (bool)GetValue(AnyRemoteChangesProperty);
		set
		{
			SetValue(AnyRemoteChangesProperty, value);
			SetBar();
			OnPropertyChanged();
		}
	}

	private static void OnAnyRemoteChanges(BindableObject bindable, object oldValue, object newValue)
	{
		var control = bindable as ItemsSyncBar;
		if (control != null)
		{
			control.AnyRemoteChanges = (bool)newValue;
		}
	}

	public static readonly BindableProperty FirstTimeSavingProperty = BindableProperty.Create(nameof(FirstTimeSaving), typeof(bool), typeof(ItemsSyncBar), propertyChanged: OnFirstTimeSavingChanges);

	public bool FirstTimeSaving
	{
		get => (bool)GetValue(FirstTimeSavingProperty);
		set
		{
			SetValue(FirstTimeSavingProperty, value);
			SetBar();
			OnPropertyChanged();
		}
	}

	private static void OnFirstTimeSavingChanges(BindableObject bindable, object oldValue, object newValue)
	{
		var control = bindable as ItemsSyncBar;
		if (control != null)
		{
			control.FirstTimeSaving = (bool)newValue;
		}
	}

	private void SetBar()
	{
		if (!CompanyFeatures.CompanyFeatSettings.ChecklistsTransferableEnabled)
		{
			SyncBar.IsVisible = false;
			return;
		}

		if (!Connectivity.NetworkAccess.Equals(NetworkAccess.Internet))
		{
			SyncBar.IsVisible = false;
			return;
		}

		if (!AnyRemoteChanges && !AnyLocalChanges)
		{
			SyncBar.IsVisible = false;
			return;
		}

		if (FirstTimeSaving)
		{
			SyncBar.BackgroundColor = GreenColor;
			SyncLabel.Text = SaveYourChangesText;
			SyncLabel.TextColor = Colors.White;
			SyncButton.TextColor = GreenColor;
			SyncButton.Background = Colors.White;
			SyncBar.IsVisible = true;
			return;
		}

		if (AnyLocalChanges)
		{
			if (AnyRemoteChanges)
			{
				SyncBar.BackgroundColor = NewAppDarkBlueColor;
				SyncLabel.Text = SyncYourProgressAndUpdateText;
				SyncLabel.TextColor = Colors.White;
				SyncButton.TextColor = NewAppDarkBlueColor;
				SyncButton.Background = Colors.White;
			}
			else
			{
				SyncBar.BackgroundColor = Colors.White;
				SyncLabel.Text = SaveYourProgressText;
				SyncLabel.TextColor = Colors.Black;
				SyncButton.TextColor = Colors.White;
				SyncButton.Background = GreenColor;
			}
		}
		else
		{
			if (AnyRemoteChanges)
			{
				SyncBar.BackgroundColor = Colors.White;
				SyncLabel.Text = SeeUpdatesFromOthersText;
				SyncLabel.TextColor = Colors.Black;
				SyncButton.TextColor = Colors.White;
				SyncButton.Background = NewAppDarkBlueColor;
			}
		}

		SyncBar.IsVisible = true;
	}

	public static readonly BindableProperty IsSyncingProperty = BindableProperty.Create(nameof(IsSyncing), typeof(bool), typeof(ItemsSyncBar));

	public bool IsSyncing
	{
		get => (bool)GetValue(IsSyncingProperty);
		set
		{
			SetValue(IsSyncingProperty, value);
			OnPropertyChanged();
		}
	}


	public ItemsSyncBar()
	{
		InitializeComponent();
		MessagingCenter.Subscribe<ItemsSyncBar>(this, Constants.NoUnsavedChanges, (sender) =>
		{
			AnyLocalChanges = false;
		});

	}
}