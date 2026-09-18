using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Input;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.Models.Handlers;
using PixiEditor.Views.Overlays.Handles;
using Canvas = Drawie.Backend.Core.Surfaces.Canvas;

namespace PixiEditor.Views.Overlays.ContextualOptions;

public class ContextualOptionsOverlay : Overlay
{
    public static readonly StyledProperty<VecD> PositionProperty = AvaloniaProperty.Register<ContextualOptionsOverlay, VecD>(
        nameof(Position));

    public static readonly StyledProperty<ObservableCollection<ContextualOption>> OptionsProperty = AvaloniaProperty.Register<ContextualOptionsOverlay, ObservableCollection<ContextualOption>>(
        nameof(Options));

    public ObservableCollection<ContextualOption> Options
    {
        get => GetValue(OptionsProperty);
        set => SetValue(OptionsProperty, value);
    }

    public VecD Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    private Dictionary<ContextualOption, ButtonHandle> optionHandles = new Dictionary<ContextualOption, ButtonHandle>();

    private Paint buttonPaint = new Paint
    {
        Color = Colors.Gray,
        Style = PaintStyle.Fill,
    };

    static ContextualOptionsOverlay()
    {
        OptionsProperty.Changed.AddClassHandler<ContextualOptionsOverlay>((o, e) => o.OnOptionsChanged(e as AvaloniaPropertyChangedEventArgs<ObservableCollection<ContextualOption>>));
    }

    private void OnOptionsChanged(AvaloniaPropertyChangedEventArgs<ObservableCollection<ContextualOption>> avaloniaPropertyChangedEventArgs)
    {
        ContextualOptionsOverlay overlay = (ContextualOptionsOverlay)avaloniaPropertyChangedEventArgs.Sender;
        overlay.Handles.Clear();
        optionHandles.Clear();
        foreach (ContextualOption option in avaloniaPropertyChangedEventArgs.NewValue.Value)
        {
            ButtonHandle handle = new ButtonHandle(overlay, option.ExecuteCommand)
            {
                Icon = option.Icon,
                HitTestVisible = true,
            };

            overlay.AddHandle(handle);
            optionHandles[option] = handle;
        }

        avaloniaPropertyChangedEventArgs.OldValue.Value?.CollectionChanged -= overlay.OnOptionsCollectionChanged;
        avaloniaPropertyChangedEventArgs.NewValue.Value?.CollectionChanged += overlay.OnOptionsCollectionChanged;
    }

    protected override void OnRenderOverlay(Canvas context, RectD canvasBounds)
    {
        const int IconButtonSize = 24;
        const int Spacing = 4;
        const float AnchorRadius = 4f;

        double scaleMultiplier = (1.0 / ZoomScale);
        double radius = AnchorRadius * scaleMultiplier;

        VecD size = new VecD(IconButtonSize * Options.Count + Spacing * (Options.Count - 1), IconButtonSize) * scaleMultiplier;
        RectD bg = new RectD(Position.X, Position.Y - size.Y / 2f, size.X, size.Y);

        context.DrawRoundRect((float)bg.X, (float)bg.Y, (float)bg.Width, (float)bg.Height, (float)radius, (float)radius, buttonPaint);
        double xOffset = Position.X + (IconButtonSize * scaleMultiplier) / 2f;
        foreach (var option in Options)
        {
            VecD pos = new VecD(xOffset, Position.Y);
            DrawIconButton(context, option, pos);
            xOffset += (IconButtonSize + Spacing) * scaleMultiplier;
        }
    }

    private void DrawIconButton(Canvas target, ContextualOption option, VecD pos)
    {
        optionHandles[option].Position = pos;
        optionHandles[option].Draw(target);
    }

    private void OnOptionsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            foreach (ContextualOption option in e.NewItems!)
            {
                ButtonHandle handle = new ButtonHandle(this, option.ExecuteCommand)
                {
                    Icon = option.Icon,
                    HitTestVisible = true,
                    Cursor = new Cursor(StandardCursorType.Hand),
                };

                AddHandle(handle);
                optionHandles[option] = handle;
            }
        }
        else if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
        {
            foreach (ContextualOption option in e.OldItems!)
            {
                if(optionHandles.TryGetValue(option, out var handle))
                {
                    RemoveHandle(handle);
                    optionHandles.Remove(option);
                }
            }
        }
    }
}
