using EZGO.Api.Models.Tags;
using EZGO.Maui.Core.Classes;

namespace EZGO.Maui.Controls;

public partial class StageHeader : StackLayout
{
	public static readonly BindableProperty StageNameProperty = BindableProperty.Create(nameof(StageName), typeof(string), typeof(StageHeader));

	public string StageName
	{
		get => (string)GetValue(StageNameProperty);
		set => SetValue(StageNameProperty, value);
	}

	public static readonly BindableProperty IconNameProperty = BindableProperty.Create(nameof(IconName), typeof(string), typeof(StageHeader));

	public string IconName
	{
		get => (string)GetValue(IconNameProperty);
		set => SetValue(IconNameProperty, value);
	}

	public static readonly BindableProperty IsSignedProperty = BindableProperty.Create(nameof(IsSigned), typeof(bool), typeof(StageHeader), propertyChanged: OnIsSignedOrLockedPropertyChanged);

	public bool IsSigned
	{
		get => (bool)GetValue(IsSignedProperty);
		set => SetValue(IsSignedProperty, value);
	}

	public static readonly BindableProperty IsSlideViewProperty = BindableProperty.Create(nameof(IsSlideView), typeof(bool), typeof(StageHeader), defaultValue: false);

	public bool IsSlideView
	{
		get => (bool)GetValue(IsSlideViewProperty);
		set => SetValue(IsSlideViewProperty, value);
	}

	private static void OnIsSignedOrLockedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var obj = bindable as StageHeader;

		obj.SetBackgroundColorAndIcon();
	}

	public static readonly BindableProperty IsLockedProperty = BindableProperty.Create(nameof(IsLocked), typeof(bool), typeof(StageHeader), propertyChanged: OnIsSignedOrLockedPropertyChanged);

	public bool IsLocked
	{
		get => (bool)GetValue(IsLockedProperty);
		set => SetValue(IsLockedProperty, value);
	}

	public static readonly BindableProperty TagsProperty = BindableProperty.Create(nameof(Tags), typeof(List<Tag>), typeof(StageHeader), propertyChanged: OnTagsPropertyChanged);

	private static void OnTagsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (newValue is List<Tag> tags)
		{
			var obj = bindable as StageHeader;
			obj.Tags = tags;
			obj.OnPropertyChanged(nameof(Tags));
		}
	}

	public List<Tag> Tags
	{
		get => (List<Tag>)GetValue(TagsProperty);
		set => SetValue(TagsProperty, value);
	}

	private void SetBackgroundColorAndIcon()
	{
		MainThread.BeginInvokeOnMainThread(() =>
		{
			bool isHeaderColor = true;

			if (IsSigned)
			{
				if (isHeaderColor)
					BackgroundColor = ResourceHelper.GetApplicationResource<Color>("GreenColor");
				else
					BackgroundColor = ResourceHelper.GetApplicationResource<Color>("LightGreenColor");

				TagsList.BackgroundColor = ResourceHelper.GetApplicationResource<Color>("LightGreenColor");
			}
			else if (IsLocked)
			{
				if (isHeaderColor)
					BackgroundColor = ResourceHelper.GetApplicationResource<Color>("DarkerGreyColor");
				else
					BackgroundColor = ResourceHelper.GetApplicationResource<Color>("LightGreyColor");

				TagsList.BackgroundColor = ResourceHelper.GetApplicationResource<Color>("LightGreyColor");
			}
			else
			{
				if (isHeaderColor)
					BackgroundColor = ResourceHelper.GetApplicationResource<Color>("DarkBlueColor");
				else
					BackgroundColor = ResourceHelper.GetApplicationResource<Color>("LightBlueColor");

				TagsList.BackgroundColor = ResourceHelper.GetApplicationResource<Color>("LightBlueColor");
			}

			if (IsSigned)
			{
				IconName = Classes.IconFont.Signature;
			}
			else if (IsLocked)
			{
				IconName = Classes.IconFont.StageLock;
			}
			else
			{
				IconName = Classes.IconFont.LightbulbOn;
			}
		});
	}

	public StageHeader()
	{
		InitializeComponent();
		SetBackgroundColorAndIcon();
	}
}