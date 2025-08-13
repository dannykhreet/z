using System.ComponentModel;
using System.Text.Json;
using EZGO.Maui.Core.Extensions;

namespace EZGO.Maui.Controls;

public partial class ChangePicture : ContentView, INotifyPropertyChanged
{
	public static BindableProperty PictureStringListProperty = BindableProperty.Create(nameof(PictureStringList), typeof(string), declaringType: typeof(ChangePicture), propertyChanged: OnPictureStringListPropertyChanged);
	public string PictureStringList
	{
		get => (string)GetValue(PictureStringListProperty);
		set => SetValue(PictureStringListProperty, value);
	}

	private static void OnPictureStringListPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		ChangePicture control = (ChangePicture)bindable;
		if (control == null)
			return;

		control.PictureStringList = (string)newValue;
		SetPicture(control);
	}

	private static void SetPicture(ChangePicture control)
	{
		string pictureStringList = control.PictureStringList;
		if (pictureStringList.IsNullOrEmpty() || pictureStringList == "[]")
		{
			//control.Image.IsVisible = false;
			return;
		}

		var pictureList = JsonSerializer.Deserialize<List<string>>(pictureStringList);
		var picture = pictureList.FirstOrDefault();
		control.Picture = picture;
		control.Image.IsVisible = true;
	}

	private string picture;
	public string Picture
	{
		get => picture; set
		{
			picture = value;
			OnPropertyChanged();
		}
	}

	public ChangePicture()
	{
		InitializeComponent();
	}
}