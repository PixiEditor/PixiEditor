using System.ComponentModel;
using System.Windows.Input;
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
    public static readonly StyledProperty<ICommand> SelectCommandProperty = AvaloniaProperty.Register<TimelineGroupHeader, ICommand>("SelectCommand");

    public static readonly StyledProperty<CelGroupViewModel> ItemProperty =
        AvaloniaProperty.Register<TimelineGroupHeader, CelGroupViewModel>(
            nameof(Item));

    public CelGroupViewModel Item
    {
        get => GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    public ICommand SelectCommand
    {
        get { return (ICommand)GetValue(SelectCommandProperty); }
        set { SetValue(SelectCommandProperty, value); }
    }

    static TimelineGroupHeader()
    {
        ItemProperty.Changed.AddClassHandler<TimelineGroupHeader>((x, e) =>
        {
            if (e.OldValue is CelGroupViewModel oldItem)
            {
                oldItem.PropertyChanged -= x.NewItemOnPropertyChanged;
                x.PseudoClasses.Set(":selected", oldItem.IsGroupSelected);
            }

            if (e.NewValue is CelGroupViewModel newItem)
            {
                newItem.PropertyChanged += x.NewItemOnPropertyChanged;
                x.PseudoClasses.Set(":selected", newItem.IsGroupSelected);
            }
        });
    }

    public TimelineGroupHeader()
    {
        PointerPressed += (sender, args) =>
        {
            if (args.Source is Control { DataContext: CelGroupViewModel celGroup })
            {
                SelectCommand?.Execute(celGroup.LayerGuid);
            }
        };
    }

    private void NewItemOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CelGroupViewModel.IsGroupSelected))
        {
            PseudoClasses.Set(":selected", (sender as CelGroupViewModel).IsGroupSelected);
        }
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
