using System.ComponentModel;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.ViewModels;
using Syncfusion.Maui.ListView;

namespace EZGO.Maui.Views.Actions;

public partial class ActionConversationPage : ContentPage
{
    private ActionConversationViewModel VM => BindingContext as ActionConversationViewModel;

    public ActionConversationPage()
    {
        InitializeComponent();
        TagsStackLayout.IsVisible = CompanyFeatures.CompanyFeatSettings.TagsEnabled;
    }

    protected override void OnAppearing()
    {
        if (VM != null)
        {
            // NOTE, THIS IS VERY DANGEROUS, NEVER REFERENCE VIEW MODEL DIRECTLY FROM PAGE 
            // OR HOOK TO IT'S EVENTS. THIS IS A MEMORY LEAK AND A VERY BAD PRACTICE
            VM.PropertyChanged += ChatOnPropertyChanged;
        }
        base.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        if (VM != null)
        {
            VM.PropertyChanged -= ChatOnPropertyChanged;
        }

        base.OnDisappearing();
    }

    private void ChatOnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // PLEASE NEVER DO THAT, USE BINDING INSTEAD
        if (e.PropertyName == nameof(ActionConversationViewModel.Chat))
        {
            Device.StartTimer(TimeSpan.FromMilliseconds(50), () =>
            {
                if (VM?.Chat != null)
                    (CommentsListView.ItemsLayout as LinearLayout).ScrollToRowIndex(VM?.Chat?.Count ?? 0);

                return false;
            });
        }
    }
}
