using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Templates;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Views.Animations;

[TemplatePart("PART_CollapseButton", typeof(ToggleButton))]
[PseudoClasses(":collapsed")]
internal class TimelineGroupHeader : TemplatedControl
{
    public static readonly StyledProperty<CelGroupViewModel> ItemProperty = AvaloniaProperty.Register<TimelineGroupHeader, CelGroupViewModel>(
        nameof(Item));

    public CelGroupViewModel Item
    {
        get => GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (e.NameScope.Find("PART_CollapseButton") is { } collapseButton)
        {
            (collapseButton as ToggleButton).IsCheckedChanged += CollapseButtonOnIsCheckedChanged;
        }
    }

    private void CollapseButtonOnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        bool isCollapsed = (sender as ToggleButton).IsChecked == true;
        PseudoClasses.Set(":collapsed", isCollapsed);
        Item.IsCollapsed = isCollapsed;
    }
}
