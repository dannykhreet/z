using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Utils;
using FFImageLoading.Maui;

namespace EZGO.Maui.Controls
{
    public class CustomImage : CachedImage
    {
        public string Placeholder { get; set; }
        public bool HidePlaceholder { get; set; }

        public bool IsLocalFile
        {
            get => (bool)GetValue(IsLocalFileProperty);
            set => SetValue(IsLocalFileProperty, value);
        }

        public static readonly BindableProperty IsLocalFileProperty = BindableProperty.Create(nameof(IsLocalFile), typeof(bool), typeof(CustomImage), defaultValue: false, propertyChanged: OnIsLocalFilePropertyChanged);

        private static void OnIsLocalFilePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is CustomImage customImage)
            {
                if (!string.IsNullOrEmpty(customImage.ImageUrl))
                {
                    customImage.LoadImage();
                }
            }
        }


        public string ImageUrl
        {
            get => (string)GetValue(ImageUrlProperty);
            set => SetValue(ImageUrlProperty, value);
        }

        public static readonly BindableProperty ImageUrlProperty = BindableProperty.Create(
            nameof(ImageUrl),
            typeof(string),
            typeof(CustomImage),
            defaultValue: string.Empty,
            propertyChanged: ImageUrlChanged
        );

        public CustomImage()
        {
            DownsampleToViewSize = true;
            BitmapOptimizations = true;
            IsOpaque = true;
            BackgroundColor = Colors.White;
        }

        private static void ImageUrlChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (Equals(oldValue, newValue))
                return;

            if (bindable is not CustomImage customImage || !customImage.IsVisible)
                return;

            bool isOnline = bindable is CustomImageOnline;
            customImage.LoadImage(isOnline);
        }

        private void LoadImage(bool isOnlineImage = false)
        {
            string imageUrl = ImageUrl?.Trim();

            if (isOnlineImage)
                IsLocalFile = false;

            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                if (HidePlaceholder)
                {
                    Source = null;
                    return;
                }

                SetPlaceholderImage();
                return;
            }

            if (!IsLocalFile && imageUrl.Contains("/media"))
                imageUrl = imageUrl.Replace("/media", "");

            if (imageUrl == Constants.NoProfilePicture || imageUrl == Constants.NoProfilePicture2)
            {
                imageUrl = Constants.NoProfilePicture2;
                IsLocalFile = true;
            }

            if (!IsLocalFile && !imageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                imageUrl = Constants.MediaBaseUrl.Format(imageUrl);

            imageUrl = imageUrl.Replace("/media/media", "/media");

            try
            {
                if (IsLocalFile)
                {
                    Source = ImageSource.FromFile(imageUrl);
                }
                else if (imageUrl.Contains(Constants.PlaceholderImage) && !HidePlaceholder)
                {
                    SetPlaceholderImage();
                }
                else
                {
                    // Use FFImageLoading features here
                    Source = new UriImageSource
                    {
                        Uri = new Uri(imageUrl),
                        CachingEnabled = true,
                        CacheValidity = TimeSpan.FromDays(5)
                    };
                }

                Placeholder = Constants.PlaceholderImage;
                CacheType = FFImageLoading.Cache.CacheType.Disk;
                CacheDuration = TimeSpan.FromDays(5);
                FadeAnimationForCachedImages = false;
                ErrorPlaceholder = HidePlaceholder ? null : Constants.PlaceholderImage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Image load error: {ex.Message}");
                SetPlaceholderImage();
            }
        }

        private void SetPlaceholderImage()
        {
            string placeholder = !string.IsNullOrWhiteSpace(Placeholder)
                ? Placeholder
                : Constants.PlaceholderImage;

            Source = ImageSource.FromFile(placeholder);
        }
    }
}
