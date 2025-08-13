using System.Windows.Input;

namespace EZGO.Maui.Controls.DataTemplates;

public partial class ActionListTemplate : Grid
{
	public static readonly BindableProperty ActionSolvedCommandProperty = BindableProperty.Create(nameof(ActionSolvedCommand), typeof(ICommand), typeof(ActionListTemplate));

	public ICommand ActionSolvedCommand
	{
		get => (ICommand)GetValue(ActionSolvedCommandProperty);
		set => SetValue(ActionSolvedCommandProperty, value);
	}

	public static readonly BindableProperty NavigateToConversationCommandProperty = BindableProperty.Create(nameof(NavigateToConversationCommand), typeof(ICommand), typeof(ActionListTemplate));

	public ICommand NavigateToConversationCommand
	{
		get => (ICommand)GetValue(NavigateToConversationCommandProperty);
		set => SetValue(NavigateToConversationCommandProperty, value);
	}

	public ActionListTemplate()
	{
		InitializeComponent();
	}
}