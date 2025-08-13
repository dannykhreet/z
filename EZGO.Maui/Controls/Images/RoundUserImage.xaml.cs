using EZGO.Maui.Core;
using EZGO.Maui.Core.Interfaces.User;

namespace EZGO.Maui.Controls.Images;

public partial class RoundUserImage : Frame, IDisposable
{
    private CancellationTokenSource? _cts;
    private bool _disposedValue;
    public static BindableProperty UserIdProperty = BindableProperty.Create(
           nameof(UserId),
           typeof(int),
           typeof(RoundUserImage),
           propertyChanged: UserIdProperty_Changed, defaultValue: -1);

    public int UserId
    {
        get => (int)GetValue(UserIdProperty);
        set => SetValue(UserIdProperty, value);
    }

    private static async void UserIdProperty_Changed(BindableObject bindable, object oldValue, object newValue)
    {
        var userId = (int)newValue;
        var control = bindable as RoundUserImage;
        if (control != null)
        {
            control._cts?.Cancel();
            control._cts?.Dispose();
            control._cts = new CancellationTokenSource();
            var token = control._cts.Token;

            control.UserImage.ImageUrl = "profile.png";

            if (userId > 0)
            {
                using var scope = App.Container.CreateScope();
                var userService = scope.ServiceProvider.GetService<IUserService>();
                var user = await userService.GetCompanyUserAsync(userId);

                if (!token.IsCancellationRequested && control.UserImage != null)
                    control.UserImage.ImageUrl = user.Picture;
            }
        }
    }
    public RoundUserImage()
    {
        InitializeComponent();
        Unloaded += RoundUserImage_Unloaded;
    }

    private void RoundUserImage_Unloaded(object? sender, EventArgs e)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    public void Dispose()
    {
        Dispose(true);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _cts?.Cancel();
                _cts?.Dispose();
                _cts = null;
            }

            _disposedValue = true;
        }
    }
}