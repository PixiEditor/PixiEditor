using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using Brush = PixiEditor.Models.BrushEngine.Brush;

namespace PixiEditor.Views.Input;

internal partial class BrushPicker : UserControl
{
    public static readonly StyledProperty<int> BrushIndexProperty = AvaloniaProperty.Register<BrushPicker, int>(
        nameof(BrushIndex));

    public int BrushIndex
    {
        get => GetValue(BrushIndexProperty);
        set => SetValue(BrushIndexProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<Brush>> BrushesProperty =
        AvaloniaProperty.Register<BrushPicker, ObservableCollection<Brush>>(
            nameof(Brushes));

    public ObservableCollection<Brush> Brushes
    {
        get => GetValue(BrushesProperty);
        set => SetValue(BrushesProperty, value);
    }

    public static readonly StyledProperty<Brush?> SelectedBrushProperty =
        AvaloniaProperty.Register<BrushPicker, Brush?>(
            nameof(SelectedBrush));

    public Brush? SelectedBrush
    {
        get => GetValue(SelectedBrushProperty);
        set => SetValue(SelectedBrushProperty, value);
    }

    private bool suppressUpdate = false;

    static BrushPicker()
    {
        BrushesProperty.Changed.AddClassHandler<BrushPicker>((x, e) => x.OnBrushesChanged(e));
        BrushIndexProperty.Changed.AddClassHandler<BrushPicker>((x, e) => x.OnBrushIndexChanged(e));
        SelectedBrushProperty.Changed.AddClassHandler<BrushPicker>((x, e) =>
        {
            if (e.NewValue is Brush newBrush && x.Brushes != null)
            {
                int index = x.Brushes.IndexOf(newBrush);
                if (index != -1 && x.BrushIndex != index)
                {
                    x.suppressUpdate = true;
                    x.BrushIndex = index;
                    x.suppressUpdate = false;
                }
            }
        });
    }

    public BrushPicker()
    {
        InitializeComponent();
    }

    private void OnBrushesChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is ObservableCollection<Brush> brushes && brushes.Count > 0)
        {
            BrushIndex = brushes.IndexOf(SelectedBrush);
            if (BrushIndex == -1)
            {
                BrushIndex = 0;
                SelectedBrush = brushes[0];
            }
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if(SelectedBrush != null && BrushIndex == -1 && Brushes != null)
        {
            int index = Brushes.IndexOf(SelectedBrush);
            if (index != -1)
            {
                BrushIndex = index;
            }
        }
    }

    private void OnBrushIndexChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (suppressUpdate || (e.Sender is Visual v && !v.IsAttachedToVisualTree()))
            return;

        if (e.NewValue is int index && Brushes != null && index >= 0 && index < Brushes.Count)
        {
            SelectedBrush = Brushes[index];
        }
        else
        {
            SelectedBrush = null;
        }
    }
}
