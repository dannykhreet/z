using EZGO.Maui.Core.Models;

namespace EZGO.Maui.Controls;

public partial class CompletedSignature : ContentView
{
    public CompletedSignature()
    {
        InitializeComponent();
        signature.IsVisible = false;
    }

    public readonly static BindableProperty SelectedSignaturesProperty = BindableProperty.Create(nameof(SelectedSignaturesProperty),
                                                                                                 typeof(List<SignatureModel>),
                                                                                                 typeof(CompletedSignature),
                                                                                                 propertyChanged: OnSelectedSignaturePropertyChanged);

    public readonly static BindableProperty IsSignatureVisibleProperty = BindableProperty.Create(nameof(IsSignatureVisibleProperty),
                                                                                                 typeof(bool),
                                                                                                 typeof(CompletedSignature),
                                                                                                 propertyChanged: OnIsSignatureVisiblePropertyChanged);

    private static void OnIsSignatureVisiblePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as CompletedSignature;
        obj.signature.IsVisible = obj.IsSignatureVisible;
    }

    private static void OnSelectedSignaturePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as CompletedSignature;
        obj.signature.ItemsSource = obj.SelectedSignatures;
        if (obj.SelectedSignatures != null)
        {
            var signatureLayout = new GridItemsLayout(obj.SelectedSignatures.Count() > 0 ? obj.SelectedSignatures.Count() : 1, ItemsLayoutOrientation.Vertical);
            obj.signature.ItemsLayout = signatureLayout;
        }
    }

    public List<SignatureModel> SelectedSignatures
    {
        get => (List<SignatureModel>)GetValue(SelectedSignaturesProperty);
        set { SetValue(SelectedSignaturesProperty, value); OnPropertyChanged(); }
    }

    public bool IsSignatureVisible
    {
        get => (bool)GetValue(IsSignatureVisibleProperty);
        set
        {
            SetValue(IsSignatureVisibleProperty, value);
            OnPropertyChanged();
        }
    }
}
