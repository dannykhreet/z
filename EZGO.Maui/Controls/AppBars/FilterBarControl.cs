using System;
using System.Windows.Input;
using EZGO.Maui.Classes;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Utils;

namespace EZGO.Maui.Controls.AppBars
{
    public class FilterBarControl<T> : StackLayout
    {
        private static bool _canExecute = true;
        #region Statuses List

        public static readonly BindableProperty StatusesProperty = BindableProperty.Create(nameof(StatusesProperty), typeof(IStatus<T>), typeof(FilterBarControl<T>), propertyChanged: OnStatusesPropertyChanged);

        private static void OnStatusesPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var obj = bindable as FilterBarControl<T>;
            obj.RebuildView();
        }

        public IStatus<T> Statuses
        {
            get => (IStatus<T>)GetValue(StatusesProperty);
            set
            {
                SetValue(StatusesProperty, value);
                OnPropertyChanged();
            }
        }

        #endregion

        #region Filter Command

        public static readonly BindableProperty FilterCommandProperty = BindableProperty.Create(nameof(FilterCommandProperty), typeof(ICommand), typeof(FilterBarControl<T>), propertyChanged: OnFilterCommandPropertyChanged);

        private static void OnFilterCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var obj = bindable as FilterBarControl<T>;
            if (newValue is Command)
            {
                obj.FilterCommand = newValue as Command;
                obj.FilterCommand.CanExecuteChanged += FilterCommand_CanExecuteChanged;
            }
            obj.RebuildView();
        }

        private static void FilterCommand_CanExecuteChanged(object sender, EventArgs e)
        {
            _canExecute = !_canExecute;
        }

        private static void OnCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var obj = bindable as FilterBarControl<T>;
            obj.RebuildView();
        }

        public ICommand FilterCommand
        {
            get => (ICommand)GetValue(FilterCommandProperty);
            set
            {
                SetValue(FilterCommandProperty, value);
                OnPropertyChanged();
            }
        }

        #endregion

        #region Item Width

        public static readonly BindableProperty ItemWidthProperty = BindableProperty.Create(nameof(ItemWidthProperty), typeof(double), typeof(FilterBarControl<T>), propertyChanged: OnCommandPropertyChanged, defaultValue: 50.0);

        public double ItemWidth
        {
            get => (double)GetValue(ItemWidthProperty);
            set
            {
                SetValue(ItemWidthProperty, value);
                OnPropertyChanged();
            }
        }

        #endregion

        #region Show percentages
        public static readonly BindableProperty ShowPercentagesProperty = BindableProperty.Create(nameof(ShowPercentagesProperty), typeof(bool), typeof(FilterBarControl<T>), defaultValue: true);

        public bool ShowPercentages
        {
            get => (bool)GetValue(ShowPercentagesProperty);
            set
            {
                SetValue(ShowPercentagesProperty, value);
                OnPropertyChanged();
            }
        }
        #endregion

        public FilterBarControl()
        {
            VerticalOptions = LayoutOptions.CenterAndExpand;
            Orientation = StackOrientation.Horizontal;
            HorizontalOptions = LayoutOptions.CenterAndExpand;
            Spacing = 0;
            Opacity = 0;
            WidthRequest = 150;
            HeightRequest = 20;
        }

        public void RebuildView()
        {
            Children.Clear();

            var statuses = Statuses?.GetStatuses();
            if (statuses != null)
            {
                statuses.ForEach(x =>
                {
                    Children.Add(GenerateView(x.Color, x.Status, x.ItemNumber, x.IsSelected, x.Percentage));
                });
            }
        }

        /// <summary>
        /// Generate Single Status Bar
        /// </summary>
        /// <param name="resourceName">Name of color resource applied to background</param>
        /// <param name="status">Value attatched to bar</param>
        /// <param name="itemsCount">Text displayed inside bar</param>
        /// <param name="height">Height of bar</param>
        /// <returns></returns>
        private View GenerateView(string resourceName, T status, int itemsCount, bool isSelected, int? percentage = null, double height = 20)
        {
            var grid = new Grid
            {
                WidthRequest = ItemWidth,
                HeightRequest = height,
                ScaleY = isSelected ? 1.2 : 1,
                BackgroundColor = ResourceHelper.GetValueFromResources<Color>(resourceName),
            };

            var text = $"{itemsCount}";

            if (ShowPercentages && percentage != null)
                text += $" ({percentage}%)";

            var label = new Label
            {
                Text = text,
                TextColor = Colors.White,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
            };
            grid.Children.Add(label);

            var tappedGesture = new TapGestureRecognizer
            {
                Command = FilterCommand,
                CommandParameter = status
            };
            tappedGesture.Tapped += TappedGesture_Tapped;
            grid.GestureRecognizers.Add(tappedGesture);

            return grid;
        }

        private async void TappedGesture_Tapped(object sender, EventArgs e)
        {
            if (!_canExecute)
                return;

            await AsyncAwaiter.AwaitAsync("FilterItems", async () =>
            {
                uint scaleDuration = 100;
                var obj = sender as Grid;
                if (obj.ScaleY == 1)
                    await obj.ScaleYTo(1.2, scaleDuration);
                else
                    await obj.ScaleYTo(1, scaleDuration);
            });
        }
    }
}

