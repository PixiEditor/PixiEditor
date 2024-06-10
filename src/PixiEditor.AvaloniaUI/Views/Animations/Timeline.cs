using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.ViewModels.Document;

namespace PixiEditor.AvaloniaUI.Views.Animations;

public class Timeline : TemplatedControl
{
    public static readonly StyledProperty<ObservableCollection<IClipHandler>> ClipsProperty =
        AvaloniaProperty.Register<Timeline, ObservableCollection<IClipHandler>>(
            nameof(Clips));

    public ObservableCollection<IClipHandler> Clips
    {
        get => GetValue(ClipsProperty);
        set => SetValue(ClipsProperty, value);
    }
}

