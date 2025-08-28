using System.ComponentModel;
using System.Windows.Input;
using EZGO.Api.Models.WorkInstructions;
using Syncfusion.Maui.Popup;

namespace EZGO.Maui.Controls.Popup;

public partial class ShowChangesPopupView : SfPopup, INotifyPropertyChanged
{
	private WorkInstructionTemplateChangeNotification selectedChange;
	public WorkInstructionTemplateChangeNotification SelectedChange { get => selectedChange; set { selectedChange = value; OnPropertyChanged(); } }

	public ShowChangesPopupView()
	{
		InitializeComponent();
	}

	public static readonly BindableProperty ChangesInProperty = BindableProperty.Create(nameof(ChangesIn), typeof(string), typeof(ShowChangesPopupView));

	public string ChangesIn
	{
		get => (string)GetValue(ChangesInProperty);
		set
		{
			SetValue(ChangesInProperty, value);
			OnPropertyChanged();
		}
	}


	public static readonly BindableProperty ChangesListProperty = BindableProperty.Create(nameof(ChangesList), typeof(List<WorkInstructionTemplateChangeNotification>), typeof(ShowChangesPopupView), propertyChanged: OnChangedListPropertyChanged);

	private static void OnChangedListPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var obj = bindable as ShowChangesPopupView;
		obj.ChangesList = (List<WorkInstructionTemplateChangeNotification>)newValue;
		obj.SelectedChange = obj.ChangesList?.FirstOrDefault();
	}

	public List<WorkInstructionTemplateChangeNotification> ChangesList
	{
		get => (List<WorkInstructionTemplateChangeNotification>)GetValue(ChangesListProperty);
		set
		{
			SetValue(ChangesListProperty, value);
			OnPropertyChanged();
		}
	}

	public static readonly BindableProperty ConfirmChangesButtonCommandProperty = BindableProperty.Create(nameof(ConfirmChangesButtonCommand), typeof(ICommand), typeof(ShowChangesPopupView));

	public ICommand ConfirmChangesButtonCommand
	{
		get => (ICommand)GetValue(ConfirmChangesButtonCommandProperty);
		set
		{
			SetValue(ConfirmChangesButtonCommandProperty, value);
			OnPropertyChanged();
		}
	}
}

public class ShowChangesPopupDataTemplateSelector : DataTemplateSelector
{
	public DataTemplate MediaItemTemplate { get; set; }
	public DataTemplate TagsItemTemplate { get; set; }
	public DataTemplate DefaultItemTemplate { get; set; }

	protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
	{
		DataTemplate result = DefaultItemTemplate;

		WorkInstructionTemplateChange change = item as WorkInstructionTemplateChange;
		if (item == null)
			return null;

		switch (change.PropertyName)
		{
			case "Media":
				result = MediaItemTemplate;
				break;
			case "Tags":
				result = TagsItemTemplate;
				break;
			default:
				break;
		}

		return result;
	}
}