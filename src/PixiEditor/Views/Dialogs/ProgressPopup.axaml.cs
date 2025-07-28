using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PixiEditor.Views.Dialogs;

public partial class ProgressPopup : PixiEditorPopup
{
    public static readonly StyledProperty<double> ProgressProperty = AvaloniaProperty.Register<ProgressPopup, double>(
        nameof(Progress));

    public static readonly StyledProperty<string> StatusProperty = AvaloniaProperty.Register<ProgressPopup, string>(
        nameof(Status));

    public static readonly StyledProperty<CancellationTokenSource> CancellationTokenProperty = AvaloniaProperty.Register<ProgressPopup, CancellationTokenSource>(
        nameof(CancellationToken));

    public CancellationTokenSource CancellationToken
    {
        get => GetValue(CancellationTokenProperty);
        set => SetValue(CancellationTokenProperty, value);
    }

    public string Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        CancellationToken.Cancel();
        if(!e.IsProgrammatic)
        {
            e.Cancel = true;
        }
    }

    public ProgressPopup()
    {
        InitializeComponent();
    }
}

