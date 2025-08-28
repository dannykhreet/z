using System;
using EZGO.Maui.Core.Extensions;

namespace EZGO.Maui.Controls
{
    public class PinchToZoomContainer : ContentView
    {
        private double currentScale = 1;
        private double startScale = 1;
        private double xOffset;
        private double yOffset;

        public static readonly BindableProperty IsZoomingProperty = BindableProperty.Create(nameof(IsZooming), typeof(bool), typeof(PinchToZoomContainer), defaultBindingMode: BindingMode.TwoWay);

        public bool IsZooming
        {
            get => (bool)GetValue(IsZoomingProperty);
            set => SetValue(IsZoomingProperty, value);
        }

        public PinchToZoomContainer()
        {
            PinchGestureRecognizer pinchGestureRecognizer = new PinchGestureRecognizer();
            pinchGestureRecognizer.PinchUpdated += OnPinchUpdated;
            GestureRecognizers.Add(pinchGestureRecognizer);

            TapGestureRecognizer doubleTapGestureRecognizer = new TapGestureRecognizer
            {
                NumberOfTapsRequired = 2
            };
            doubleTapGestureRecognizer.Tapped += OnDoubleTapped;
            GestureRecognizers.Add(doubleTapGestureRecognizer);
        }

        private void OnDoubleTapped(object sender, EventArgs e)
        {
            if (Content.Scale > 1)
            {
                Content.Scale = 1;
                Content.TranslationX = 0;
                Content.TranslationY = 0;
            }
            else
                Content.Scale = 2;
        }

        private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs args)
        {
            switch (args.Status)
            {
                case GestureStatus.Started:
                    IsZooming = true;
                    startScale = Content.Scale;
                    Content.AnchorX = 0;
                    Content.AnchorY = 0;
                    break;
                case GestureStatus.Running:
                    currentScale += (args.Scale - 1) * startScale;
                    currentScale = Math.Max(1, currentScale);
                    double renderedX = Content.X + xOffset;
                    double deltaX = renderedX / Width;
                    double deltaWidth = Width / (Content.Width * startScale);
                    double originX = (args.ScaleOrigin.X - deltaX) * deltaWidth;
                    double renderedY = Content.Y + yOffset;
                    double deltaY = renderedY / Height;
                    double deltaHeight = Height / (Content.Height * startScale);
                    double originY = (args.ScaleOrigin.Y - deltaY) * deltaHeight;
                    double targetX = xOffset - (originX * Content.Width) * (currentScale - startScale);
                    double targetY = yOffset - (originY * Content.Height) * (currentScale - startScale);
                    Content.TranslationX = targetX.Clamp(-Content.Width * (currentScale - 1), 0);
                    Content.TranslationY = targetY.Clamp(-Content.Height * (currentScale - 1), 0);
                    Content.Scale = currentScale;
                    break;
                case GestureStatus.Completed:
                    xOffset = Content.TranslationX;
                    yOffset = Content.TranslationY;
                    IsZooming = false;
                    break;
            }
        }
    }
}

