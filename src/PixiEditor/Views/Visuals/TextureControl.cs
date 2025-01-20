using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using Drawie.Backend.Core;
using Drawie.Interop.Avalonia.Core.Controls;
using Drawie.Numerics;

namespace PixiEditor.Views.Visuals;

public class TextureControl : DrawieTextureControl
{
    public static readonly StyledProperty<IBrush> BackgroundProperty = AvaloniaProperty.Register<TextureControl, IBrush>
        (nameof(Background));


    public IBrush Background
    {
        get { return (IBrush)GetValue(BackgroundProperty); }
        set { SetValue(BackgroundProperty, value); }
    }

    public TextureControl()
    {
        ClipToBounds = true;
        TextureProperty.Changed.Subscribe(OnTextureChanged);
    }

    public override void Render(DrawingContext context)
    {
        if (Background != null)
        {
            context.FillRectangle(Background, new Rect(Bounds.Size));
        }

        base.Render(context);
    }

    private void OnTextureChanged(AvaloniaPropertyChangedEventArgs<Texture> args)
    {
        if (args.OldValue.Value != null)
        {
            args.OldValue.Value.Changed -= TextureOnChanged;
        }

        if (args.NewValue.Value != null)
        {
            args.NewValue.Value.Changed += TextureOnChanged;
        }
        
        QueueNextFrame();
    }

    private void TextureOnChanged(RectD? changedRect)
    {
        QueueNextFrame();
    }
}
