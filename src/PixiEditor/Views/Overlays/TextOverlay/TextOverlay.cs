using Avalonia;
using Avalonia.Input;
using Avalonia.Threading;
using Drawie.Numerics;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Input;
using PixiEditor.OperatingSystem;
using Canvas = Drawie.Backend.Core.Surfaces.Canvas;

namespace PixiEditor.Views.Overlays.TextOverlay;

public class TextOverlay : Overlay
{
    private Dictionary<KeyCombination, Action> shortcuts;

    public TextOverlay()
    {
        shortcuts = new Dictionary<KeyCombination, Action>
        {
            { new KeyCombination(Key.V, KeyModifiers.Control), PasteText },
        };
    }

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
        if (IsShortcut(key, keyModifiers))
        {
            ExecuteShortcut(key, keyModifiers);
            return;
        }

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
            if (converted == null || converted.Length > 1) return;

            string toAdd = keyModifiers.HasFlag(KeyModifiers.Shift) ? converted.ToUpper() : converted.ToLower();
            char? keyChar = toAdd.FirstOrDefault();
            if (keyChar != null)
            {
                if (char.IsControl(keyChar.Value)) return;
                Text += keyChar;
            }
        }
    }

    private bool IsShortcut(Key key, KeyModifiers keyModifiers)
    {
        return shortcuts.ContainsKey(new KeyCombination(key, keyModifiers));
    }

    private void ExecuteShortcut(Key key, KeyModifiers keyModifiers)
    {
        KeyCombination shortcut = new(key, keyModifiers);
        if (shortcuts.ContainsKey(shortcut))
        {
            shortcuts[shortcut].Invoke();
        }
    }

    private void PasteText()
    {
        ClipboardController.GetTextFromClipboard().ContinueWith(t =>
        {
            Dispatcher.UIThread.Invoke(() => Text += t.Result);
        }, TaskContinuationOptions.OnlyOnRanToCompletion);
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
