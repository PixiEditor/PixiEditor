using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Numerics;
using PixiEditor.Extensions.UI.Overlays;
using PixiEditor.Helpers;
using PixiEditor.Helpers.UI;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Input;
using PixiEditor.OperatingSystem;
using Canvas = Drawie.Backend.Core.Surfaces.Canvas;

namespace PixiEditor.Views.Overlays.TextOverlay;

internal class TextOverlay : Overlay
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

    public static readonly StyledProperty<Font> FontProperty = AvaloniaProperty.Register<TextOverlay, Font>(
        nameof(Font));

    public Font Font
    {
        get => GetValue(FontProperty);
        set => SetValue(FontProperty, value);
    }

    public static readonly StyledProperty<int> CursorPositionProperty = AvaloniaProperty.Register<TextOverlay, int>(
        nameof(CursorPosition), coerce: ClampValue);

    public int CursorPosition
    {
        get => GetValue(CursorPositionProperty);
        set => SetValue(CursorPositionProperty, value);
    }

    public static readonly StyledProperty<int> SelectionEndProperty = AvaloniaProperty.Register<TextOverlay, int>(
        nameof(SelectionEnd), coerce: ClampValue);

    public int SelectionEnd
    {
        get => GetValue(SelectionEndProperty);
        set => SetValue(SelectionEndProperty, value);
    }

    public static readonly StyledProperty<Matrix3X3> MatrixProperty = AvaloniaProperty.Register<TextOverlay, Matrix3X3>(
        nameof(Matrix), Matrix3X3.Identity);

    public Matrix3X3 Matrix
    {
        get => GetValue(MatrixProperty);
        set => SetValue(MatrixProperty, value);
    }

    public static readonly StyledProperty<ExecutionTrigger<string>> RequestEditTextProperty =
        AvaloniaProperty.Register<TextOverlay, ExecutionTrigger<string>>(
            nameof(RequestEditText));

    public ExecutionTrigger<string> RequestEditText
    {
        get => GetValue(RequestEditTextProperty);
        set => SetValue(RequestEditTextProperty, value);
    }

    public static readonly StyledProperty<bool> IsEditingProperty = AvaloniaProperty.Register<TextOverlay, bool>(
        nameof(IsEditing));

    public bool IsEditing
    {
        get => GetValue(IsEditingProperty);
        set => SetValue(IsEditingProperty, value);
    }

    public static readonly StyledProperty<double?> SpacingProperty = AvaloniaProperty.Register<TextOverlay, double?>(
        nameof(Spacing));

    public double? Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    private Dictionary<KeyCombination, Action> shortcuts;

    private Caret caret = new Caret();
    private VecF[] glyphPositions;
    private float[] glyphWidths;
    private RichText richText;
    private VecD movedDistance;
    private VecD initialPos;
    private bool isLmbPressed;
    private bool clickHandled;

    private Paint selectionPaint;
    private Paint opacityPaint;

    private int lastXMovementCursorIndex;

    static TextOverlay()
    {
        IsVisibleProperty.Changed.Subscribe(IsVisibleChanged);
        RequestEditTextProperty.Changed.Subscribe(RequestEditTextChanged);
        IsEditingProperty.Changed.Subscribe(IsEditingChanged);
        TextProperty.Changed.Subscribe(TextChanged);
        FontProperty.Changed.Subscribe(FontChanged);
        SpacingProperty.Changed.Subscribe(SpaceChanged);

        AffectsOverlayRender(FontProperty, TextProperty, CursorPositionProperty, SelectionEndProperty,
            IsEditingProperty,
            MatrixProperty, SpacingProperty);
    }

    public TextOverlay()
    {
        shortcuts = new Dictionary<KeyCombination, Action>
        {
            { new KeyCombination(Key.C, KeyModifiers.Control), () => CopyText() },
            { new KeyCombination(Key.C, KeyModifiers.Control | KeyModifiers.Shift), () => CopyText(true) },
            { new KeyCombination(Key.X, KeyModifiers.Control), CutText },
            { new KeyCombination(Key.V, KeyModifiers.Control), PasteText },
            { new KeyCombination(Key.Delete, KeyModifiers.None), () => DeleteChar(0) },
            { new KeyCombination(Key.Back, KeyModifiers.None), () => DeleteChar(-1) },
            { new KeyCombination(Key.Left, KeyModifiers.None), () => MoveCursorBy(new VecI(-1, 0)) },
            { new KeyCombination(Key.Right, KeyModifiers.None), () => MoveCursorBy(new VecI(1, 0)) },
            { new KeyCombination(Key.Up, KeyModifiers.None), () => MoveCursorBy(new VecI(0, -1)) },
            { new KeyCombination(Key.Down, KeyModifiers.None), () => MoveCursorBy(new VecI(0, 1)) },
            { new KeyCombination(Key.Left, KeyModifiers.Shift), () => MoveCursorBy(new VecI(-1, 0), false) },
            { new KeyCombination(Key.Right, KeyModifiers.Shift), () => MoveCursorBy(new VecI(1, 0), false) },
            { new KeyCombination(Key.Up, KeyModifiers.Shift), () => MoveCursorBy(new VecI(0, -1), false) },
            { new KeyCombination(Key.Down, KeyModifiers.Shift), () => MoveCursorBy(new VecI(0, 1), false) },
            {
                new KeyCombination(Key.Left, KeyModifiers.Control),
                () => MoveCursorBy(new VecI(-1, 0), mode: MoveMode.Words)
            },
            {
                new KeyCombination(Key.Right, KeyModifiers.Control),
                () => MoveCursorBy(new VecI(1, 0), mode: MoveMode.Words)
            },
            {
                new KeyCombination(Key.Up, KeyModifiers.Control),
                () => MoveCursorBy(new VecI(0, -1), mode: MoveMode.Words)
            },
            {
                new KeyCombination(Key.Down, KeyModifiers.Control),
                () => MoveCursorBy(new VecI(0, 1), mode: MoveMode.Words)
            },
            {
                new KeyCombination(Key.Left, KeyModifiers.Control | KeyModifiers.Shift),
                () => MoveCursorBy(new VecI(-1, 0), false, mode: MoveMode.Words)
            },
            {
                new KeyCombination(Key.Right, KeyModifiers.Control | KeyModifiers.Shift),
                () => MoveCursorBy(new VecI(1, 0), false, mode: MoveMode.Words)
            },
            {
                new KeyCombination(Key.Up, KeyModifiers.Control | KeyModifiers.Shift),
                () => MoveCursorBy(new VecI(0, -1), false, mode: MoveMode.Words)
            },
            {
                new KeyCombination(Key.Down, KeyModifiers.Control | KeyModifiers.Shift),
                () => MoveCursorBy(new VecI(0, 1), false, mode: MoveMode.Words)
            },
            { new KeyCombination(Key.Escape, KeyModifiers.None), () => IsEditing = false },
            { new KeyCombination(Key.A, KeyModifiers.Control), SelectAll },
            { new KeyCombination(Key.Home, KeyModifiers.None), () => GoToStartOfLine(true) },
            { new KeyCombination(Key.Home, KeyModifiers.Shift), () => GoToStartOfLine(false) },
            { new KeyCombination(Key.End, KeyModifiers.None), () => GoToEndOfLine(true) },
            { new KeyCombination(Key.End, KeyModifiers.Shift), () => GoToEndOfLine(false) },
        };

        AdjustShortcutsForOS();

        selectionPaint = new Paint()
        {
            Color = ThemeResources.SelectionFillColor.WithAlpha(255), Style = PaintStyle.Fill
        };

        opacityPaint = new Paint() { Color = Colors.White.WithAlpha(ThemeResources.SelectionFillColor.A) };
    }


    public override void RenderOverlay(Canvas context, RectD canvasBounds)
    {
        if (!IsEditing) return;

        int saved = context.Save();

        context.SetMatrix(context.TotalMatrix.Concat(Matrix));

        RenderCaret(context);
        RenderSelection(context);

        context.RestoreToCount(saved);
        Refresh();
    }

    protected override void OnOverlayLostFocus()
    {
        ShortcutController.UnblockShortcutExecution(nameof(TextOverlay));
    }

    protected override void OnOverlayGotFocus()
    {
        if (IsEditing)
        {
            ShortcutController.BlockShortcutExecution(nameof(TextOverlay));
        }
    }

    private void RenderCaret(Canvas context)
    {
        caret.CaretPosition = CursorPosition;
        caret.FontSize = Font.Size;
        caret.GlyphPositions = glyphPositions;
        caret.GlyphWidths = glyphWidths;
        caret.Offset = Position;

        caret.CaretWidth = 2f / (float)ZoomScale;
        caret.Render(context);
    }

    private void RenderSelection(Canvas context)
    {
        if (CursorPosition == SelectionEnd) return;

        int begin = Math.Min(CursorPosition, SelectionEnd);
        int end = Math.Max(CursorPosition, SelectionEnd);

        richText.IndexOnLine(CursorPosition, out int lineStart);

        RectD? currentLineBounds = null;
        int lastLine = lineStart;
        int saved = context.SaveLayer(opacityPaint);

        for (int i = begin; i <= end; i++)
        {
            richText.IndexOnLine(i, out int line);

            if (line != lastLine || i == end)
            {
                if (currentLineBounds != null)
                {
                    context.DrawRect(currentLineBounds.Value, selectionPaint);
                }

                currentLineBounds = null;
            }

            lastLine = line;

            double x = glyphPositions[i].X;
            double width = glyphWidths[i];
            VecD lineOffset = richText.GetLineOffset(line, Font);
            RectD selectionBounds =
                new RectD(new VecD(x, -Font.Size + lineOffset.Y), new VecD(width, Font.Size * 1.25f)).Offset(Position);
            if (currentLineBounds == null)
            {
                currentLineBounds = selectionBounds;
            }
            else
            {
                currentLineBounds = currentLineBounds.Value.Union(selectionBounds);
            }
        }

        context.RestoreToCount(saved);
    }

    public override bool TestHit(VecD point)
    {
        VecD mapped = Matrix.Invert().MapPoint(point);
        return richText != null && richText.MeasureBounds(Font).Offset(Position).Inflate(2).ContainsInclusive(mapped);
    }

    protected override void OnOverlayPointerPressed(OverlayPointerArgs args)
    {
        args.Handled = true;
        clickHandled = args.ClickCount == 2;
        if (args.ClickCount == 2)
        {
            SelectWordAtPosition(args.Point);
        }

        movedDistance = VecD.Zero;
        initialPos = args.Point;
        isLmbPressed = args.PointerButton == MouseButton.Left;
        args.Pointer.Capture(this);
    }

    protected override void OnOverlayPointerMoved(OverlayPointerArgs args)
    {
        movedDistance = args.Point - initialPos;
        if (isLmbPressed && !clickHandled)
        {
            if (movedDistance.Length > 2)
            {
                SetCursorPosToPosition(args.Point);
                SetSelectionEndToPosition(initialPos);
            }
        }
    }

    protected override void OnOverlayPointerReleased(OverlayPointerArgs args)
    {
        if (movedDistance.Length < 2 && !clickHandled)
        {
            if (args.InitialPressMouseButton == MouseButton.Left)
            {
                if (!IsEditing)
                {
                    IsEditing = true;
                }

                SetCursorPosToPosition(args.Point);
            }
        }

        isLmbPressed = false;
    }

    protected override void OnOverlayPointerEntered(OverlayPointerArgs args)
    {
        Cursor = new Cursor(StandardCursorType.Ibeam);
    }

    protected override void OnOverlayPointerExited(OverlayPointerArgs args)
    {
        Cursor = new Cursor(StandardCursorType.Arrow);
    }

    private void SetCursorPosToPosition(VecD point)
    {
        var indexOfClosest = GetClosestCharacterIndex(point);

        CursorPosition = indexOfClosest;
        SelectionEnd = indexOfClosest;
    }

    private void SetSelectionEndToPosition(VecD point)
    {
        var indexOfClosest = GetClosestCharacterIndex(point);
        SelectionEnd = indexOfClosest;
    }

    private void SelectAll()
    {
        CursorPosition = 0;
        SelectionEnd = Text.Length;
    }

    private void GoToStartOfLine(bool updateSelection)
    {
        richText.IndexOnLine(CursorPosition, out int lineIndex);
        int lineStart = richText.GetLineStartEnd(lineIndex).lineStart;
        CursorPosition = lineStart;
        if (updateSelection)
        {
            SelectionEnd = CursorPosition;
        }
    }

    private void GoToEndOfLine(bool updateSelection)
    {
        richText.IndexOnLine(CursorPosition, out int lineIndex);
        int lineEnd = richText.GetLineStartEnd(lineIndex).lineEnd - 1;
        CursorPosition = lineEnd;
        if (updateSelection)
        {
            SelectionEnd = CursorPosition;
        }
    }

    private void SelectWordAtPosition(VecD point)
    {
        var indexOfClosest = GetClosestCharacterIndex(point);
        int start = indexOfClosest;
        int end = indexOfClosest;

        while (start > 0 && !char.IsWhiteSpace(Text[start - 1]))
        {
            start--;
        }

        while (end < Text.Length - 1 && !char.IsWhiteSpace(Text[end + 1]))
        {
            end++;
        }

        CursorPosition = start;
        SelectionEnd = end + 1;
    }

    private void CopyText(bool asUnicode = false)
    {
        if (CursorPosition == SelectionEnd) return;
        string selectedText = Text.Substring(
            Math.Min(CursorPosition, SelectionEnd),
            Math.Abs(CursorPosition - SelectionEnd));

        if (asUnicode)
        {
            selectedText = string.Join(" ", selectedText.Select(c => $"U+{((int)c):X4}"));
        }

        ClipboardController.Clipboard.SetTextAsync(selectedText);
    }

    private void CutText()
    {
        CopyText();
        DeleteChar(0);
    }

    private int GetClosestCharacterIndex(VecD point)
    {
        VecD mapped = Matrix.Invert().MapPoint(point);
        var positions = richText.GetGlyphPositions(Font);
        int indexOfClosest = positions.Select((pos, index) => (pos, index))
            .OrderBy(pos => ((pos.pos + Position - new VecD(0, Font.Size / 2f)) - mapped).LengthSquared)
            .First().index;
        return indexOfClosest;
    }

    protected override void OnKeyPressed(Key key, KeyModifiers keyModifiers, string? keySymbol)
    {
        if (!IsEditing) return;

        ShortcutController.BlockShortcutExecution(nameof(TextOverlay));

        if (IsUndoRedoShortcut(key, keyModifiers))
        {
            ShortcutController.UnblockShortcutExecution(nameof(TextOverlay));
            return;
        }

        if (IsShortcut(key, keyModifiers))
        {
            ExecuteShortcut(key, keyModifiers);
            return;
        }

        InsertChar(key, keySymbol);
    }

    private bool IsUndoRedoShortcut(Key key, KeyModifiers keyModifiers)
    {
        return key == Key.Z && keyModifiers == KeyModifiers.Control ||
               key == Key.Y && keyModifiers == KeyModifiers.Control;
    }

    private void InsertChar(Key key, string symbol)
    {
        if (key == Key.Enter)
        {
            InsertTextAtCursor("\n");
        }
        else if (key == Key.Space)
        {
            InsertTextAtCursor(" ");
        }
        else
        {
            if (symbol is { Length: 1 })
            {
                char symbolChar = symbol[0];
                if (char.IsControl(symbolChar)) return;
                InsertTextAtCursor(symbol);
            }
        }
    }

    private void InsertTextAtCursor(string toAdd)
    {
        if (CursorPosition == SelectionEnd)
        {
            Text = Text.Insert(CursorPosition, toAdd);
            CursorPosition += toAdd.Length;
            SelectionEnd += toAdd.Length;
        }
        else
        {
            string newText = Text.Remove(Math.Min(CursorPosition, SelectionEnd),
                Math.Abs(CursorPosition - SelectionEnd));
            Text = newText.Insert(Math.Min(CursorPosition, SelectionEnd), toAdd);
            CursorPosition = Math.Min(CursorPosition, SelectionEnd) + toAdd.Length;
            SelectionEnd = CursorPosition;
        }

        lastXMovementCursorIndex = CursorPosition;
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
        ClipboardController.GetTextFromClipboard().ContinueWith(
            t =>
            {
                Dispatcher.UIThread.Invoke(() => InsertTextAtCursor(t.Result));
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
    }

    private void DeleteChar(int direction)
    {
        if (SelectionEnd != CursorPosition)
        {
            Text = Text.Remove(Math.Min(CursorPosition, SelectionEnd),
                Math.Abs(CursorPosition - SelectionEnd));
            CursorPosition = Math.Min(CursorPosition, SelectionEnd);
            SelectionEnd = CursorPosition;
        }
        else if (Text.Length > 0 && CursorPosition + direction >= 0 && CursorPosition + direction < Text.Length)
        {
            Text = Text.Remove(CursorPosition + direction, 1);
            CursorPosition += direction;
            SelectionEnd = CursorPosition;
        }

        lastXMovementCursorIndex = CursorPosition;
    }

    private void MoveCursorBy(VecI direction, bool updateSelection = true, MoveMode mode = MoveMode.Characters)
    {
        int moveBy = direction.X;
        if (direction.X != 0)
        {
            lastXMovementCursorIndex = Math.Clamp(CursorPosition + direction.X, 0, Text.Length);

            if (mode == MoveMode.Words)
            {
                string[] words = richText.FormattedText.Split(' ');
                int i = 0;
                int cursorPosInWord = 0;
                int wordIndex = 0;

                for (var index = 0; index < words.Length; index++)
                {
                    var word = words[index];
                    if (CursorPosition >= i && CursorPosition <= i + word.Length)
                    {
                        cursorPosInWord = CursorPosition - i;
                        wordIndex = index;
                        break;
                    }

                    i += word.Length + 1;
                }

                if (cursorPosInWord > 0 && cursorPosInWord < words[wordIndex].Length)
                {
                    if (moveBy < 0)
                    {
                        moveBy = -cursorPosInWord;
                    }
                    else
                    {
                        moveBy = words[wordIndex].Length - cursorPosInWord;
                    }
                }
                else
                {
                    int wordLength = words[wordIndex].Length;
                    if (wordLength > 0)
                    {
                        if (moveBy > 0 && cursorPosInWord == 0)
                        {
                            moveBy += wordLength - 1;
                        }
                        else if (moveBy < 0 && cursorPosInWord == wordLength)
                        {
                            moveBy -= words[wordIndex].Length - 1;
                        }
                    }
                }
            }
        }

        if (direction.Y != 0)
        {
            richText.IndexOnLine(CursorPosition, out int lineIndex);

            int clampedDesiredLineIndex = Math.Clamp(lineIndex + direction.Y, 0, richText.Lines.Length - 1);

            VecF position = glyphPositions[lastXMovementCursorIndex];
            (int lineStart, int lineEnd) = richText.GetLineStartEnd(clampedDesiredLineIndex);
            VecF[] lineGlyphPositions = glyphPositions[lineStart..lineEnd];
            int closestIndex = lineGlyphPositions.Select((pos, i) => (i, pos))
                .OrderBy(pos => Math.Abs(pos.pos.X - position.X)).First().i;
            moveBy = richText.GetIndexOnLine(clampedDesiredLineIndex, closestIndex) - CursorPosition;
        }

        CursorPosition += moveBy;
        if (updateSelection)
        {
            SelectionEnd = CursorPosition;
        }
    }

    private void RequestEditTextTriggered(object? sender, string e)
    {
        IsEditing = true;
        CursorPosition = glyphPositions.Length;
        SelectionEnd = CursorPosition;
    }

    private void UpdateGlyphs()
    {
        if (Font == null || Font.IsDisposed) return;

        richText = new(Text);
        richText.Spacing = Spacing;
        glyphPositions = richText.GetGlyphPositions(Font);
        glyphWidths = richText.GetGlyphWidths(Font);
    }

    private void AdjustShortcutsForOS()
    {
        if (IOperatingSystem.Current.IsMacOs)
        {
            Dictionary<KeyCombination, Action> newShortcuts = new();
            foreach (var shortcut in shortcuts)
            {
                if (shortcut.Key.Modifiers.HasFlag(KeyModifiers.Control))
                {
                    KeyModifiers newModifiers = shortcut.Key.Modifiers & ~KeyModifiers.Control;
                    newModifiers |= KeyModifiers.Meta;
                    newShortcuts.Add(new KeyCombination(shortcut.Key.Key, newModifiers), shortcut.Value);
                }
                else
                {
                    newShortcuts.Add(shortcut.Key, shortcut.Value);
                }
            }

            shortcuts = newShortcuts;
        }
    }

    private static void IsVisibleChanged(AvaloniaPropertyChangedEventArgs<bool> args)
    {
        TextOverlay sender = args.Sender as TextOverlay;
        if (sender == null) return;

        if (!args.NewValue.Value)
        {
            sender.IsEditing = false;
        }
    }

    private static void RequestEditTextChanged(AvaloniaPropertyChangedEventArgs<ExecutionTrigger<string>> args)
    {
        var sender = args.Sender as TextOverlay;
        if (args.OldValue.Value != null)
        {
            args.OldValue.Value.Triggered -= sender.RequestEditTextTriggered;
        }

        if (args.NewValue.Value != null)
        {
            args.NewValue.Value.Triggered += sender.RequestEditTextTriggered;
        }
    }

    private static void IsEditingChanged(AvaloniaPropertyChangedEventArgs<bool> args)
    {
        if (args.NewValue.Value)
        {
            ShortcutController.BlockShortcutExecution(nameof(TextOverlay));
            TextOverlay sender = args.Sender as TextOverlay;
            sender.UpdateGlyphs();

            if (sender.CursorPosition > sender.glyphPositions.Length)
            {
                sender.CursorPosition = sender.glyphPositions.Length;
            }

            if (sender.SelectionEnd > sender.glyphPositions.Length)
            {
                sender.SelectionEnd = sender.glyphPositions.Length;
            }

            sender.lastXMovementCursorIndex = sender.CursorPosition;

            sender.FocusOverlay();
        }
        else
        {
            ShortcutController.UnblockShortcutExecution(nameof(TextOverlay));
        }
    }

    private static void TextChanged(AvaloniaPropertyChangedEventArgs<string> args)
    {
        TextOverlay sender = args.Sender as TextOverlay;
        sender.UpdateGlyphs();
    }

    private static void FontChanged(AvaloniaPropertyChangedEventArgs<Font> args)
    {
        TextOverlay sender = args.Sender as TextOverlay;
        sender.UpdateGlyphs();
    }

    private static void SpaceChanged(AvaloniaPropertyChangedEventArgs<double?> args)
    {
        TextOverlay sender = args.Sender as TextOverlay;
        sender.UpdateGlyphs();
    }

    private static int ClampValue(AvaloniaObject sender, int newPos)
    {
        TextOverlay textOverlay = sender as TextOverlay;
        if (textOverlay == null) return newPos;
        if (textOverlay.Text == null) return 0;

        if (textOverlay.glyphPositions == null) return 0;

        return Math.Clamp(newPos, 0, textOverlay.glyphPositions.Length - 1);
    }
}

public enum MoveMode
{
    Characters,
    Words,
}
