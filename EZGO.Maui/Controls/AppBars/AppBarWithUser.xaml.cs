using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels;
using System.Windows.Input;

namespace EZGO.Maui.Controls.AppBars;

public partial class AppBarWithUser : ContentView
{
    #region Bindable Properties

    public static readonly BindableProperty PopupCommandProperty = BindableProperty.Create(nameof(PopupCommand), typeof(ICommand), typeof(AppBarWithUser));

    public static readonly BindableProperty QRButtonCommandProperty = BindableProperty.Create(nameof(QRButtonCommand), typeof(ICommand), typeof(AppBarWithUser));

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(AppBarWithUser));

    public static readonly BindableProperty FullnameProperty = BindableProperty.Create(nameof(Fullname), typeof(string), typeof(AppBarWithUser), propertyChanged: OnFullNameChanged);

    public static readonly BindableProperty LogoProperty = BindableProperty.Create(nameof(Logo), typeof(string), typeof(AppBarWithUser), propertyChanged: OnLogoChanged);


    #endregion

    public AppBarWithUser()
    {
        InitializeComponent();

        MessagingCenter.Subscribe<ProfileViewModel>(this, Constants.ReloadUserDataMessage, (sender) =>
        {
            RefreshInfo();
        });

        RefreshInfo();
    }

    ~AppBarWithUser()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            MessagingCenter.Unsubscribe<ProfileViewModel>(this, Constants.ReloadUserDataMessage);
        });
    }

    public void RefreshInfo()
    {
        ProfilePicture.ImageUrl = string.IsNullOrEmpty(UserSettings.UserPictureUrl) ? "profile.png" : UserSettings.UserPictureUrl;

        if (Device.Idiom == TargetIdiom.Phone)
        {
            Fullname = $"{UserSettings.Firstname.Trim()}\n{UserSettings.Lastname.Trim()}";
        }
        else
        {
            Fullname = UserSettings.Fullname;
        }

        Logo = UserSettings.CompanyLogoUrl;

        OnPropertyChanged(ProfilePicture.ImageUrl);
    }

    private static void OnFullNameChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as AppBarWithUser;
        obj.Fullname = newValue.ToString();
    }

    private static void OnLogoChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as AppBarWithUser;
        obj.Logo = newValue.ToString();
    }

    #region Properties

    public string Fullname
    {
        get => (string)GetValue(FullnameProperty);
        set
        {
            SetValue(FullnameProperty, value);
            OnPropertyChanged();
        }
    }

    public ICommand PopupCommand
    {
        get => (ICommand)GetValue(PopupCommandProperty);
        set
        {
            SetValue(PopupCommandProperty, value);
            OnPropertyChanged();
        }
    }

    public ICommand QRButtonCommand
    {
        get => (ICommand)GetValue(QRButtonCommandProperty);
        set
        {
            SetValue(QRButtonCommandProperty, value);
            OnPropertyChanged();
        }
    }

    public string Logo
    {
        get => (string)GetValue(LogoProperty);
        set
        {
            SetValue(LogoProperty, value);
            OnPropertyChanged();
        }
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set
        {
            SetValue(TitleProperty, value);
            OnPropertyChanged();
        }
    }

    #endregion
}
