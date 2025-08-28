using System.Windows.Input;
using EZGO.Maui.Core.Classes.Filters;

namespace EZGO.Maui.Controls.Filters;

public partial class SelectedTagsList : Grid
{
	public SelectedTagsList()
	{
		InitializeComponent();
	}

	public static BindableProperty FilterProperty = BindableProperty.Create(nameof(Filter), typeof(IFilterControl), declaringType: typeof(TagsFilterSection));

	public IFilterControl Filter
	{
		get => (IFilterControl)GetValue(FilterProperty);
		set => SetValue(FilterProperty, value);
	}

	public static readonly BindableProperty DeleteTagCommandProperty = BindableProperty.Create(nameof(DeleteTagCommand), typeof(ICommand), typeof(TagsFilterSection));

	public ICommand DeleteTagCommand
	{
		get => (ICommand)GetValue(DeleteTagCommandProperty);
		set => SetValue(DeleteTagCommandProperty, value);
	}
}