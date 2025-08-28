using System;
using System.Windows.Input;
using Syncfusion.Maui.PdfViewer;

namespace EZGO.Maui.Controls
{
    public class CustomPdfViewer : SfPdfViewer
    {
        private const float ZOOM_THRESHOLD = 120;
        public event EventHandler SwipeLeft;
        public event EventHandler SwipeRight;

        public CustomPdfViewer()
        {
            var item = Toolbars?.GetByName("MoreOptionToolbar");
            if (item != null)
            {
                item.IsVisible = false;
            }
            ShowToolbars = false;
        }

        public void OnSwipeLeft()
        {
            if (ZoomFactor > ZOOM_THRESHOLD)
                return;

            ICommand cmd = SwipeLeftCommand;
            if (cmd != null && cmd.CanExecute("Left"))
                cmd.Execute("Left");

            SwipeLeft?.Invoke(this, null);
        }

        public void OnSwipeRight()
        {
            if (ZoomFactor > ZOOM_THRESHOLD)
                return;

            ICommand cmd = SwipeRightCommand;
            if (cmd != null && cmd.CanExecute("Right"))
                cmd.Execute("Right");

            SwipeRight?.Invoke(this, null);
        }

        public static readonly BindableProperty SwipeLeftCommandProperty = BindableProperty.Create(nameof(SwipeLeftCommand), typeof(ICommand), typeof(CustomPdfViewer), null);

        public ICommand SwipeLeftCommand
        {
            get => (ICommand)GetValue(SwipeLeftCommandProperty);
            set
            {
                SetValue(SwipeLeftCommandProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty SwipeRightCommandProperty = BindableProperty.Create(nameof(SwipeRightCommand), typeof(ICommand), typeof(CustomPdfViewer), null);

        public ICommand SwipeRightCommand
        {
            get => (ICommand)GetValue(SwipeRightCommandProperty);
            set
            {
                SetValue(SwipeRightCommandProperty, value);
                OnPropertyChanged();
            }
        }
    }
}

