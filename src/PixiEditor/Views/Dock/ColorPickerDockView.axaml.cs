using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using PixiEditor.Helpers.Behaviours;

namespace PixiEditor.Views.Dock;

public partial class ColorPickerDockView : UserControl
{
    public ColorPickerDockView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        var textBoxes = this.GetVisualDescendants().OfType<TextBox>().ToArray();

        ColorSlidersDockView.AttachBehavioursToTextBoxes(textBoxes); 
    }
}
