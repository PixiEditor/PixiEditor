using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

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
