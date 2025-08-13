using EZGO.Maui.Core.Classes;

namespace EZGO.Maui.Views.Tasks.Comments;

public partial class TaskCommentEditPage : ContentPage
{
    public TaskCommentEditPage()
    {
        InitializeComponent();
        SetLayout();
        UpdatePlaceholderVisibility();
    }

    private void SetLayout()
    {
        if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
        {
            commentRow.Height = 400;
            Tags.IsVisible = true;
            ImagesList.ItemSize = 110;
        }
    }

    private void Editor_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdatePlaceholderVisibility();
    }

    private void Editor_Focused(object sender, FocusEventArgs e)
    {
        UpdatePlaceholderVisibility();
    }

    private void Editor_Unfocused(object sender, FocusEventArgs e)
    {
        UpdatePlaceholderVisibility();
    }

    private void UpdatePlaceholderVisibility()
    {
        if (PlaceholderLabel != null && entry != null)
        {
            PlaceholderLabel.IsVisible = string.IsNullOrWhiteSpace(entry.Text);
        }
    }
}