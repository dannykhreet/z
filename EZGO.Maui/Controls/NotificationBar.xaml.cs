using System.Windows.Input;

namespace EZGO.Maui.Controls;

public partial class NotificationBar : Grid
{
	public static readonly BindableProperty NotificationButtonCommandProperty = BindableProperty.Create(nameof(NotificationButtonCommand), typeof(ICommand), typeof(NotificationBar));

	public ICommand NotificationButtonCommand
	{
		get => (ICommand)GetValue(NotificationButtonCommandProperty);
		set
		{
			SetValue(NotificationButtonCommandProperty, value);
			OnPropertyChanged();
		}
	}

	public static readonly BindableProperty HasUnreadMessagesProperty = BindableProperty.Create(nameof(HasUnreadMessages), typeof(bool), typeof(NotificationBar), propertyChanged: OnHasUnreadMessagesPropertyChanged);

	private static void OnHasUnreadMessagesPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (newValue is bool hasUnreadChanges)
		{
			var obj = bindable as NotificationBar;
			obj.NotificationBarPage.IsVisible = hasUnreadChanges;
		}
	}

	public bool HasUnreadMessages
	{
		get => (bool)GetValue(HasUnreadMessagesProperty);
		set => SetValue(HasUnreadMessagesProperty, value);
	}


	public NotificationBar()
	{
		InitializeComponent();
	}
}