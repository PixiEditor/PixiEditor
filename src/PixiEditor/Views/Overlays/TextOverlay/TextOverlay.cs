using Avalonia;
using Avalonia.Input;
using Drawie.Numerics;
using PixiEditor.Models.Controllers;
using PixiEditor.OperatingSystem;
using Canvas = Drawie.Backend.Core.Surfaces.Canvas;

namespace PixiEditor.Views.Overlays.TextOverlay;

public class TextOverlay : Overlay
{
    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<TextOverlay, string>(
        nameof(Text));

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<VecD> PositionProperty = AvaloniaProperty.Register<TextOverlay, VecD>(
        nameof(Position));

    public VecD Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public static readonly StyledProperty<double> FontSizeProperty = AvaloniaProperty.Register<TextOverlay, double>(
        nameof(FontSize));

    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    static TextOverlay()
    {
        IsVisibleProperty.Changed.Subscribe(IsVisibleChanged);
    }

    public override void RenderOverlay(Canvas context, RectD canvasBounds)
    {
    }

    protected override void OnKeyPressed(Key key, KeyModifiers keyModifiers)
    {
        if (key == Key.Back)
        {
            if (Text.Length > 0)
            {
                Text = Text[..^1];
            }
        }
        else if (key == Key.Enter)
        {
            Text += Environment.NewLine;
        }
        else if (key == Key.Space)
        {
            Text += " ";
        }
        else
        {
            string converted = IOperatingSystem.Current.InputKeys.GetKeyboardKey(key);
            if(converted == null || converted.Length > 1) return;
            
            string toAdd = keyModifiers.HasFlag(KeyModifiers.Shift) ? converted.ToUpper() : converted.ToLower();
            char? keyChar = toAdd.FirstOrDefault();
            if (keyChar != null)
            {
                if(char.IsControl(keyChar.Value)) return;
                Text += keyChar;
            }
        }
    }

    private static void IsVisibleChanged(AvaloniaPropertyChangedEventArgs<bool> args)
    {
        if (args.NewValue.Value)
        {
            ShortcutController.BlockShortcutExecution(nameof(TextOverlay));
        }
        else
        {
            ShortcutController.UnblockShortcutExecution(nameof(TextOverlay));
        }
    }
}
