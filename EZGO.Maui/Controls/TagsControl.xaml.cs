using System.Windows.Input;
using EZGO.Maui.Core.Models.Tags;

namespace EZGO.Maui.Controls;

public partial class TagsControl : Grid
{
    private bool isExpanded;
    public bool IsExpanded
    {
        get => isExpanded;
        set
        {
            isExpanded = value;
            OnPropertyChanged();
        }
    }

    public static BindableProperty TagListProperty = BindableProperty.Create(nameof(TagList), typeof(List<TagModel>), declaringType: typeof(TagsControl));

    public List<TagModel> TagList
    {
        get => (List<TagModel>)GetValue(TagListProperty);
        set => SetValue(TagListProperty, value);
    }

    public ICommand ExpandCommand { get; set; }

    public TagsControl()
    {

        ExpandCommand = new Command(() =>
        {
            IsExpanded = !IsExpanded;
            ExpandedTagList.HeightRequest = IsExpanded ? 315 : 55;
            //This is needed for ui to update
            var existingTagList = TagList;
            TagList = new List<TagModel>(existingTagList);
        });

        InitializeComponent();
        IsExpanded = false;
    }
}
