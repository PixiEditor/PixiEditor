using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Position;
using Drawie.Numerics;
using PixiEditor.Models.Rendering;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Views.Main.ViewportControls;

internal partial class FixedViewport : UserControl, INotifyPropertyChanged
{
    public static readonly StyledProperty<DocumentViewModel> DocumentProperty =
        AvaloniaProperty.Register<FixedViewport, DocumentViewModel>(nameof(Document), null);

    public static readonly StyledProperty<bool> DelayedProperty =
        AvaloniaProperty.Register<FixedViewport, bool>(nameof(Delayed), false);

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool Delayed
    {
        get => GetValue(DelayedProperty);
        set => SetValue(DelayedProperty, value);
    }

    public DocumentViewModel? Document
    {
        get => GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    public Guid GuidValue { get; } = Guid.NewGuid();

    static FixedViewport()
    {
        DocumentProperty.Changed.Subscribe(OnDocumentChange);
    }

    public FixedViewport()
    {
        InitializeComponent();
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

        if (oldDoc != null)
        {
            oldDoc.SizeChanged -= viewport.DocSizeChanged;
        }
        if (newDoc != null)
        {
            newDoc.SizeChanged += viewport.DocSizeChanged;
        }
    }

    private void DocSizeChanged(object? sender, DocumentSizeChangedEventArgs e)
    {
        Document?.Operations.AddOrUpdateViewport(GetLocation());
    }

    private void OnImageSizeChanged(object sender, SizeChangedEventArgs e)
    {
        Document?.Operations.AddOrUpdateViewport(GetLocation());
    }
}

