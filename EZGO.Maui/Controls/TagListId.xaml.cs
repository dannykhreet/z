using System.ComponentModel;
using System.Text.Json;
using EZGO.Api.Models.Tags;
using EZGO.Maui.Core;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Tags;

namespace EZGO.Maui.Controls;

public partial class TagListId : ContentView, INotifyPropertyChanged
{
	public static BindableProperty TagStringIdsProperty = BindableProperty.Create(nameof(TagStringIds), typeof(string), declaringType: typeof(TagListId), propertyChanged: OnTagStringIdsPropertyChanged);
	private List<Tag> tagList;

	private static async void OnTagStringIdsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		TagListId control = (TagListId)bindable;
		if (control == null)
			return;

		control.TagStringIds = (string)newValue;
		await SetTagList(control);
	}

	private static async Task SetTagList(TagListId control)
	{
		string tagStringList = control.TagStringIds;
		if (!tagStringList.IsNullOrEmpty())
		{
			using var scope = App.Container.CreateScope();
			var tagsService = scope.ServiceProvider.GetService<ITagsService>();
			var tagModels = await tagsService.GetTagsAsync();
			var tagIds = JsonSerializer.Deserialize<List<int>>(tagStringList);
			var tags = tagModels.Where(x => tagIds.Contains(x.Id)).Select(x => x.ToTag()).ToList();
			control.TagList = tags;
		}
	}

	public string TagStringIds
	{
		get => (string)GetValue(TagStringIdsProperty);
		set => SetValue(TagStringIdsProperty, value);
	}

	public List<Tag> TagList
	{
		get => tagList; set
		{
			tagList = value;
			OnPropertyChanged();
		}
	}




	public TagListId()
	{
		InitializeComponent();
	}
}