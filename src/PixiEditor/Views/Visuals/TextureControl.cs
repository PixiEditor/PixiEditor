using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Interop.VulkanAvalonia.Controls;
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
    }

    private void TextureOnChanged(RectD? changedRect)
    {
        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Render);
    }
}
