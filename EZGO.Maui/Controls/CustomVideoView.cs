using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Utils;

namespace EZGO.Maui.Controls
{
    public class CustomVideoView : ContentView
    {
        private readonly MyVideoView VideoView;
        private readonly ActivityIndicator activityIndicator;

        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(IDetailItem), typeof(CustomVideoView), propertyChanged: SelectedItemChanged);

        public IDetailItem SelectedItem
        {
            get { return (IDetailItem)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public CustomVideoView()
        {
            VideoView = new MyVideoView
            {
                ShouldShowPlaybackControls = true,
                Aspect = Aspect.AspectFit,
                ShouldAutoPlay = true,
                ShouldLoopPlayback = true,
            };

            activityIndicator = new ActivityIndicator
            {
                Opacity = 0.8,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Color = DeviceInfo.Platform == DevicePlatform.iOS ? Colors.White : ResourceHelper.GetApplicationResource<Color>("GreenColor"),
                IsRunning = true,
                InputTransparent = true
            };

            VideoView.PropertyChanged += VideoView_PropertyChanged;

            Unloaded += CustomVideoView_Unloaded;

            var grid = new Grid();
            grid.Children.Add(VideoView);
            grid.Children.Add(activityIndicator);
            Content = grid;
        }

        private async static void SelectedItemChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            CustomVideoView customImage = bindable as CustomVideoView;

            if (customImage.SelectedItem != null && customImage.SelectedItem.HasVideo)
            {
                var url = customImage.SelectedItem.Video;

                if (!customImage.SelectedItem.IsLocalMedia)
                    url = await AppUrlsResolver.Video(customImage.SelectedItem.Video);

                customImage.VideoView.Source = url;
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    customImage.VideoView.Play();
                });
            }
        }

        private void VideoView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VideoView.CurrentState))
            {
                if (activityIndicator != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        activityIndicator.IsRunning = VideoView.CurrentState == MediaElementState.Buffering;
                    });
                }
            }
        }

        private void CustomVideoView_Unloaded(object? sender, EventArgs e)
        {
            VideoView.Handler?.DisconnectHandler();
        }
    }

    public class MyVideoView : MediaElement { }
}

