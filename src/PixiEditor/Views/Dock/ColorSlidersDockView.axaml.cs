using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using PixiEditor.Helpers.Behaviours;

namespace PixiEditor.Views.Dock;

public partial class ColorSlidersDockView : UserControl
{
    public ColorSlidersDockView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        var textBoxes = this.GetVisualDescendants().OfType<TextBox>().ToArray();

        AttachBehavioursToTextBoxes(textBoxes);
    }

    internal static void AttachBehavioursToTextBoxes(TextBox[] textBoxes)
    {
        foreach (var textBox in textBoxes)
        {
            var existingBehaviors = Interaction.GetBehaviors(textBox);
            if (existingBehaviors.Any(x => x is GlobalShortcutFocusBehavior)) continue;
            bool attach = false;
            if (existingBehaviors == null)
            {
                attach = true;
                existingBehaviors = new BehaviorCollection();
            }

            try
            {
                existingBehaviors.Add(new GlobalShortcutFocusBehavior());

                if (attach)
                {
                    Interaction.SetBehaviors(textBox, existingBehaviors);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                // avalonia's bug
            }
        }
    }
}
