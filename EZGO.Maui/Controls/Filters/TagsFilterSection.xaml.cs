using System.Collections.ObjectModel;
using System.Windows.Input;
using EZGO.Maui.Core;
using EZGO.Maui.Core.Classes.Filters;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Tags;
using EZGO.Maui.Core.Models.Tags;

namespace EZGO.Maui.Controls.Filters;

public partial class TagsFilterSection : Border
{
	CancellationTokenSource cts = null;

	public int TextChangedDelay { get; set; }

	public List<TagModel> Tags { get; set; }

	public ObservableCollection<TagModel> SearchedTags { get; set; }

	public ICommand TagExpandCommand { get; set; }

	public ICommand ExpandCommand { get; set; }

	public ICommand TapCommand { get; set; }

	public ICommand SearchTagsCommand { get; set; }

	public bool IsExpanded { get; set; }

	public static BindableProperty FilterProperty = BindableProperty.Create(nameof(Filter), typeof(IFilterControl), declaringType: typeof(TagsFilterSection));

	public IFilterControl Filter
	{
		get => (IFilterControl)GetValue(FilterProperty);
		set => SetValue(FilterProperty, value);
	}

	public TagsFilterSection()
	{
		SearchedTags = new ObservableCollection<TagModel>();

		TagExpandCommand = new Command<object>((obj) =>
		{
			if (obj is TagModel tagModel)
			{
				tagModel.IsExpanded = !tagModel.IsExpanded;
			}
		});

		ExpandCommand = new Command(() =>
		{
			IsExpanded = !IsExpanded;

			OnPropertyChanged(nameof(IsExpanded));
		});

		TapCommand = new Command<object>((obj) =>
		{
			if (obj is TagModel tagModel && Filter != null)
			{
				tagModel.IsActive = !tagModel.IsActive;

				if (tagModel.IsActive)
				{
					Filter.SearchedTags.Add(tagModel);
				}
				else
				{
					Filter.SearchedTags.Remove(tagModel);
				}

				Filter.Filter(false, false);
				OnPropertyChanged(nameof(Filter.SearchedTags));
			}
		});

		SearchTagsCommand = new Command<object>((obj) =>
		{

		});

		InitializeComponent();
		_ = Task.Run(SetTags);
		IsExpanded = true;
		OnPropertyChanged(nameof(IsExpanded));
	}

	private async Task SetTags()
	{
		using var scope = App.Container.CreateScope();
		var tagsService = scope.ServiceProvider.GetService<ITagsService>();
		var tags = await tagsService.GetTagModelsAsync();
		Tags = tags;
		OnPropertyChanged(nameof(Tags));
	}

	public async void searchText_TextChanged(string newText)
	{
		if (Filter?.Tags == null) return;

		cts?.Cancel();
		cts?.Dispose();
		cts = new CancellationTokenSource();
		var token = cts.Token;

		try
		{
			var delay = TextChangedDelay > 0 ? TextChangedDelay : 650;
			await Task.Delay(delay, token);

			if (token.IsCancellationRequested)
				return;

			if (string.IsNullOrWhiteSpace(newText))
			{
				SearchedTags = new ObservableCollection<TagModel>();
				OnPropertyChanged(nameof(SearchedTags));
				TagTable.IsVisible = true;
				return;
			}

			var mainTags = Filter.Tags.Where(x => x.Name.Contains(newText, StringComparison.InvariantCultureIgnoreCase));
			var subTags = Filter.Tags.SelectMany(x => x.SubTags).Where(x => x.Name.Contains(newText, StringComparison.InvariantCultureIgnoreCase));

			SearchedTags = new ObservableCollection<TagModel>(mainTags.Concat(subTags));
			OnPropertyChanged(nameof(SearchedTags));
			TagTable.IsVisible = false;
		}
		catch (OperationCanceledException) { }
		
	}
}
