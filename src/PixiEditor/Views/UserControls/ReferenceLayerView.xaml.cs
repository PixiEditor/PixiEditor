using System.Windows;
using System.Windows.Controls;

namespace PixiEditor.Views.UserControls;

public partial class ReferenceLayerView : UserControl
{
    public static readonly DependencyProperty ShowReferenceLayerProperty =
        DependencyProperty.Register(nameof(ShowReferenceLayer), typeof(bool), typeof(ReferenceLayerView), new PropertyMetadata(UpdateVisibility));

    public bool ShowReferenceLayer
    {
        get => (bool)GetValue(ShowReferenceLayerProperty);
        set => SetValue(ShowReferenceLayerProperty, value);
    }

    public static readonly DependencyProperty HideReferenceLayerProperty =
        DependencyProperty.Register(nameof(HideReferenceLayer), typeof(bool), typeof(ReferenceLayerView), new PropertyMetadata(UpdateVisibility));

    public bool HideReferenceLayer
    {
        get => (bool)GetValue(HideReferenceLayerProperty);
        set => SetValue(HideReferenceLayerProperty, value);
    }

    internal static readonly DependencyPropertyKey ReferenceLayerVisibilityKey =
        DependencyProperty.RegisterReadOnly(nameof(ReferenceLayerVisibility), typeof(Visibility), typeof(ReferenceLayerView), new FrameworkPropertyMetadata());

    public Visibility ReferenceLayerVisibility
    {
        get => (Visibility)GetValue(ReferenceLayerVisibilityKey.DependencyProperty);
        private set => SetValue(ReferenceLayerVisibilityKey, value);
    }

    public ReferenceLayerView()
    {
        InitializeComponent();
    }

    private static void UpdateVisibility(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        var view = obj as ReferenceLayerView;

        Visibility visibility = view.ShowReferenceLayer ? Visibility.Visible : Visibility.Collapsed;

        if (view.HideReferenceLayer)
        {
            visibility = Visibility.Collapsed;
        }

        view.ReferenceLayerVisibility = visibility;
    }
}
