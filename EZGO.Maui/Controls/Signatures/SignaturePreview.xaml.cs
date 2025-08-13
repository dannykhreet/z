using System.Windows.Input;
using EZGO.Api.Models;

namespace EZGO.Maui.Controls.Signatures;

public partial class SignaturePreview : ContentView
{
	public SignaturePreview()
	{
		InitializeComponent();
	}

	void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
	{
		Signature1Pad.IsVisible = !Signature1Pad.IsVisible;
		if (Signature1Pad.IsVisible)
		{
			ExpandIconPad1.Rotation = 0;
			NotesEditor.IsVisible = false;
			ExpandIconNotes.Rotation = 0;
		}
		else
			ExpandIconPad1.Rotation = 180;
	}

	void TapGestureRecognizer_Tapped_1(System.Object sender, System.EventArgs e)
	{
		Signature2Pad.IsVisible = !Signature2Pad.IsVisible;
		if (Signature2Pad.IsVisible)
		{
			ExpandIconPad2.Rotation = 0;
			NotesEditor.IsVisible = false;
			ExpandIconNotes.Rotation = 0;
		}
		else
			ExpandIconPad2.Rotation = 180;
	}

	void TapGestureRecognizer_Tapped_2(System.Object sender, System.EventArgs e)
	{
		NotesEditor.IsVisible = !NotesEditor.IsVisible;
		if (NotesEditor.IsVisible)
		{
			ExpandIconNotes.Rotation = 180;
			Signature1Pad.IsVisible = false;
			Signature2Pad.IsVisible = false;
			ExpandIconPad1.Rotation = ExpandIconPad2.Rotation = 180;
		}
		else
		{
			ExpandIconNotes.Rotation = 0;

			Signature1Pad.IsVisible = true;
			Signature2Pad.IsVisible = true;
			ExpandIconPad1.Rotation = ExpandIconPad2.Rotation = 0;
		}
	}

	public static readonly BindableProperty AreSignaturesVisibleProperty = BindableProperty.Create(nameof(AreSignaturesVisible), typeof(bool), typeof(SignaturePreview), propertyChanged: OnAreSignaturesVisiblePropertyChanged, defaultValue: true);

	private static void OnAreSignaturesVisiblePropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var bind = bindable as SignaturePreview;
		bind.AreSignaturesVisible = (bool)newValue;
		if (!bind.AreSignaturesVisible)
		{
			bind.NotesEditor.IsVisible = true;
			bind.ExpandIconNotes.Rotation = 180;
		}
		else
		{
			bind.NotesEditor.IsVisible = false;
			bind.ExpandIconNotes.Rotation = 0;
		}
	}

	public bool AreSignaturesVisible
	{
		get => (bool)GetValue(AreSignaturesVisibleProperty);
		set { SetValue(AreSignaturesVisibleProperty, value); OnPropertyChanged(nameof(AreSignaturesVisible)); }
	}

	public static readonly BindableProperty ShiftNotesProperty = BindableProperty.Create(nameof(ShiftNotes), typeof(string), typeof(SignaturePreview), propertyChanged: OnShiftNotesPropertyChanged);
	private static void OnShiftNotesPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var bind = bindable as SignaturePreview;
		bind.ShiftNotes = (string)newValue;
	}

	public string ShiftNotes { get => (string)GetValue(ShiftNotesProperty); set { SetValue(ShiftNotesProperty, value); OnPropertyChanged(); } }


	public static readonly BindableProperty CancelCommandProperty = BindableProperty.Create(nameof(CancelCommand), typeof(ICommand), typeof(SignaturePreview));

	public ICommand CancelCommand { get => (ICommand)GetValue(CancelCommandProperty); set { SetValue(CancelCommandProperty, value); OnPropertyChanged(); } }

	public static readonly BindableProperty Signature1Property = BindableProperty.Create(nameof(Signature1), typeof(Signature), typeof(SignaturePreview));
	public Signature Signature1 { get => (Signature)GetValue(Signature1Property); set { SetValue(Signature1Property, value); OnPropertyChanged(); } }

	public static readonly BindableProperty Signature2Property = BindableProperty.Create(nameof(Signature2), typeof(Signature), typeof(SignaturePreview));
	public Signature Signature2 { get => (Signature)GetValue(Signature2Property); set { SetValue(Signature2Property, value); OnPropertyChanged(); } }
}