using System.Collections.Generic;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using ChunkyImageLib.DataHolders;
using PixiEditor.AvaloniaUI.Models.Position;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Views.Main.ViewportControls;

internal partial class FixedViewport : UserControl, INotifyPropertyChanged
{
    public static readonly StyledProperty<DocumentViewModel> DocumentProperty =
        AvaloniaProperty.Register<FixedViewport, DocumentViewModel>(nameof(Document), null);

    private static readonly StyledProperty<Dictionary<ChunkResolution, WriteableBitmap>> BitmapsProperty =
        AvaloniaProperty.Register<FixedViewport, Dictionary<ChunkResolution, WriteableBitmap>>(nameof(Bitmaps), null);

    public static readonly StyledProperty<bool> DelayedProperty =
        AvaloniaProperty.Register<FixedViewport, bool>(nameof(Delayed), false);

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool Delayed
    {
        get => GetValue(DelayedProperty);
        set => SetValue(DelayedProperty, value);
    }

    public Dictionary<ChunkResolution, WriteableBitmap>? Bitmaps
    {
        get => GetValue(BitmapsProperty);
        set => SetValue(BitmapsProperty, value);
    }

    public DocumentViewModel? Document
    {
        get => GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    public WriteableBitmap? TargetBitmap
    {
        get
        {
            if (Document?.LazyBitmaps.TryGetValue(CalculateResolution(), out WriteableBitmap? value) == true)
                return value;
            return null;
        }
    }

    public Guid GuidValue { get; } = Guid.NewGuid();

    static FixedViewport()
    {
        DocumentProperty.Changed.Subscribe(OnDocumentChange);
        BitmapsProperty.Changed.Subscribe(OnBitmapsChange);
    }

    public FixedViewport()
    {
        InitializeComponent();
        Binding binding = new Binding { Source = this, Path = $"{nameof(Document)}.{nameof(Document.LazyBitmaps)}" };
        this.Bind(BitmapsProperty, binding);
        Loaded += OnLoad;
        Unloaded += OnUnload;
    }

    private void OnUnload(object sender, RoutedEventArgs e)
    {
        Document?.Operations.RemoveViewport(GuidValue);
    }

    private void OnLoad(object sender, RoutedEventArgs e)
    {
        Document?.Operations.AddOrUpdateViewport(GetLocation());
    }

    private ChunkResolution CalculateResolution()
    {
        if (Document is null)
            return ChunkResolution.Full;
        double density = Document.Width / mainImage.Bounds.Width;
        if (density > 8.01)
            return ChunkResolution.Eighth;
        else if (density > 4.01)
            return ChunkResolution.Quarter;
        else if (density > 2.01)
            return ChunkResolution.Half;
        return ChunkResolution.Full;
    }

    private void ForceRefreshFinalImage()
    {
        mainImage.InvalidateVisual();
    }

    private ViewportInfo GetLocation()
    {
        VecD docSize = new VecD(1);
        if (Document is not null)
            docSize = Document.SizeBindable;

        return new ViewportInfo(
            0,
            docSize / 2,
            new VecD(mainImage.Bounds.Width, mainImage.Bounds.Height),
            docSize,
            CalculateResolution(),
            GuidValue,
            Delayed,
            ForceRefreshFinalImage);
    }

    private static void OnDocumentChange(AvaloniaPropertyChangedEventArgs<DocumentViewModel> args)
    {
        DocumentViewModel? oldDoc = args.OldValue.Value;
        DocumentViewModel? newDoc = args.NewValue.Value;
        FixedViewport? viewport = (FixedViewport)args.Sender;
        oldDoc?.Operations.RemoveViewport(viewport.GuidValue);
        newDoc?.Operations.AddOrUpdateViewport(viewport.GetLocation());
    }

    private static void OnBitmapsChange(AvaloniaPropertyChangedEventArgs<Dictionary<ChunkResolution, WriteableBitmap>> args)
    {
        FixedViewport? viewport = (FixedViewport)args.Sender;
        viewport.PropertyChanged?.Invoke(viewport, new(nameof(TargetBitmap)));
        viewport.Document?.Operations.AddOrUpdateViewport(viewport.GetLocation());
    }

    private void OnImageSizeChanged(object sender, SizeChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, new(nameof(TargetBitmap)));
        //Document?.Operations.AddOrUpdateViewport(GetLocation()); //TODO: This causes deadlock
    }
}

